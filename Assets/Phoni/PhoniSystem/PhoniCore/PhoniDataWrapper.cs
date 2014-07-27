using System;
using System.IO;
using System.Collections.Generic;
using System.Runtime.Serialization.Formatters.Binary;

namespace Phoni {
namespace PhoniCore {
	
	public abstract class PhoniDataWrapperBase {
		
		protected PhoniDataBase _readData;
		protected PhoniDataBase _writeData;
		protected PhoniDataBase _inputData;
		protected PhoniDataBase _outputData;
			
		/// <summary>
		/// Gets a value indicating whether the ReceivedData is valid.
		/// The data is valid if the wrapper received it from a remote source at least once, so the value is not its initial  value.
		/// </summary>
		/// <value>
		/// <c>true</c> if this instance is valid; otherwise, <c>false</c>.
		/// </value>
		public bool IsValid {
			get {
				return _readData.isValid;
			}
		}
			
		public PhoniDataWrapperBase() {
		}
			
		public void UpdateReceivedData() {
			_readData = _inputData;
		}
		
		public void FlushSendingData() {
			_outputData = _writeData;
		}
			
		/// <summary>
		/// Internal Use.
		/// </summary>
		internal void InputReceivedData(byte[] data) {
			_inputData.Deserialize(data);
			_inputData.isValid = true;
		}
			
		/// <summary>
		/// Internal Use.
		/// </summary>	
		internal PhoniDataBase GetInternalOutputData ()
		{
			return _outputData;
		}
	}
	
	public class PhoniDataWrapper<T> : PhoniDataWrapperBase {
									
		public T ReceivedData {
			get {
				T data = _readData.GetData<T>();
				return data;
			}
		}
		
		public T SendingData {
			get {
				return _writeData.GetData<T>();
			}
			set {
				_writeData.SetData<T>(value);
			}
		}
		
		public PhoniDataWrapper() : base() {
			_readData = new PhoniData<T>();
			_inputData = new PhoniData<T>();				
			_writeData = new PhoniData<T>();
			_outputData = new PhoniData<T>();
		}
	}
		
	public class PhoniDataWrapper<T, U> : PhoniDataWrapperBase where U : IPhoniPackage<T>, new() {
					
		public T ReceivedData {
			get {
				T data = _readData.GetData<T>();
				return data;
			}
		}
		
		public T SendingData {
			get {
				return _writeData.GetData<T>();
			}
			set {
				_writeData.SetData<T>(value);
			}
		}
			
		public PhoniDataWrapper() : base() {
			_readData = new PhoniData<T, U>();
			_inputData = new PhoniData<T, U>();
			_writeData = new PhoniData<T, U>();
			_outputData = new PhoniData<T, U>();
		}			
	}
		
	public class PhoniDataWrapperCustom<T> : PhoniDataWrapperBase where T : PhoniDataBase, new() {
					
		public T ReceivedData {
			get {
				T data = (T)_readData;
				return data;
			}
		}
		
		public T SendingData {
			get {
				return (T)_writeData;
			}
			set {
				_writeData = value;
			}
		}
			
		
		public PhoniDataWrapperCustom() : base() {
			_readData = new T();
			_inputData = new T();
			_writeData = new T();
			_outputData = new T();
		}
	}	
		
	
} // namespace PhoniCore
} // namespace Phoni