using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;

namespace Phoni {
namespace PhoniCore {
	
	public class PhoniClient {
	
		private PhoniTCPClient tcpClient;
		private PhoniUDPClient udpClient;
		private bool udpRedirected;
		
		
		private Dictionary<NetworkInfo, TcpPackageBuffer> commandBuffer = new Dictionary<NetworkInfo, TcpPackageBuffer>();
		
		public PhoniTCPClient TcpClient {
			get {
				return tcpClient;
			}
		}
		
		public PhoniUDPClient UdpClient {
			get {
				return udpClient;
			}
		}
		
		public bool IsConnected  {
			get {
				return udpRedirected && tcpClient.IsConnected && udpClient.IsConnected;
			}
		}
		
		public NetworkInfo LocalNetworkInfo {
			get {
				return new NetworkInfo(tcpClient.LocalAddress, tcpClient.LocalPort);
			}
		}
		
		public NetworkInfo RemoteNetworkInfo {
			get {
				return new NetworkInfo(tcpClient.RemoteAddress, tcpClient.RemotePort);
			}
		}
		
		public event PhoniUDPClient.ReadCallback UdpReadEvent {
			add {
				udpClient.readEvent += value;
			}
			remove {
				udpClient.readEvent -= value;
			}
		}
		
		public event PhoniUDPClient.WriteCallback UdpWriteEvent {
			add {
				udpClient.writeEvent += value;
			}
			remove {
				udpClient.writeEvent -= value;
			}
		}
		
		public delegate void TcpConnectCallback(bool isConnected, PhoniClient phoniClient);
		public event TcpConnectCallback TcpConnectEvent;
		
		public delegate void TcpReadCallback(uint command, uint package, byte[] data, NetworkInfo info);
		public event TcpReadCallback TcpReadEvent;
		
		public delegate void LostPhoniConnectionCallback(PhoniClient client);
		public event LostPhoniConnectionCallback lostConnectionEvent;
		
		
		
		public PhoniClient(string remoteIP, int remotePort, TcpConnectCallback connectCallback) {
			TcpConnectEvent += connectCallback;
			Init ();
			tcpClient.Connect(remoteIP, remotePort);
			udpClient.Connect(0);		
		}
			
		public PhoniClient(string remoteIP, int remotePort) : this(remoteIP, remotePort, null){
		}
			
		public PhoniClient(NetworkInfo remoteInfo) : this(remoteInfo, null) {			
		}
		
		public PhoniClient(NetworkInfo remoteInfo, TcpConnectCallback connectCallback) : 
			this(remoteInfo.ipAddress, remoteInfo.port, connectCallback) {			
		}
			
		public PhoniClient(TcpClient client) {
			Init ();
			tcpClient.SetClient(client);
			udpClient.Connect(0);
		}
			
		~PhoniClient() {
			Close();
		}
			
		private void Init() {
			tcpClient = new PhoniTCPClient();
			udpClient = new PhoniUDPClient();
			tcpClient.connectEvent += TcpConnectHandler;
			tcpClient.readEvent += ParseCommand;
			tcpClient.lostConnectionEvent += HandleLostConnection;
			udpClient.lostConnectionEvent += HandleLostConnection;
			udpRedirected = false;
		}
		
		public void UdpRedirect(string remoteIP, int remotePort) {
			udpClient.RedirectRemoteInfo(remoteIP, remotePort);
			udpRedirected = true;
		}
		
		public void Close() {
			if(tcpClient != null) {
				tcpClient.Close();
				tcpClient = null;
			}
			if(udpClient != null) {
				udpClient.Close();
				udpClient = null;
			}
		}
		
		/*
		public void Stop() {
			if(tcpClient != null) {
				tcpClient.StopReaderThread();
			}
			if(udpClient != null) {
				udpClient.StopReaderThread();
				udpClient.StopWriterThread();
			}
		}
		*/
			
			
		private void TcpConnectHandler(bool isConnected, IPEndPoint remote) {
			if(TcpConnectEvent != null) {
				TcpConnectEvent(isConnected, this);
			}
		}
			
		private void HandleLostConnection(PhoniTCPClient client) {
			if(lostConnectionEvent != null) {
				lostConnectionEvent(this);
			}
		}
			
		private void HandleLostConnection(PhoniUDPClient client) {
			if(lostConnectionEvent != null) {
				lostConnectionEvent(this);
			}
		}
		
		private void ParseCommand(byte[] buffer, IPEndPoint ipEndPoint) {
			NetworkInfo remoteInfo = new NetworkInfo(ipEndPoint);
			if(!commandBuffer.ContainsKey(remoteInfo)) {
				commandBuffer[remoteInfo] = new TcpPackageBuffer();
			}
			TcpPackageBuffer package = commandBuffer[remoteInfo];
			package.buffer.AddRange(buffer);
			byte[] data;
			if(package.dataLen > 0) {
				if(!GetCommandData(package, out data)) {
					return;
				}
				//fire event;
				if(TcpReadEvent != null) {
					TcpReadEvent(package.commandCode, package.packageCode, data, remoteInfo);
				}
			}
			else {
				while(CheckHeader(package) && GetCommandData(package, out data)) {
					//fire event;
					if(TcpReadEvent != null) {
						TcpReadEvent(package.commandCode, package.packageCode, data, remoteInfo);
					}
				}
			}
			
		}
		
		private bool CheckHeader(TcpPackageBuffer package) {
			bool result = false;
			int headerLen = PhoniHeader.Length;
			if(package.buffer.Count >= headerLen+4) {
				byte[] array = package.buffer.ToArray();
				int i = 0;
				for(i=0; i+headerLen+4 <= array.Length; ++i) {
					PhoniHeader header = PhoniHeader.ParseBytes(array, i);
					if(header != null) {
						package.commandCode = header.contentCode;
						package.packageCode = header.packageCode;
						package.dataLen = BitConverter.ToUInt32(array, i+headerLen);
						i = i+headerLen+4;
						result = true;
						break;
					}
				}
				//clean up list
				if(i >= package.buffer.Count) {
					package.buffer.Clear();
				}
				else {
					package.buffer = package.buffer.GetRange(i, package.buffer.Count-i);
				}
				
			}
			return result;
		}
		
		private bool GetCommandData(TcpPackageBuffer package, out byte[] data) {
			if(package.dataLen == 0) {
				data = null;
				return true;
			}
			else if(package.buffer.Count == package.dataLen) {
				data = package.buffer.ToArray();
				package.buffer.Clear();		
				package.dataLen = 0;
				return true;
			}
			else if(package.buffer.Count > package.dataLen) {
				data = package.buffer.GetRange(0, (int)package.dataLen).ToArray();
				package.buffer = package.buffer.GetRange((int)package.dataLen, package.buffer.Count - (int)package.dataLen);
				package.dataLen = 0;
				return true;
			}
			else {
				data = null;
				return false;
			}
		}
		
	}
	
	public class TcpPackageBuffer {
		public uint commandCode;
		public uint packageCode;
		public uint dataLen;
		public List<byte> buffer;
		
		public TcpPackageBuffer() {
			commandCode = 0;
			packageCode = 0;
			dataLen = 0;
			buffer = new List<byte>();
		}
	}
		
} // namespace PhoniCore
} // namespace Phoni
