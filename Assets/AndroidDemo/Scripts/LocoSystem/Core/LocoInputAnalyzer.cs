using UnityEngine;
using System.Collections.Generic;
using System;

namespace LocoCore {
	
	
	public static class LocoInputAnalyzer {
		public static TouchInputAnalyzer touchInput = new TouchInputAnalyzer();
	}
	
	public class TouchInputAnalyzer {
		
		public LocoTouch simulateTouch;
		public bool simulateHasTouch;
		public LocoTouch simulateTouchSecond;
		public bool simulateHasTouchSecond;
		
		public float rayCastRange;
		public int layerMask;
		public int layer;
		public float swipeMinSpeed;
		public float recoverMaxCount;
		
		public HashSet<GameObject> receiverSet = new HashSet<GameObject>();
				
		private Dictionary<int, SwipeCheckStruct> swipeDic = new Dictionary<int, SwipeCheckStruct>();
				
		public TouchInputAnalyzer() {
			layerMask = 1 << LayerMask.NameToLayer("TouchInputObject");
			layer = LayerMask.NameToLayer("TouchInputObject");
			rayCastRange = 200;
			swipeMinSpeed = 600;
			recoverMaxCount = 3;
			simulateTouch = new LocoTouch();
			simulateTouchSecond = new LocoTouch();
			simulateHasTouch = false;
			simulateHasTouchSecond = false;
		}
		
		public LocoTouch[] GetTouches() {
#if !UNITY_EDITOR && (UNITY_IPHONE || UNITY_ANDROID)
			return LocoRawInput.Touches;
#else
			List<LocoTouch> touches = new List<LocoTouch>();
			if(simulateHasTouch) {
				touches.Add(simulateTouch);
			}
			if(simulateHasTouchSecond) {
				touches.Add(simulateTouchSecond);
			}
			return touches.ToArray();
#endif
		}
		
		public void StartSwipe(int fingerId, Vector2 startPos) {
			if(!swipeDic.ContainsKey(fingerId)) {
				swipeDic[fingerId] = new SwipeCheckStruct();
			}
			swipeDic[fingerId].TouchStart(startPos);
			swipeDic[fingerId].Start(startPos);
		}
		
		public Vector2 Swiping(int fingerId, Vector2 pos, float deltaTime) {
			if(swipeDic.ContainsKey(fingerId)) {
				float speed = swipeDic[fingerId].Move(pos, deltaTime);
				//Debug.Log(speed);
				if(speed >= swipeMinSpeed) {
					if(!swipeDic[fingerId].swipeFlag) {
						//Debug.Log("SWIPING, TRUE");
						swipeDic[fingerId].swipeFlag = true;
						swipeDic[fingerId].isSwiped = true;
						Vector2 dir = swipeDic[fingerId].GetDirection();
						return dir.normalized;
					}
					swipeDic[fingerId].recoverCount = 0;
				}
				else {
					if(swipeDic[fingerId].swipeFlag) {
						//Debug.Log("SWIPING, TRIGGER");
						swipeDic[fingerId].recoverCount ++;
						if(swipeDic[fingerId].recoverCount >= recoverMaxCount) {
							swipeDic[fingerId].swipeFlag = false;
							swipeDic[fingerId].Start(pos);
						}
					}
				}
			}
			return Vector2.zero;
		}
		
		
		public Vector2 EndSwipe(int fingerId, Vector2 pos, float deltaTime) {
			if(swipeDic.ContainsKey(fingerId)) {
				if(swipeDic[fingerId].Move(pos, deltaTime) >= swipeMinSpeed) {
					if(!swipeDic[fingerId].swipeFlag) {
						swipeDic[fingerId].swipeFlag = true;
						Vector2 dir = swipeDic[fingerId].GetDirection();
						return dir.normalized;
					}
				}
				else {
					// for one time swipe
					if(!swipeDic[fingerId].isSwiped) {
						Vector2 dir = swipeDic[fingerId].endPos - swipeDic[fingerId].startPos;
						if(dir.magnitude/swipeDic[fingerId].totalTime >= swipeMinSpeed) {
							return dir.normalized;
						}
					}
				}
			}
			return Vector2.zero;
		}
		
		public void ClearSwipe() {
			swipeDic.Clear();
		}
		
		private class SwipeCheckStruct {
			
			public bool swipeFlag = false;
			
			private Queue<Vector2> positionQueue = new Queue<Vector2>();
			private Queue<float> deltaTimeQueue = new Queue<float>();
			private int maxNum = 6;
			public Vector2 endPos;
			public Vector2 startPos;
			public float time;
			public int recoverCount;
			public bool isSwiped = false;
			public float totalTime;
			
			public void TouchStart(Vector2 pos) {
				startPos = pos;
				isSwiped = false;
				totalTime = 0;
			}
			
			public void Start(Vector2 pos) {
				swipeFlag = false;
				endPos = pos;
				time = 0;
				recoverCount = 0;
				positionQueue.Clear();
				deltaTimeQueue.Clear();
				positionQueue.Enqueue(pos);
				deltaTimeQueue.Enqueue(0);
			}
			
			public float Move(Vector2 pos, float deltaTime) {
				totalTime += deltaTime;
				endPos = pos;
				positionQueue.Enqueue(pos);
				deltaTimeQueue.Enqueue(deltaTime);
				time += deltaTime;
				if(positionQueue.Count > maxNum) {
					positionQueue.Dequeue();
					deltaTimeQueue.Dequeue();
					time -= deltaTimeQueue.Peek();
					return Vector2.Distance(positionQueue.Peek(), pos)/time;
				}
				return 0;
			}
			
			public Vector2 GetDirection() {
				if(positionQueue.Count >= 2) {
					return endPos - positionQueue.Peek();
				}
				return Vector2.zero;
			}
		}
	}
	
	
} // namespace LocoCore