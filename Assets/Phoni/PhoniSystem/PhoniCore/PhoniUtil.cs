//#define JSON

using System.Collections;
using System.Collections.Generic;
using System;
using System.Net;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading;

namespace Phoni {
namespace PhoniCore {
		

	public static class PhoniUtil {
			
	
		public static byte[] GetSubArray(byte[] data, int startIndex, int length) {
			byte[] pureData = new byte[length];
			Array.Copy(data, startIndex, pureData, 0, length);
			return pureData;
		}
			
		public static byte[] GetSubArray(byte[] data, int startIndex) {
			return GetSubArray(data, startIndex, data.Length - startIndex);
		}
			
		
		public static IPAddress GetIPAddress() {
#if UNITY_IPHONE || UNITY_STANDALONE_OSX
			return IPAddress.Parse(UnityEngine.Network.player.ipAddress);
#else
			foreach(IPAddress ip in Dns.GetHostAddresses(Dns.GetHostName())) {
                if (ip.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
                {
                    return ip;
                }
            }
            return IPAddress.Any;
#endif
		}
		
			
		public static byte[] Serialize(object obj) {
#if JSON
			List<byte> list = new List<byte>();
			byte[] typeBytes = System.Text.Encoding.Unicode.GetBytes(obj.GetType().ToString());
			list.AddRange(BitConverter.GetBytes(typeBytes.Length));
			list.AddRange(typeBytes);
			string json = JsonFx.Json.JsonWriter.Serialize(obj);
			list.AddRange(System.Text.Encoding.Unicode.GetBytes(json));
			return list.ToArray();
#else
			MemoryStream packageStream = new MemoryStream();
			new BinaryFormatter().Serialize(packageStream, obj);
			return packageStream.ToArray();
#endif
		}
			
		public static object Deserialize(byte[] data) {				
							
			if(data == null || data.Length == 0) {
				return null;
			}				
			
#if JSON
			int typeLen = BitConverter.ToInt32(data,0);
			string typeStr = System.Text.Encoding.Unicode.GetString(data, 4, typeLen);
			string json = System.Text.Encoding.Unicode.GetString(GetSubArray(data, 4+typeLen));
			
			return DeserializeGenericMethod.MakeGenericMethod(Type.GetType(typeStr)).Invoke(null, new object[]{json});
#else
			MemoryStream packageStream = new MemoryStream(data);
			return (new BinaryFormatter().Deserialize(packageStream));
#endif
		}
						
#if JSON		
		private static System.Reflection.MethodInfo DeserializeGenericMethod {
			get {
				if(_method == null) {
					foreach(System.Reflection.MethodInfo mi in typeof(JsonFx.Json.JsonReader).GetMethods()) {
						if(mi.Name == "Deserialize" && mi.IsGenericMethod && mi.GetParameters().Length == 1) {
							_method = mi;
						}
					}
				}
				return _method;
			}
		}
		private static System.Reflection.MethodInfo _method = null;
#endif		
	}
	
} // namespace PhoniCore
} // namespace Phoni
