using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Collections;
using System.Collections.Generic;
using Phoni;
using Phoni.PhoniCore;

namespace Phoni {	
			
	public enum PhoniPlatformCode : uint {
		PLATFORM_NONE = 0,
		
		PLATFORM_STANDALONE,
		PLATFORM_ANDROID,
		PLATFORM_VITA,
	};
	
	public enum PhoniCommandCode : uint {
		COMMAND_NONE = 0,
		// system use
		COMMAND_CONNECT,
		COMMAND_DISCONNECT,
		COMMAND_LOST_CONNECTION,
		COMMAND_SUSPEND,
		// custom use
		COMMAND_RUMBLE,		
		COMMAND_UI_SWITCH,
	}
	
	public enum PhoniDataCode : uint {
		DATA_PHONI_NONE = 0,
		// system use
		DATA_PHONI_TOUCH,
		DATA_PHONI_MOTION,
		DATA_PHONI_ANALOG,
		DATA_PHONI_BUTTON,
		// custom use
		MY_STRING_DATA,
	}
	
	public enum PhoniPackageCode : uint {
		PACKAGE_NONE = 0,
		// system use
		PACKAGE_SINGLE_DATA,
		PACKAGE_DOUBLE_DATA,
		// custom use
		PACKAGE_CUSTOM_SAMPLE,
	};
	
	internal enum PhoniButtonLineup {
		Left = 0,
		Up,
		Right,
		Down,
		Square,
		Triangle,
		Circle,
		Cross,
		Start,
		Select,
		
		Total,
	}
	
	[Flags]
	public enum PhoniButton : uint {
		None	= 0,
		
		Left 	= 1 << PhoniButtonLineup.Left,
		Up 		= 1 << PhoniButtonLineup.Up,
		Right 	= 1 << PhoniButtonLineup.Right,
		Down 	= 1 << PhoniButtonLineup.Down,
		Square 	= 1 << PhoniButtonLineup.Square,
		Triangle = 1 << PhoniButtonLineup.Triangle,
		Circle 	= 1 << PhoniButtonLineup.Circle,
		Cross 	= 1 << PhoniButtonLineup.Cross,
		Start 	= 1 << PhoniButtonLineup.Start,
		Select 	= 1 << PhoniButtonLineup.Select,
		
		All = ((1 << PhoniButtonLineup.Total) - 1),
	}
	
	
	public static class PhoniPackageCenter {
		public static Dictionary<PhoniPackageCode, PhoniDataBase> codeMap = new Dictionary<PhoniPackageCode, PhoniDataBase>();
		
		static PhoniPackageCenter() {
			// system use
			Add<PhoniDataBase>();
			Add<PhoniDataDoubleBase>();
			// custom use
			Add<PhoniCustomDataSample>();
		}
		
		static void Add<T>() where T : PhoniDataBase, new(){
			PhoniDataBase data = new T();
			codeMap[(PhoniPackageCode)data.GetCode()] = data;
		}
	}
	
			
	public interface IPhoniPackageBase {
		Object Unpack();
	}
	
	public interface IPhoniPackage<T> : IPhoniPackageBase {
		void Pack(T data);
	}
				
	public class PhoniDataBase {
				
		public bool isValid;
		protected Object _valueObj;
				
		public PhoniDataBase () {
			_valueObj = null;
		}
		
		public PhoniDataBase (Object elem) {
			_valueObj = elem;
		}
		
		public virtual uint GetCode ()
		{
			return (uint)PhoniPackageCode.PACKAGE_SINGLE_DATA;
		}
				
		public virtual T GetData<T> () {
			return (T)_valueObj;
		}
		
		public virtual void SetData<T> (T data) {
			_valueObj = data;
		}
				
		public virtual byte[] Serialize ()
		{
			return PhoniUtil.Serialize(_valueObj);
		}
		
		public virtual PhoniDataBase Deserialize (byte[] data)
		{
			if(data == null) {
				return null;
			}
			
			_valueObj = PhoniUtil.Deserialize(data);
			if(_valueObj == null) {
				return null;
			}
			
			System.Type dataType = _valueObj.GetType();
			System.Type genericType = typeof(PhoniData<>).MakeGenericType(dataType);
			PhoniDataBase result = (PhoniDataBase)Activator.CreateInstance(genericType);
			result.SetData<object>(_valueObj);
			return result;
		}		
	}
	
	public class PhoniData<T> : PhoniDataBase {
		
		public T Data {
			get {
				return (T)_valueObj;
			}
			set {
				_valueObj = value;
			}
		}
		
		public PhoniData () : base() {
			if(typeof(T) == typeof(string)) {
				_valueObj = "";
			}
			else if(!typeof(T).IsValueType) {
				_valueObj = (T)Activator.CreateInstance(typeof(T));
			}
			else {
				_valueObj = default(T);
			}
		}
		
		public PhoniData (T elem) : base(elem) {
		}
	}
	
	public class PhoniDataDoubleBase : PhoniDataBase {
		
		protected Object _packObj;
				
		public PhoniDataDoubleBase () : base() {
		}
		
		public PhoniDataDoubleBase (Object elem) : base(elem) {
		}
				
		public override uint GetCode ()
		{
			return (uint)PhoniPackageCode.PACKAGE_DOUBLE_DATA;
		}
		
		public override PhoniDataBase Deserialize (byte[] data)
		{
			if(data == null) {
				return null;
			}
			_packObj = PhoniUtil.Deserialize(data);
			if(_packObj == null) {
				return null;
			}
			
			_valueObj = ((IPhoniPackageBase)_packObj).Unpack();
			if(_valueObj == null) {
				return null;
			}
			
			System.Type dataType = _valueObj.GetType();
			System.Type packType = _packObj.GetType();
			System.Type genericType = typeof(PhoniData<,>).MakeGenericType(dataType, packType);
			PhoniDataBase result = (PhoniDataBase)Activator.CreateInstance(genericType);
			result.SetData<object>(_valueObj);
			return result;
		}				
	}
	
	public class PhoniData<T, U> : PhoniDataDoubleBase where U : IPhoniPackage<T>, new() {
		public T Data {
			get {
				return (T)_valueObj;
			}
			set {
				_valueObj = value;
			}
		}
		
		public PhoniData () : base() {
			if(!typeof(T).IsValueType && typeof(T) != typeof(string)) {
				_valueObj = (T)Activator.CreateInstance(typeof(T));
			}
			else {
				_valueObj = default(T);
			}
		}
		
		public PhoniData (T elem) : base(elem) {
		}
				
		public override byte[] Serialize ()
		{
			_packObj = new U();
			((IPhoniPackage<T>)_packObj).Pack((T)_valueObj);
			return PhoniUtil.Serialize(_packObj);
		}
	}
		
	public class PhoniHeader {
		
		public const UInt32 PHONI_HEADER_CODE = 0xF02EFFFF;
		
		public readonly UInt32 phoniHeaderCode;
		//public UInt32 platformCode;
		public NetworkInfo sourceNetworkInfo;
		public UInt32 packageCode;
		public UInt32 contentCode;
		
		private static PhoniHeader instance = new PhoniHeader();
		
		
		static public int Length {
			get {
				return instance.ToBytes().Length;
			}
		}
		
		public PhoniHeader (){
			phoniHeaderCode = PHONI_HEADER_CODE;
			//platformCode = 0;
			sourceNetworkInfo = new NetworkInfo();
			packageCode = 0;
			contentCode = 0;
		}
		
		public PhoniHeader(NetworkInfo sourceNetworkInfo, UInt32 packageCode, UInt32 contentCode) {
			phoniHeaderCode = PHONI_HEADER_CODE;
			//this.platformCode = platformCode;
			this.sourceNetworkInfo = sourceNetworkInfo;
			this.packageCode = packageCode;
			this.contentCode = contentCode;
		}
		
		public byte[] ToBytes() {
			List<byte> dataList = new List<byte>();
			dataList.AddRange(BitConverter.GetBytes(phoniHeaderCode));
			//dataList.AddRange(BitConverter.GetBytes(platformCode));
			dataList.AddRange(sourceNetworkInfo.ipBytes);
			dataList.AddRange(BitConverter.GetBytes(sourceNetworkInfo.port));
			dataList.AddRange(BitConverter.GetBytes(packageCode));
			dataList.AddRange(BitConverter.GetBytes(contentCode));
			return dataList.ToArray();
		}
		
		public bool Parse(byte[] data, int i) {
			if(data.Length - i < Length) {
				return false;
			}
			UInt32 header = BitConverter.ToUInt32(data, i+0);
			if(header != PHONI_HEADER_CODE) {
				return false;
			}
			//platformCode = BitConverter.ToUInt32(data, i+4);
			sourceNetworkInfo.ipBytes = PhoniUtil.GetSubArray(data, i+4, 4);
			sourceNetworkInfo.port = BitConverter.ToInt32(data, i+8);			
			packageCode = BitConverter.ToUInt32(data, i+12);		
			contentCode = BitConverter.ToUInt32(data, i+16);
			
			return true;
		}
		
		public bool Parse(byte[] data) {
			return Parse(data, 0);
		}
		
		static public PhoniHeader ParseBytes(byte[] data, int i) {
			PhoniHeader header = new PhoniHeader();
			if(header.Parse(data, i)) {
				return header;
			}
			return null;
		}
		
		static public PhoniHeader ParseBytes(byte[] data) {
			return ParseBytes(data, 0);
		}
		
	}
	
} // namespace Phoni
