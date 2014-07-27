using System;
using System.Collections.Generic;
using System.Collections;
using System.Net.Sockets;
using System.Net;
using System.Threading;

namespace Phoni {
namespace PhoniCore {
	
	public class PhoniTCPListener {
		
		//shared
		protected TcpListener _tcpListener;
		protected IPAddress _localAddress;
		protected int _listenPort;
		
		
		public IPAddress LocalAddress {
			get {
				return _localAddress;
			}
		}
		
		public int ListenPort {
			get {
				return _listenPort;
			}
		}
		//read
		protected Thread _readerThread;
        protected uint _readerThreadExit;
        protected bool _reading;
		protected ManualResetEvent _readingMRE;
        protected ReaderWriterLock _readRwl;	
		
		public delegate void ReadCallback(TcpClient client);
		public event ReadCallback readEvent;
		
		
		public PhoniTCPListener()
        {
			_tcpListener = null;
			
            _readerThread = new Thread(new ThreadStart(PhoniTCPClientThreadedReadThreadStart));
            _readRwl = new ReaderWriterLock();
            _readerThreadExit = 0;
            _reading = false;
			_readingMRE = new ManualResetEvent(true);
        }
		
		~PhoniTCPListener()
		{
			_readerThreadExit = 1;
			_readingMRE.Set();

            try
            {
                _readerThread.Join();
            }
            catch (System.Exception)
            {
            	
            }         
			
		}
		
		
		public void Connect(int listenPort) {
			_listenPort = listenPort;
			Connect();			
		}
		
		
		private void Connect() {
			_localAddress = PhoniUtil.GetIPAddress();
			_tcpListener = new TcpListener(new IPEndPoint(_localAddress, _listenPort));
			_listenPort = ((IPEndPoint)_tcpListener.LocalEndpoint).Port;
		}
		
		
		public TcpListener GetListener() {
			return _tcpListener;
		}
		
		public void Close()
        {
			if(_tcpListener!=null) {
				_tcpListener.Stop();
			}
			PhoniLog.Log("TCP listener Close");
        }
		
		
		
		#region read part
		
		protected void ReceiveCallback(IAsyncResult ar)
        {
			
            TcpListener listener = (TcpListener)ar.AsyncState;
			TcpClient client = null;
            try
            {
                client = listener.EndAcceptTcpClient(ar);
            }
            catch (System.Exception exp)
            {
				PhoniLog.Error(exp.Message);
                _reading = false;
				_readingMRE.Set();
                return;
            }
			
            _readRwl.AcquireWriterLock(-1);
            
			//register client
			if(readEvent != null) {
				readEvent(client);
			}
			
            _readRwl.ReleaseWriterLock();
            _reading = false;
			_readingMRE.Set();
        }
		
			
		public void PhoniTCPClientThreadedReadThreadStart()
        {
			//start reading
			try {
				_tcpListener.Start();
			} catch (SocketException e) {
				PhoniLog.Error(e.Message);
				PhoniLog.Log("use a port assigned by the system.");
				Connect(0);
				_tcpListener.Start();
			}
			_listenPort = ((IPEndPoint)_tcpListener.LocalEndpoint).Port;
			
            while (_readerThreadExit == 0)
            {
                _reading = true;
				_readingMRE.Reset();
                try
                {
                    if (_tcpListener != null) {
						PhoniLog.Log("tcp listener StartReceive");
                        _tcpListener.BeginAcceptTcpClient(new AsyncCallback(ReceiveCallback), _tcpListener);
					}
                } catch (System.Exception e)
                {
                    // socket closed exception
					PhoniLog.Error(e.Message);
                }
				_readingMRE.WaitOne();
            }
			
			
        }
			

        public void StartReaderThread()
        {
			PhoniLog.Log("read thread started");
            _readerThread.Start();
        }

        public void StopReaderThread ()
        {
            _readerThreadExit = 1;
			_readingMRE.Set();
            _readerThread.Join();
			
			PhoniLog.Log("Stop Read");
        }
			
		#endregion
		
	}
} // namespace PhoniCore
} // namespace Phoni
