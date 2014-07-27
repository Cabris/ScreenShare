using System;
using System.Collections.Generic;
using System.Collections;
using System.Net.Sockets;
using System.Net;
using System.Threading;

namespace Phoni {
namespace PhoniCore {
	
	public class PhoniUDPClient {
		
		//shared
		protected UdpClient _udpClient;
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
        protected ReaderWriterLock _readRwl;	
		protected ManualResetEvent _readingMRE;
		
		public delegate void ReadCallback(byte[] buffer, IPEndPoint ipEndPoint);
		public event ReadCallback readEvent;
		
		public delegate void LostUDPConnectionCallback(PhoniUDPClient client);
		public event LostUDPConnectionCallback lostConnectionEvent;
		
		//write
		protected Thread _writerThread;
        protected uint _writerThreadExit;
        protected bool _writing;
        protected ReaderWriterLock _writeRwl;	
		protected ManualResetEvent _writingMRE;
		
		public delegate List<byte[]> WriteCallback();
		public event WriteCallback writeEvent;
		
        protected class UdpState
        {
            public IPEndPoint e;
            public UdpClient u;
        }		
		
		public PhoniUDPClient()
        {
            _udpClient = null;
			_remoteAddress = IPAddress.Any;
			_remotePort = 0;
			_isConnected = false;
			
			_writerThread = new Thread(new ThreadStart(PhoniUDPClientThreadedWriteThreadStart));
            _writeRwl = new ReaderWriterLock();
            _writerThreadExit = 0;
            _writing = false;
			_writingMRE = new ManualResetEvent(true);
			
            _readerThread = new Thread(new ThreadStart(PhoniUDPClientThreadedReadThreadStart));
            _readRwl = new ReaderWriterLock();
            _readerThreadExit = 0;
            _reading = false;
			_readingMRE = new ManualResetEvent(true);
        }
		
		~PhoniUDPClient()
		{
			Close();
		}
		
		public void Connect(string server, int port, int localPort)
        {
			_remoteAddress = IPAddress.Parse(server);
			_remotePort = port;
			_localPort = localPort;
			Connect();			
        }
		
		public void Connect(string server, int port)
        {
			_remoteAddress = IPAddress.Parse(server);
			_remotePort = port;
			_localPort = 0;
			Connect();			
        }
		
		public void Connect(int localPort) {
			_remoteAddress = IPAddress.Any;
			_remotePort = 0;
			_localPort = localPort;
			Connect();
		}
		
		private void Connect() {
			_udpClient = new UdpClient();
			//_udpClient.ExclusiveAddressUse = false;
			_udpClient.Client.SetSocketOption(SocketOptionLevel.Socket,SocketOptionName.ReuseAddress,true);
			_localAddress = PhoniUtil.GetIPAddress();
			_udpClient.Client.Bind(new IPEndPoint(_localAddress, _localPort));
			_localPort = ((IPEndPoint)_udpClient.Client.LocalEndPoint).Port;
			//_udpClient.Connect(server, port);
            PhoniLog.Log("Initial recieve buffer size: " + _udpClient.Client.ReceiveBufferSize);
            _udpClient.Client.ReceiveBufferSize = 655360; // 640 KB
            PhoniLog.Log("Expanded recieve buffer size " + _udpClient.Client.ReceiveBufferSize);
			_isConnected = true;
		}
		
		public void RedirectRemoteInfo(string server, int port) {
			_remoteAddress = IPAddress.Parse(server);
			_remotePort = port;
		}
		
		public UdpClient GetClient() {
			return _udpClient;
		}
		
		public void Close()
        {
			StopReaderThread();
			StopWriterThread();
			if(_udpClient != null) {
            	_udpClient.Close();
				_udpClient = null;
			}
			_isConnected = false;
			PhoniLog.Log("Close");
        }
		
		
		
		#region read part
		
		protected void ReceiveCallback(IAsyncResult ar)
        {
			
            UdpClient u = (UdpClient)((UdpState)(ar.AsyncState)).u;
            IPEndPoint e = (IPEndPoint)((UdpState)(ar.AsyncState)).e;
            byte[] buffer = null;
            try
            {
                buffer = u.EndReceive(ar, ref e);
				//PhoniOutput.WriteLine("back");
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
            
			if(readEvent != null) {
				readEvent(buffer, e);
			}
            
            _readRwl.ReleaseWriterLock();
            _reading = false;
			_readingMRE.Set();
        }
		
			
		protected void PhoniUDPClientThreadedReadThreadStart()
        {
            //IPEndPoint remoteEP = new IPEndPoint(_serverAddress, _port);
			IPEndPoint remoteEP = new IPEndPoint(IPAddress.Any, 0);
            UdpState s = new UdpState();
            while (_readerThreadExit == 0)
            {
                s.e = remoteEP;
                s.u = _udpClient;
                _reading = true;
				_readingMRE.Reset();
                try
                {
                    if (_udpClient != null) {
						//PhoniOutput.WriteLine("StartReceive");
                        _udpClient.BeginReceive(new AsyncCallback(ReceiveCallback), s);
					}
					else {
						_reading = false;
						_readingMRE.Set();
						Thread.Sleep(0);
					}
                } catch (System.Exception e)
                {
                    // socket closed exception
					IsConnected =false;
                    PhoniLog.Error(e.Message);
                }
					
				_readingMRE.WaitOne();
            }
			
			
        }
			
		public void AquireReadReaderLock() {
			_readRwl.AcquireReaderLock(-1);
		}
			
		public void ReleaseReadReaderLock() {
			_readRwl.ReleaseReaderLock();
		}
			
		public bool IsReadWriterLockHeld() {
			return _readRwl.IsWriterLockHeld;
		}
		
		/*
		public void AquireReadWriterLock() {
			_readRwl.AcquireWriterLock(-1);
		}
			
		public void ReleaseReadWriterLock() {
			_readRwl.ReleaseWriterLock();
		}
		*/

        public void StartReaderThread()
        {
			PhoniLog.Log("udp read thread started");
            _readerThread.Start();
        }

        public void StopReaderThread ()
        {
            _readerThreadExit = 1;
			_readingMRE.Set();
			if(_readerThread.IsAlive)
            	_readerThread.Join();
			
			PhoniLog.Log("udp Stop Read");
        }
			
		#endregion
		
		#region write part
		
		protected void SendCallback(IAsyncResult ar)
        {
            UdpClient u = (UdpClient)ar.AsyncState;
            //PhoniOutput.WriteLine("sending");
            try
            {
                u.EndSend(ar);
            }
			catch(System.Exception e) {
				IsConnected =false;
				PhoniLog.Error(e.Message);
				_writing = false;
				_writingMRE.Set();
                return;
			}
			//PhoniOutput.WriteLine("sent");
            _writing = false;
			_writingMRE.Set();
        }
		
		
		protected void PhoniUDPClientThreadedWriteThreadStart()
        {
            UdpClient u = new UdpClient();
            while (_writerThreadExit == 0)
            {
                u = _udpClient;
                _writing = true;
				_writingMRE.Reset();
				List<byte[]> packages = new List<byte[]>();
				_writeRwl.AcquireReaderLock(-1);
				if(writeEvent != null) {
					packages = writeEvent();
				}
				_writeRwl.ReleaseReaderLock();
				foreach(byte[] data in packages) {
					try
	                {
	                    if (_udpClient != null) {
	                        //_udpClient.BeginSend(data, data.Length, new AsyncCallback(SendCallback), u);
							_udpClient.BeginSend(data, data.Length, new IPEndPoint(_remoteAddress, _remotePort),
								new AsyncCallback(SendCallback), u);
						}
						else {
							_writing = false;
							_writingMRE.Set();
							Thread.Sleep(0);
						}
	                } catch (System.Exception e)
	                {
						IsConnected =false;
	                    // socket closed exception
	                    PhoniLog.Error(e.Message);
	                }
						
					_writingMRE.WaitOne();
	                
					Thread.Sleep(5);
				}
				
            }
        }
		
		
		public void AquireWriteWriterLock() {
			_writeRwl.AcquireWriterLock(-1);
		}
			
		public void ReleaseWriteWriterLock() {
			_writeRwl.ReleaseWriterLock();
		}
			
		public bool IsWriteReaderLockHeld() {
			return _writeRwl.IsReaderLockHeld;
		}
		
		/*	
		public void AquireWriteReaderLock() {
			_writeRwl.AcquireReaderLock(-1);
		}
			
		public void ReleaseWriteReaderLock() {
			_writeRwl.ReleaseReaderLock();
		}
		*/

        public void StartWriterThread()
        {
            _writerThread.Start();
        }

        public void StopWriterThread ()
        {
            _writerThreadExit = 1;
			_writingMRE.Set();
			if(_writerThread.IsAlive) {
           		_writerThread.Join();
			}
			PhoniLog.Log("Stop Write");
        }
		
		#endregion
		
	}
} // namespace PhoniCore
} // namespace Phoni
