using UnityEngine;
using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Xml;


namespace LocoCore {
	
	public static class Util {
		
		
		public static Vector2 Vec3ToVec2(Vector3 vec) {
			return new Vector2(vec.x, vec.y);
		}
		
		public static Vector3 Vec2ToVec3(Vector2 vec, float z) {
			return new Vector3(vec.x, vec.y, z);
		}
				
		public static Vector3 ScreenToWorld(Camera cam, Vector3 screenPos, float worldZ) {
			float zVal = worldZ - cam.transform.position.z;
			return cam.ScreenToWorldPoint(Util.Vec2ToVec3(screenPos, zVal));
		}
		
		public static Vector3 WorldToScreen(Camera cam, Vector3 worldPos) {
			return cam.WorldToScreenPoint(worldPos);
		}
				
	}
	
	
	
} // namespace LocoCore
