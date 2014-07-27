using System;
using System.Threading;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using Phoni.PhoniCore;

/**
 * 
 * Phoni System
 * a network framework to make standalone device into game controllers.
 * 
 * Developed by Xun "Eric" Zhang (lxjk001@gmail.com)
 * 2013.3.14
 * 
 **/

namespace Phoni {

	public static class PhoniPlayerController {
	
		// local platform
		private static PhoniPlatformCode platform = PhoniPlatformCode.PLATFORM_NONE;
				
		// internal use
		private static bool isInit = false;
		private static Dictionary<NetworkInfo, ConnectThreadData> connectThreadDic = new Dictionary<NetworkInfo, ConnectThreadData>();
		
		public static void Init(PhoniPlatformCode platformCode) {
			platform = platformCode;
		}
		
		public static void PlayerClientConnect(string serverAddress, int serverPort)
	    {
			
			NetworkInfo info = new NetworkInfo(serverAddress, serverPort);
			if(!PhoniInput.Game.Contains(info)) {
				// create and connect phoni client
				PhoniClient phoniClient = new PhoniClient(info, ConnectCallback);
			}
			
			if(!isInit) {
				// only add delegate once
				PhoniInput.Game.CommandEvent += ConnectCommandHandler;	
				isInit = true;
			}
	
	    }
		
		private static void ConnectCommandHandler(PhoniDataPort dataPort, PhoniCommandInfo commandInfo) {
			// connect command comes when the game received connect command we sent out and respounded.
			if(commandInfo.command == PhoniCommandCode.COMMAND_CONNECT) {
				NetworkInfo remoteInfo = dataPort.RemoteNetworkInfo;
				// stop the connect thread we create for this game
				if(connectThreadDic.ContainsKey(remoteInfo)) {
					connectThreadDic[remoteInfo].isRespond = true;
					if(connectThreadDic[remoteInfo].connectThread.IsAlive) {
						connectThreadDic[remoteInfo].connectThread.Join();
					}
					connectThreadDic.Remove(remoteInfo);
				}
				
				PhoniClient client = dataPort.NetworkClient;
				
				if(!client.IsConnected) {
					PhoniConnectData connectData = commandInfo.data.GetData<PhoniConnectData>();
					// redirect udp info
					client.UdpRedirect(connectData.ipAddress, connectData.port);
					// set remote platform
					dataPort.SetRemotePlatformOnce((PhoniPlatformCode)connectData.platformCode);
					PhoniLog.Log("player receive udp: " + connectData.ipAddress + " : " +
						connectData.port);
					// setup udp event and start
					client.UdpWriteEvent += delegate() {
						if(PhoniInput.Game[dataPort.ID] != null) {
							return PhoniInput.Game[dataPort.ID].Packages;
						}
						return null;
					};
					client.UdpReadEvent += delegate(Byte[] buffer, IPEndPoint ipEndPoint) {
						InputData(buffer);
					};
					client.UdpClient.StartReaderThread();
					client.UdpClient.StartWriterThread();
				}
			}
		}
				
		// connect call back happens when our tcp client connected to game listener, not CONNECT command
		private static void ConnectCallback(bool isConnected, PhoniClient phoniClient) {
			
			NetworkInfo info = phoniClient.RemoteNetworkInfo;
			
			if(!isConnected) {
				PhoniLog.Error("Connection Failed " + info);
				return;
			}
			//add connection and start receive command from tcp
			phoniClient.TcpReadEvent += delegate(uint command, uint package, byte[] data, NetworkInfo remoteInfo) {
				PhoniInput.Game.InputReceivedCommand(remoteInfo, command, package, data);
			};
			phoniClient.lostConnectionEvent += HandleLostConnection;
			PhoniInput.Game.AddConnection( platform, phoniClient);
			phoniClient.TcpClient.StartReaderThread();
			
			// start connect thread to constantly send connect command to the game until the game respond
			if(!connectThreadDic.ContainsKey(info)) {
				connectThreadDic[info] = new ConnectThreadData();
				connectThreadDic[info].connectThread = new Thread(new ParameterizedThreadStart(ConnectThreadStart));
				connectThreadDic[info].connectThread.Start(phoniClient);
			}
				
			PhoniLog.Log("player udp : " + phoniClient.UdpClient.LocalAddress + " : " + 
				phoniClient.UdpClient.LocalPort);
		}
		
		private static void ConnectThreadStart(object client) {
			PhoniClient phoniClient = (PhoniClient)client;
			NetworkInfo info = phoniClient.RemoteNetworkInfo;
			while(connectThreadDic.ContainsKey(info) && !connectThreadDic[info].isRespond) {
				// send out connect command
				PhoniInput.Game[info].SendCommand(PhoniCommandCode.COMMAND_CONNECT,
					new PhoniData<PhoniConnectData>(
						new PhoniConnectData(phoniClient.UdpClient.LocalAddress.ToString(), 
							phoniClient.UdpClient.LocalPort,
							(uint)platform)));
				
				
				Thread.Sleep(5);
			}
		}
		
		private static void HandleLostConnection(PhoniClient client) {
			PhoniLog.Log("Disconnect " + client.RemoteNetworkInfo);
			// for lost connection, hack send a COMMAND_LOST_CONNECTION command
			PhoniInput.Game.InputReceivedCommand(client.RemoteNetworkInfo,
				PhoniCommandCode.COMMAND_LOST_CONNECTION,
				PhoniPackageCode.PACKAGE_NONE,
				null);
		}
		
		public static void PlayerClientDisconnect(string serverAddress, int serverPort)
	    {
			NetworkInfo info = new NetworkInfo(serverAddress, serverPort);
			// remove connected game
			if(PhoniInput.Game.Contains(info)) {
				PhoniInput.Game.Remove(info);
			}
			// stop connecting if not connected
			if(connectThreadDic.ContainsKey(info)) {
				connectThreadDic[info].isRespond = true;
				if(connectThreadDic[info].connectThread.IsAlive) {
					connectThreadDic[info].connectThread.Join();
				}
				connectThreadDic.Remove(info);
			}
	    }
		
	    public static void PlayerClientDisconnectAll()
	    {
			PhoniInput.Game.Clear();
			// clear all connecting thread
			foreach(ConnectThreadData connectThreadData in connectThreadDic.Values) {
				connectThreadData.isRespond = true;
				if(connectThreadData.connectThread.IsAlive) {
					connectThreadData.connectThread.Join();
				}
			}
			connectThreadDic.Clear();
	    }
		
		// receive data from udp
		private static void InputData(byte[] data) {
			PhoniHeader header = PhoniHeader.ParseBytes(data);
			if(header == null) {
				return;
			}
			int headerLen = PhoniHeader.Length;
			byte[] pureData = PhoniUtil.GetSubArray(data, headerLen, data.Length - headerLen);
			PhoniInput.Game.InputReceivedData(header.sourceNetworkInfo, header.contentCode, pureData);
			
		}	
		
	}
	
	public class ConnectThreadData {
		public Thread connectThread;
		public bool isRespond;
	}
	
} // namespace Phoni