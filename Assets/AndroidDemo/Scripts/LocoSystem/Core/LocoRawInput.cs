using UnityEngine;
using System.Collections;

namespace LocoCore {

	public static class LocoRawInput {
		
		public static LocoTouch[] Touches {
			get {
				int len = Input.touches.Length;
				LocoTouch[] touches = new LocoTouch[len];
				for(int i = 0; i < len; ++i) {
					touches[i] = new LocoTouch(Input.touches[i]);
				}
				return touches;
			}
		}
	}
	
	
	
} // namespace LocoCore