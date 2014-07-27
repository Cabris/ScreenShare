using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
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

	public static class PhoniGameController {
		
		// listen for player connections
		private static PhoniTCPListener welcomeClient;
		// local platform
		private static PhoniPlatformCode platform = PhoniPlatformCode.PLATFORM_NONE;
		
		public static string GameIPAddress {
			get {
				return welcomeClient.LocalAddress.ToString();
			}
		}
		
		public static int GamePort {
			get {
				return welcomeClient.ListenPort;
			}
		}
		
		public static bool IsStarted {
			get {
				return isStarted;
			}		
		}
		
		private static bool isStarted = false;	
		// internal use
		private static bool isInit = false;	
		
		public static void Init(PhoniPlatformCode platformCode) {
			platform = platformCode;
		}
		
		public static bool GameClientStart() {
			return GameClientStart(0);
		}
				
		public static bool GameClientStart(int listenPort)
	    {
			
			if(isStarted) {
				return false;
			}
			
			if(!isInit) {
				// only add delegate once
				PhoniInput.Player.CommandEvent += ConnectCommandHandler;				
				isInit = true;
			}
			
	            
			welcomeClient = new  PhoniTCPListener();
			welcomeClient.readEvent += ReceiveConnection;
	
	        try
	        {
				welcomeClient.Connect(listenPort);
				welcomeClient.StartReaderThread();
	        }
	        catch(Exception e)
	        {
				PhoniLog.Error(e.Message);
	            return false;
	        }
			
			isStarted = true;
			
			return true;
	
	    }
		
	    public static void GameClientShutdown() {
			if(welcomeClient == null) {
				return;
			}
			
	        try
	        {	
				welcomeClient.StopReaderThread();
				welcomeClient.Close();
				PhoniInput.Player.Clear();
				welcomeClient = null;
	        }
	        catch
	        {
	            return;
	        }
			
			isStarted = false;
	    }
		
		private static void ConnectCommandHandler(PhoniDataPort dataPort, PhoniCommandInfo commandInfo) {
			// the connect command comes after player connected to welcome cient and send out connect request
			if(commandInfo.command == PhoniCommandCode.COMMAND_CONNECT) {
				PhoniClient client = dataPort.NetworkClient;
				if(!client.IsConnected) {
					PhoniConnectData connectData = commandInfo.data.GetData<PhoniConnectData>();
					// redirect udp info
					client.UdpRedirect(connectData.ipAddress, connectData.port);
					// set remote platform
					dataPort.SetRemotePlatformOnce((PhoniPlatformCode)connectData.platformCode);
					// send out connect request with local platform and udp info
					dataPort.SendCommand(PhoniCommandCode.COMMAND_CONNECT,
						new PhoniData<PhoniConnectData>(
							new PhoniConnectData(
							dataPort.NetworkClient.UdpClient.LocalAddress.ToString(), 
							dataPort.NetworkClient.UdpClient.LocalPort,
							(uint)platform)));
					PhoniLog.Log("server receive udp: " + connectData.ipAddress + " : " +
							connectData.port);
					// setup udp event and start udp thread
					client.UdpReadEvent += delegate(Byte[] buffer, IPEndPoint ipEndPoint) {
						InputData(buffer);
					};
					client.UdpWriteEvent += delegate() {
						if(PhoniInput.Player[dataPort.ID] != null) {
							return PhoniInput.Player[dataPort.ID].Packages;
						}
						return null;
					};
					client.UdpClient.StartReaderThread();
					client.UdpClient.StartWriterThread();
				}
			}
		}
		
		// receive a tcp connection, not CONNECT command.
		private static void ReceiveConnection(TcpClient client) {
			NetworkInfo info = new NetworkInfo((IPEndPoint)client.Client.RemoteEndPoint);
			if(!PhoniInput.Player.Contains(info)) {
				PhoniClient phoniClient = new PhoniClient(client);
				phoniClient.TcpReadEvent += delegate(uint command, uint package, byte[] data, NetworkInfo remoteInfo) {
					PhoniInput.Player.InputReceivedCommand(remoteInfo, command, package, data);
				};
				phoniClient.lostConnectionEvent += HandleLostConnection;
				//add connection and start receive command from tcp
				PhoniInput.Player.AddConnection(platform, phoniClient);
				PhoniLog.Log("server udp : " + phoniClient.UdpClient.LocalAddress + " : " + 
					phoniClient.UdpClient.LocalPort);
				phoniClient.TcpClient.StartReaderThread();
					
			}
		}
		
		private static void HandleLostConnection(PhoniClient client) {
			PhoniLog.Log("Disconnect " + client.RemoteNetworkInfo);
			// for lost connection, hack send a COMMAND_LOST_CONNECTION command
			PhoniInput.Player.InputReceivedCommand(client.RemoteNetworkInfo,
				PhoniCommandCode.COMMAND_LOST_CONNECTION,
				PhoniPackageCode.PACKAGE_NONE,
				null);
		}
		
		// receive data from udp
		private static void InputData(byte[] data) {
			PhoniHeader header = PhoniHeader.ParseBytes(data);
			if(header == null) {
				return;
			}
			int headerLen = PhoniHeader.Length;
			byte[] pureData = PhoniUtil.GetSubArray(data, headerLen, data.Length - headerLen);
			PhoniInput.Player.InputReceivedData(header.sourceNetworkInfo, header.contentCode, pureData);
			
		}	
	}
	
} // namespace Phoni