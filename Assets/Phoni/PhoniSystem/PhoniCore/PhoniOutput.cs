using System;
using System.Collections;

namespace Phoni {
namespace PhoniCore {
	
	public static class PhoniLog {
		
		public static void Log(Object line) {
#if UNITY_EDITOR || UNITY_ANDROID
			UnityEngine.Debug.Log(line);
#else
			Console.Out.WriteLine(line.ToString());
#endif
		}
			
		public static void Error(Object line) {
#if UNITY_EDITOR || UNITY_ANDROID
			UnityEngine.Debug.LogError(line.ToString());
#else
			Console.Error.WriteLine(line.ToString());
#endif
		}
	}
	
} // namespace PhoniCore
} // namespace Phoni
