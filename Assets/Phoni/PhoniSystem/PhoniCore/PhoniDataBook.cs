using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Collections.Generic;
using System.Reflection;

namespace Phoni {
namespace PhoniCore {
	
	public class PhoniDataBook : IEnumerable<PhoniDataPort> {
		private int nextID = 0;
			
		private bool isPlayer = false;
		
		private Dictionary<NetworkInfo, int> _remoteInfoToIDBook = new Dictionary<NetworkInfo, int>();
		private Dictionary<int, PhoniDataPort> _idToInputBook = new Dictionary<int, PhoniDataPort>();
		private Queue<int> _reusableIdQueue = new Queue<int>();

		internal event PhoniCommandCallback CommandEvent ;
			
		public PhoniDataBook(bool isPlayer) {
			this.isPlayer = isPlayer;
		}
						
		public PhoniDataPort this[int id] {
			get {
				if(_idToInputBook.ContainsKey(id)) {
					return _idToInputBook[id];
				}
				return null;
			}
		}
		
		public PhoniDataPort this[NetworkInfo info] {
			get {
				if(_remoteInfoToIDBook.ContainsKey(info)) {
					return _idToInputBook[_remoteInfoToIDBook[info]];
				}
				return null;
			}
		}
		
		/// <summary>
		/// Gets the count of connected players/games.
		/// </summary>
		/// <value>
		/// The count.
		/// </value>
		public int Count {
			get {
				return _idToInputBook.Count;
			}
		}
		
		public IEnumerator<PhoniDataPort> GetEnumerator ()
		{
			return _idToInputBook.Values.GetEnumerator();
		}
		
		System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator ()
		{
			return GetEnumerator();
		}	
		
		/// <summary>
		/// Gets the port list by activity. The list is sorted in ascending order of the playerID.
		/// </summary>
		/// <returns>
		/// The port list by activity.
		/// </returns>
		/// <param name='isActive'>
		/// Is active.
		/// </param>
		public List<PhoniDataPort> GetPortListByActivity(bool isActive) {
			List<PhoniDataPort> ports = new List<PhoniDataPort>();
			foreach(PhoniDataPort port in _idToInputBook.Values) {
				if(port.IsActive == isActive) {
					ports.Add(port);
				}
			}
			ports.Sort(delegate(PhoniDataPort x, PhoniDataPort y) {
				return x.ID.CompareTo(y.ID);
			});
			return ports;
		}
			
		/// <summary>
		/// Gets the port list. The list is sorted in ascending order of the playerID.
		/// </summary>
		/// <returns>
		/// The port list.
		/// </returns>
		public List<PhoniDataPort> GetPortList() {
			List<PhoniDataPort> ports = new List<PhoniDataPort>(_idToInputBook.Values);
			ports.Sort(delegate(PhoniDataPort x, PhoniDataPort y) {
					return x.ID.CompareTo(y.ID);
				});
			return ports;
		}
			
			
		public bool Contains(NetworkInfo remoteInfo) {
			return _remoteInfoToIDBook.ContainsKey(remoteInfo);
		}
		
		public bool Contains(int id) {
			return _idToInputBook.ContainsKey(id);
		}
		
		/// <summary>
		/// Remove and disconnect specified player/game.
		/// </summary>
		/// <param name='remoteInfo'>
		/// <c>true</c> if successed, <c>false</c> if the player/game with this info does not exist.
		/// </param>
		public bool Remove(NetworkInfo remoteInfo) {
			if(_remoteInfoToIDBook.ContainsKey(remoteInfo)) {
				return Remove(_remoteInfoToIDBook[remoteInfo]);
			}
			return false;
		}
		
		/// <summary>
		/// Remove and disconnect specified player/game.
		/// </summary>
		/// <param name='id'>
		/// <c>true</c> if successed, <c>false</c> if the player/game with this id does not exist.
		/// </param>
		public bool Remove(int id) {
			if(_idToInputBook.ContainsKey(id)){
				_remoteInfoToIDBook.Remove(_idToInputBook[id].RemoteNetworkInfo);
				_idToInputBook[id].Close();
				_idToInputBook.Remove(id);
				_reusableIdQueue.Enqueue(id);
				return true;
			}
			return false;
		}
			
		/// <summary>
		/// Remove and disconnect all players/games.
		/// </summary>
		public void Clear() {
			List<PhoniDataPort> portList = new List<PhoniDataPort>(_idToInputBook.Values);
			foreach(PhoniDataPort port in portList) {
				port.Close();
			}
			_idToInputBook.Clear();
			_remoteInfoToIDBook.Clear();
			_reusableIdQueue.Clear();
			nextID = 0;
		}
		
		/// <summary>
		/// Internal Use.
		/// </summary>
		internal int AddConnection(PhoniPlatformCode localPlatform, PhoniClient client) {
			if(Contains(client.RemoteNetworkInfo)) {
				this[client.RemoteNetworkInfo].SetNetworkClient(client);
				return _remoteInfoToIDBook[client.RemoteNetworkInfo];
			}
			else {
				_remoteInfoToIDBook[client.RemoteNetworkInfo] = GetNextID();
				_idToInputBook[GetNextID()] = new PhoniDataPort(GetNextID(), isPlayer, client, localPlatform);
				PhoniLog.Log("ADD ID " + GetNextID() + " " + _idToInputBook[GetNextID()]);
				return UseNextID();
			}
		}
		
		/// <summary>
		/// Internal Use.
		/// Input data from UDP thread
		/// </summary>
		internal void InputReceivedData(NetworkInfo info, PhoniDataCode code, byte[] data) {
			//_idToInputBook[AddNetworkInfo (info)].InputReceivedData(code, data);
			if(Contains(info)) {
				this[info].InputReceivedData(code, data);
			}
		}
			
		/// <summary>
		/// Internal Use.
		/// Input data from UDP thread
		/// </summary>
		internal void InputReceivedData(NetworkInfo info, uint code, byte[] data) {
			InputReceivedData(info, (PhoniDataCode)code, data);
		}
			
		/// <summary>
		/// Internal Use.
		/// Input command from TCP thread
		/// </summary>
		internal void InputReceivedCommand(NetworkInfo info, PhoniCommandCode commandCode, 
				PhoniPackageCode packageCode,  byte[] data) {
				
			if(Contains(info)) {
				PhoniDataBase unpackedData = null;
				if(PhoniPackageCenter.codeMap.ContainsKey(packageCode)) {
					
					unpackedData = PhoniPackageCenter.codeMap[packageCode].Deserialize(data);
				}
				PhoniCommandInfo commandInfo = new PhoniCommandInfo(commandCode, unpackedData);
					
				this[info].QueueReceiveCommand(commandInfo);
				if(CommandEvent != null) {
					CommandEvent(this[info], commandInfo);
				}
			}
		}
			
		/// <summary>
		/// Internal Use.
		/// Input command from TCP thread
		/// </summary>
		internal void InputReceivedCommand(NetworkInfo info, uint commandCode, 
				uint packageCode, byte[] data) {
			InputReceivedCommand(info, (PhoniCommandCode)commandCode,
					(PhoniPackageCode)packageCode, data);
		}
			
		/// <summary>
		/// Sync the received data, from UDP thread of each player/game to the main game thread.
		/// This method will acquire UDP read thread reader lock for each player/game.
		/// </summary>
		public void UpdateData() {
			foreach(PhoniDataPort dataPort in _idToInputBook.Values) {
				dataPort.UpdateAllReceivedData();
			}
		}
		
		/// <summary>
		/// Sync the sending data, from main game thread to UDP thread of each player/game.
		/// This method will acquire UDP write thread writer lock for each player/game.
		/// </summary>
		public void FlushData() {
			foreach(PhoniDataPort dataPort in _idToInputBook.Values) {
				dataPort.FlushAllSendingData();
			}
		}
			
		/// <summary>
		/// Gets and cleans up the command queues from TCP thread of each player/game.
		/// This method will acquire TCP read thread writer lock for each player/game.
		/// </summary>
		/// <returns>
		/// The command queues.
		/// </returns>
		public Dictionary<PhoniDataPort, Queue<PhoniCommandInfo>> GetCommandQueues () {
			Dictionary<PhoniDataPort, Queue<PhoniCommandInfo>> dic = new Dictionary<PhoniDataPort, Queue<PhoniCommandInfo>>();
			foreach(PhoniDataPort dataPort in _idToInputBook.Values) {
				Queue<PhoniCommandInfo> queue = dataPort.UseCommandQueue();
				if(queue.Count > 0) {
					dic[dataPort] = queue;
				}
			}
			return dic;
		}
			
		private int GetNextID() {
			if(_reusableIdQueue.Count > 0) {
				return _reusableIdQueue.Peek();
			}
			return nextID;
		}
		
		private int UseNextID() {
			if(_reusableIdQueue.Count > 0) {
				return _reusableIdQueue.Dequeue();
			}
			return nextID++;
		}
	}

	
} // namespace PhoniCore
} // namespace Phoni
	







