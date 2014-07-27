using System;
using System.Collections.Generic;
using System.Collections;
using System.Net.Sockets;
using System.Net;
using System.Threading;

namespace Phoni {
namespace PhoniCore {
	
	public class PhoniTCPClient {
		
		//shared
		protected TcpClient _tcpClient;
		protected IPAddress _remoteAddress;
		protected IPAddress _localAddress;
		protected int _remotePort;
		protected int _localPort;
		protected bool _isConnected;
		
		public IPAddress RemoteAddress {
			get {
				return _remoteAddress;
			}
		}
		
		public IPAddress LocalAddress {
			get {
				return _localAddress;
			}
		}
		public int RemotePort {
			get {
				return _remotePort;
			}
		}
		public int LocalPort {
			get {
				return _localPort;
			}
		}
		
		public bool IsConnected {
			get {
				return _isConnected;
			}
			private set {
				if(_isConnected != value) {
					_isConnected = value;
					if(!_isConnected && lostConnectionEvent != null) {
						lostConnectionEvent(this);
					}
				}
			}
		}
		//read
		protected Thread _readerThread;
        protected uint _readerThreadExit;
        protected bool _reading;
		protected ManualResetEvent _readingMRE;
        protected ReaderWriterLock _readRwl;	
			
		public delegate void EndConnectCallback(bool isConnected, IPEndPoint remote);
		public event EndConnectCallback connectEvent;
		
		public delegate void ReadCallback(byte[] buffer, IPEndPoint ipEndPoint);
		public event ReadCallback readEvent;
		
		public delegate void LostTCPConnectionCallback(PhoniTCPClient client);
		public event LostTCPConnectionCallback lostConnectionEvent;
		
        protected class TcpState
        {
            public NetworkStream stream;
            public byte[] buffer;
			public IPEndPoint e;
        }		
		
		public PhoniTCPClient()
        {
            _tcpClient = null;
			_remoteAddress = IPAddress.Any;
			_remotePort = 0;
			_isConnected = false;
			
            _readerThread = new Thread(new ThreadStart(PhoniTCPClientThreadedReadThreadStart));
            _readRwl = new ReaderWriterLock();
            _readerThreadExit = 0;
            _reading = false;
			_readingMRE = new ManualResetEvent(true);
        }
		
		~PhoniTCPClient()
		{
			Close();   			
		}
		
		
		public void Connect(string server, int port)
        {
			_remoteAddress = IPAddress.Parse(server);
			_remotePort = port;
			_localPort = 0;
			Connect();			
        }
				
		public void Connect(string server, int port, int localPort)
        {
			_remoteAddress = IPAddress.Parse(server);
			_remotePort = port;
			_localPort = localPort;
			Connect();			
        }
		
		
		private void Connect() {
			_localAddress = PhoniUtil.GetIPAddress();
			_tcpClient = new TcpClient(new IPEndPoint(_localAddress, _localPort));
			_localPort = ((IPEndPoint)_tcpClient.Client.LocalEndPoint).Port;
			//_tcpClient.NoDelay = true;
			if(_remotePort != 0) {
				_tcpClient.BeginConnect(_remoteAddress, _remotePort, new AsyncCallback(ConnectCallback), _tcpClient);
			}
		}
		
		public void RedirectRemoteInfo(string server, int port) {
			_remoteAddress = IPAddress.Parse(server);
			_remotePort = port;
			_tcpClient.BeginConnect(_remoteAddress, _remotePort, new AsyncCallback(ConnectCallback), _tcpClient);

		}
		
		protected void ConnectCallback(IAsyncResult ar) {
			TcpClient client = (TcpClient)ar.AsyncState;
			try {
				client.EndConnect(ar);
				_isConnected = _tcpClient.Connected;
				if(connectEvent != null) {
					connectEvent(_isConnected, (IPEndPoint)_tcpClient.Client.RemoteEndPoint);
				}
				PhoniLog.Log("Connect state: "+_tcpClient.Connected);
			}
			catch (System.Exception e) {
				PhoniLog.Error(e.Message);
			}
		}
		
		
		public TcpClient GetClient() {
			return _tcpClient;
		}
		
		public void SetClient(TcpClient client) {
			_tcpClient = client;
			_remoteAddress = ((IPEndPoint)_tcpClient.Client.RemoteEndPoint).Address;
			_remotePort = ((IPEndPoint)_tcpClient.Client.RemoteEndPoint).Port;
			_localAddress = ((IPEndPoint)_tcpClient.Client.LocalEndPoint).Address;
			_localPort = ((IPEndPoint)_tcpClient.Client.LocalEndPoint).Port;
			_isConnected = true;
		}
				
		public void Close()
        {
			StopReaderThread();
			if(_tcpClient!=null) {
            	_tcpClient.Close();
				_tcpClient = null;
			}
			_isConnected = false;
			PhoniLog.Log("TCP Close");
        }
		
		
		
		#region read part
		
		protected void ReceiveCallback(IAsyncResult ar)
        {
			
            NetworkStream stream = ((TcpState)ar.AsyncState).stream;
            byte[] buffer = ((TcpState)ar.AsyncState).buffer;
			IPEndPoint e = ((TcpState)ar.AsyncState).e;
			int readBytes = 0;
            try
            {
				//read
				readBytes = stream.EndRead(ar);
				PhoniLog.Log("TCP read back");
            }
            catch (System.Exception exp)
            {
				PhoniLog.Error(exp.Message);
				IsConnected =false;
                _reading = false;
				_readingMRE.Set();
                return;
            }
			
            _readRwl.AcquireWriterLock(-1);
            
			if(readBytes > 0 && readEvent != null) {
				readEvent(PhoniUtil.GetSubArray(buffer, 0, readBytes), e);
			}
			else {
				IsConnected = false;
			}
				
            _readRwl.ReleaseWriterLock();
            _reading = false;
			_readingMRE.Set();
			
        }
		
			
		public void PhoniTCPClientThreadedReadThreadStart()
        {
			//start reading
			TcpState s = new TcpState();
            while (_readerThreadExit == 0)
            {
                _reading = true;
				_readingMRE.Reset();
                try
                {
                    if (IsConnected) {
						PhoniLog.Log("TCP StartReceive");
                        NetworkStream stream =  _tcpClient.GetStream();
						s.stream = stream;
						s.buffer = new byte[1024];
						s.e = (IPEndPoint)_tcpClient.Client.RemoteEndPoint;
						stream.BeginRead(s.buffer, 0, s.buffer.Length, new AsyncCallback(ReceiveCallback), s);
					}
					else {
						_reading = false;
						_readingMRE.Set();
						Thread.Sleep(0);
					}
                } catch (System.Exception e)
                {
					IsConnected =false;
                    // socket closed exception
					PhoniLog.Error(e.Message);
                }
					
				_readingMRE.WaitOne();
            }
			
			
        }
			
		public void AquireReadWriterLock() {
			_readRwl.AcquireWriterLock(-1);
		}
			
		public void ReleaseReadWriterLock() {
			_readRwl.ReleaseWriterLock();
		}
			
		public bool IsReadWriterLockHeld() {
			return _readRwl.IsWriterLockHeld;
		}

        public void StartReaderThread()
        {
			PhoniLog.Log("tcp read thread started");
            _readerThread.Start();
        }

        public void StopReaderThread ()
        {
            _readerThreadExit = 1;
			_readingMRE.Set();
            _readerThread.Join();
			
			PhoniLog.Log("tcp Stop Read");
        }
			
		#endregion
		
		#region write part
		
		protected void SendCallback(IAsyncResult ar)
        {
            NetworkStream stream = (NetworkStream)ar.AsyncState;
            PhoniLog.Log("tcp sending");
            try
            {
                stream.EndWrite(ar);
            }
			catch(System.Exception e) {
				IsConnected = false;
				PhoniLog.Error(e.Message);
			}
			PhoniLog.Log("tcp sent");
        }
		
		
		public bool SendRequest(byte[] data)
        {
			if(IsConnected) {
				NetworkStream stream = _tcpClient.GetStream();
				stream.BeginWrite(data, 0, data.Length, SendCallback, stream);
				//stream.Write(data, 0, data.Length);
				return true;
			}
			return false;
        }
		#endregion
		
	}
		
} // namespace PhoniCore
} // namespace Phoni
