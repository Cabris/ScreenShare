using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.Serialization.Formatters.Binary;
using System.Net;
using Phoni.PhoniCore;

namespace Phoni {
	
	public class PhoniDataPort {	
		
	#region Phoni Data Region
				
		[PhoniData(PhoniDataCode.DATA_PHONI_TOUCH)]
		public PhoniDataWrapper<PhoniTouchData, PhoniTouchDataSerializer> TouchData {get; private set;}
		
		[PhoniData(PhoniDataCode.DATA_PHONI_MOTION)]
		public PhoniDataWrapper<PhoniMotionData, PhoniMotionDataSerializer> MotionData {get; private set;}
		
		[PhoniData(PhoniDataCode.DATA_PHONI_ANALOG)]
		public PhoniDataWrapper<PhoniAnalogData, PhoniAnalogDataSerializer> AnalogData {get; private set;}
				
		[PhoniData(PhoniDataCode.DATA_PHONI_BUTTON)]
		public PhoniDataWrapper<PhoniButtonData> ButtonData {get; private set;}

		[PhoniData(PhoniDataCode.MY_STRING_DATA, SendingCase.SendToPlayer)]
		public PhoniDataWrapper<string> MyString{get; private set;}


	#endregion	
		
		protected bool _isActive = false;
		
		protected PhoniClient _networkClient = null;
		
		protected int _id = 0;
		
		protected bool _isPlayer = false;
		
		protected PhoniPlatformCode _localPlatform = PhoniPlatformCode.PLATFORM_NONE;
		protected PhoniPlatformCode _remotePlatform = PhoniPlatformCode.PLATFORM_NONE;
		
		protected Dictionary<PhoniDataCode, PhoniDataWrapperBase> codeMap = new Dictionary<PhoniDataCode, PhoniDataWrapperBase>();
		
		/// <summary>
		/// The sending codes. 
		/// For all the code in this set, the associated data will be sent out via UDP automatically.
		/// </summary>
		public HashSet<PhoniDataCode> sendingCodes = new HashSet<PhoniDataCode>();	
		
		protected Queue<PhoniCommandInfo> _commandQueue = new Queue<PhoniCommandInfo>();
				
		public bool IsActive {
			get {
				return _isActive;
			}
		}
		
		public PhoniClient NetworkClient {
			get {
				return _networkClient;
			}
		}
		
		public NetworkInfo LocalNetworkInfo {
			get {
				return NetworkClient.LocalNetworkInfo;
			}
		}
		
		public string LocalIPAddress {
			get {
				return LocalNetworkInfo.ipAddress;
			}
		}
		
		public int LocalPort {
			get {
				return LocalNetworkInfo.port;
			}
		}
		
		public NetworkInfo RemoteNetworkInfo {
			get {
				return NetworkClient.RemoteNetworkInfo;
			}
		}
		
		public string RemoteIPAddress {
			get {
				return RemoteNetworkInfo.ipAddress;
			}
		}
		
		public int RemotePort {
			get {
				return RemoteNetworkInfo.port;
			}
		}
		
		public int ID {
			get {
				return _id;
			}
		}
		
		public bool IsPlayerPort {
			get {
				return _isPlayer;
			}
		}
		
		public bool IsGamePort {
			get {
				return !_isPlayer;
			}			
		}
		
		public PhoniPlatformCode LocalPlatform {
			get {
				return _localPlatform;
			}
		}
		
		public PhoniPlatformCode RemotePlatform {
			get {
				return _remotePlatform;
			}
		}
				
		public PhoniDataPort(int id, bool isPlayer, PhoniClient client, PhoniPlatformCode localPlatform){
			_id = id;
			_isPlayer = isPlayer;
			_networkClient = client;
			ReflectPhoniData();
			_localPlatform = localPlatform;
			_isActive = true;
		}
		
		~PhoniDataPort() {
			Close();
		}
				
		protected virtual void ReflectPhoniData() {
			foreach(PropertyInfo pi in this.GetType().GetProperties()) {
				object[] att = pi.GetCustomAttributes(typeof(PhoniDataAttribute), false);
				if(att.Length > 0) {
					Type wrapperType = pi.PropertyType;
					pi.SetValue(this, Activator.CreateInstance(wrapperType), null);
					PhoniDataWrapperBase pdwb = (PhoniDataWrapperBase)pi.GetValue(this, null);
					PhoniDataCode code = ((PhoniDataAttribute)att[0]).code;
					codeMap[code] = pdwb;
					switch(((PhoniDataAttribute)att[0]).sendingCase) {
					case SendingCase.SendToBoth:
						sendingCodes.Add(code);
						break;
					case SendingCase.SendToGame:
						if(IsGamePort) {
							sendingCodes.Add(code);
						}
						break;
					case SendingCase.SendToPlayer:
						if(IsPlayerPort) {
							sendingCodes.Add(code);
						}
						break;
					}
				}
			}
		}
		
		
		public virtual void SetActive(bool active) {
			_isActive = active;
		}
		
		public virtual void SetNetworkClient(PhoniClient client) {
			_networkClient = client;
		}
		
		public bool SetRemotePlatformOnce(PhoniPlatformCode remotePlatform) {
			if(_remotePlatform != PhoniPlatformCode.PLATFORM_NONE) {
				return false;
			}
			_remotePlatform = remotePlatform;
			return true;
		}
		
		public virtual bool SendCommand(PhoniCommandCode command) {
			return SendCommand(command, null);
		}
		
		public virtual bool SendCommand(PhoniCommandCode command, PhoniDataBase data) {
			if(NetworkClient != null) {
				byte[] serializedData= new byte[0];
				if(data != null) {
					serializedData = data.Serialize();
				}
				List<byte> dataList = new List<byte>();
				uint packageCode = (uint)PhoniPackageCode.PACKAGE_NONE;
				if(data != null) {
					packageCode = data.GetCode();
				}
				PhoniHeader header  =new PhoniHeader(LocalNetworkInfo, packageCode, (uint)command);
				dataList.AddRange(header.ToBytes());
				dataList.AddRange(BitConverter.GetBytes((uint)serializedData.Length));
				dataList.AddRange(serializedData);
				
				return NetworkClient.TcpClient.SendRequest(dataList.ToArray());
			}
			return false;
		}
		
		/// <summary>
		/// Internal Use.
		/// </summary>
		internal virtual void QueueReceiveCommand(PhoniCommandInfo info){
			_commandQueue.Enqueue(info);
		}
		
		/// <summary>
		/// Pull out the command queue and clean up the queue afterwards.
		/// This method will acquire TCP read thread's writer lock.
		/// The query will be skipped if TCP read thread's writer lock is held,
		/// which means TCP thread is enqueuing new commands.
		/// </summary>
		/// <returns>
		/// The command queue.
		/// </returns>
		public Queue<PhoniCommandInfo> UseCommandQueue() {
			if(_networkClient.TcpClient.IsReadWriterLockHeld()) {
				return new Queue<PhoniCommandInfo>();
			}
			Queue<PhoniCommandInfo> queue;
			_networkClient.TcpClient.AquireReadWriterLock();
			queue = _commandQueue;
			_commandQueue = new Queue<PhoniCommandInfo>();
			_networkClient.TcpClient.ReleaseReadWriterLock();
			return queue;
			
		}
		
		/// <summary>
		/// Internal Use.
		/// </summary>
		internal virtual void InputReceivedData(PhoniDataCode code, byte[] data) {
			codeMap[code].InputReceivedData(data);
		}
		
		/// <summary>
		/// Internal Use.
		/// </summary>
		internal virtual void InputReceivedData(uint code, byte[] data) {
			InputReceivedData((PhoniDataCode)code, data);
		}
		
		/// <summary>
		/// Internal Use.
		/// </summary>
		internal List<byte[]> Packages {
			get {
				List<byte[]> packages = new List<byte[]>();
				
				foreach(PhoniDataCode code in sendingCodes) {					
					PhoniDataBase data = codeMap[code].GetInternalOutputData();
					if(data != null) {
						packages.Add(FillInPackage(code, data));
					}					
				}
				
				return packages;
			}
		}
		
		protected virtual byte[] FillInPackage(PhoniDataCode code, PhoniDataBase content) {
			List<byte> dataList = new List<byte>();
			// header
			PhoniHeader header = new PhoniHeader(LocalNetworkInfo, (uint)content.GetCode(), 
				(uint)code);
			dataList.AddRange(header.ToBytes());
			// data
			dataList.AddRange(content.Serialize());
			return dataList.ToArray();
		}
				
		/// <summary>
		/// Updates all received data.
		/// This method will acquire UDP read thread's reader lock.
		/// The update will be skipped if UDP read thread's writer lock is held,
		/// which means the UDP thread is inputting data.
		/// </summary>
		public void UpdateAllReceivedData() {
			if(codeMap.Count == 0 || _networkClient.UdpClient.IsReadWriterLockHeld()) {
				return;
			}
			_networkClient.UdpClient.AquireReadReaderLock();
			foreach(PhoniDataWrapperBase phoniWrapper in codeMap.Values) {
				phoniWrapper.UpdateReceivedData();
			}
			_networkClient.UdpClient.ReleaseReadReaderLock();
		}
		
		/// <summary>
		/// Flushs all sending data.
		/// This method will acquire TCP write thread's writer lock.
		/// The flush will be skipped if UDP write thread's reader lock is held,
		/// which means the UDP thread is retrieving data.
		/// </summary>
		public void FlushAllSendingData() {
			if(sendingCodes.Count == 0 || _networkClient.UdpClient.IsWriteReaderLockHeld()) {
				return;
			}
			_networkClient.UdpClient.AquireWriteWriterLock();
			foreach(PhoniDataCode code in sendingCodes) {
				codeMap[code].FlushSendingData();
			}
			_networkClient.UdpClient.ReleaseWriteWriterLock();
		}
		
		/// <summary>
		/// Close this port without sending out DISCONNECT command.
		/// Recommend using "Disconnect()" instead.
		/// </summary>
		public void Close() {
			if(_networkClient != null) {
				_networkClient.Close();
				_networkClient = null;
			}
		}
		
		/// <summary>
		/// Send out DISCONNECT command and close the port.
		/// </summary>
		public void Disconnect() {
			SendCommand(PhoniCommandCode.COMMAND_DISCONNECT);
			Close();
		}
		
	}
	
	public class NetworkInfo {
		
		public int port;
		public string ipAddress {
			get {
				return _ipAddress.ToString();
			}
			set {
				_ipAddress = IPAddress.Parse(value);
			}
		}
		public byte[] ipBytes {
			get {
				return _ipAddress.GetAddressBytes();
			}
			set {
				_ipAddress = new IPAddress(value);
			}
		}
		
		private IPAddress _ipAddress;
		
		public NetworkInfo() {
			ipAddress = "0.0.0.0";
			port = 0;
		}
		
		public NetworkInfo(string ipAddress, int port) {
			this.ipAddress = ipAddress;
			this.port = port;
		}
		
		public NetworkInfo(IPAddress ipAddress, int port) {
			this._ipAddress = ipAddress;
			this.port = port;
		}
		
		public NetworkInfo(IPEndPoint ipEndPoint) {
			this._ipAddress = ipEndPoint.Address;
			this.port = ipEndPoint.Port;
		}
		
		public override bool Equals (object obj)
		{
			if(obj == null) {
				return false;
			}
			NetworkInfo info = obj as NetworkInfo;
			if((System.Object)info == null) {
				return false;
			}
			return (ipAddress == info.ipAddress) && (port == info.port);
		}
		
		public bool Equals (NetworkInfo info) {
			if((System.Object)info == null) {
				return false;
			}
			return (ipAddress == info.ipAddress) && (port == info.port);
		}
		
		public override int GetHashCode ()
		{
			return ipAddress.GetHashCode() ^ port.GetHashCode();
		}
		
		public override string ToString ()
		{
			return string.Format ("{0}:{1}", ipAddress, port);
		}
		
		public bool Parse(string info) {
			string[] infos = info.Trim().Split(':');
			if(infos.Length == 2) {
				ipAddress = infos[0].Trim();
				port = int.Parse(infos[1].Trim());
				return true;
			}
			return false;
		}
		
		static public bool operator == (NetworkInfo a, NetworkInfo b) {
			if(System.Object.ReferenceEquals((System.Object)a,(System.Object)b)) {
				return true;
			}
			if( (System.Object)a == null || (System.Object)b == null) {
				return false;
			}
			return (a.ipAddress == b.ipAddress) && (a.port == b.port);
		}
		
		static public bool operator != (NetworkInfo a, NetworkInfo b) {
			return !(a == b);
		}
	}
	
} // namespace Phoni