using UnityEngine;
using System.Collections.Generic;
using LocoCore;

public class TouchDispatcher : MonoBehaviour {
	
	private Dictionary<int, HashSet<GameObject>> swipeGameObjects = new Dictionary<int, HashSet<GameObject>>();
	private Dictionary<int, GameObject> lastTouchGameObjects = new Dictionary<int, GameObject>();
	public delegate void TouchFunc(int fingerId, Vector2 pos);
	public event TouchFunc touchDownEvent = null;
	public event TouchFunc touchUpEvent = null;
	public event TouchFunc touchEvent = null;
	//private bool isTouch = false;

	// Use this for initialization
	void Start () {
	}
	
	// Update is called once per frame
	void Update () {
		LocoTouch[] touches = LocoInput.Touches;
		if(touches.Length > 0) {
			foreach(LocoTouch touch in touches) {
				switch(touch.phase) {
				case TouchPhase.Began:
					if(touchDownEvent != null) {
						touchDownEvent(touch.fingerId, touch.position);
					}
					InCheck(Util.Vec2ToVec3(touch.position,0), touch, "OnTouchIn");
					TriggerInputEvent(Util.Vec2ToVec3(touch.position,0), "OnTouchDown", touch);
					TriggerReceiveEvent(touch);
					SwipeStart(touch);
					break;
				case TouchPhase.Ended:
					//isTouch = false;
					if(touchUpEvent != null) {
						touchUpEvent(touch.fingerId, touch.position);
					}
					OutCheck(touch, "OnTouchOut");
					TriggerInputEvent(Util.Vec2ToVec3(touch.position,0), "OnTouchUp", touch);
					TriggerReceiveEvent(touch);
					SwipeEnd(touch);
					break;
				case TouchPhase.Moved:
				case TouchPhase.Stationary:	
					if(touchEvent != null) {
						touchEvent(touch.fingerId, touch.position);
					}
					InOutCheck(Util.Vec2ToVec3(touch.position,0), touch, "OnTouchIn", "OnTouchOut");
					TriggerInputEvent(Util.Vec2ToVec3(touch.position,0), "OnTouch", touch);
					TriggerReceiveEvent(touch);
					SwipeCheck(touch);
					break;
				}
			}
		}
	}
	
	private void SwipeStart(LocoTouch touch) {
		LocoInputAnalyzer.touchInput.StartSwipe(touch.fingerId, touch.position);
		if(swipeGameObjects.ContainsKey(touch.fingerId)) {
			swipeGameObjects[touch.fingerId].Clear();
		}
		else {
			swipeGameObjects[touch.fingerId] = new HashSet<GameObject>();
		}
	}
	
	private void SwipeCheck(LocoTouch touch) {
		if(!swipeGameObjects.ContainsKey(touch.fingerId)) {
			swipeGameObjects[touch.fingerId] = new HashSet<GameObject>();
		}
		swipeGameObjects[touch.fingerId].Add(GetNearestHitGameObject(Util.Vec2ToVec3(touch.position,0)));
		Vector2 swipeDir = LocoInputAnalyzer.touchInput.Swiping(touch.fingerId, touch.position, Time.deltaTime);
		if(swipeDir != Vector2.zero) {
			foreach(GameObject obj in swipeGameObjects[touch.fingerId]) {
				TriggerInputEvent(obj, "OnSwipe", swipeDir);
			}
			swipeGameObjects[touch.fingerId].Clear();
			swipeGameObjects[touch.fingerId].Add(GetNearestHitGameObject(Util.Vec2ToVec3(touch.position,0)));
		}
	}
	
	private void SwipeEnd(LocoTouch touch) {
		swipeGameObjects[touch.fingerId].Add(GetNearestHitGameObject(Util.Vec2ToVec3(touch.position,0)));
		Vector2 swipeDir = LocoInputAnalyzer.touchInput.EndSwipe(touch.fingerId, touch.position, Time.deltaTime);
		if(swipeDir != Vector2.zero) {
			foreach(GameObject obj in swipeGameObjects[touch.fingerId]) {
				TriggerInputEvent(obj, "OnSwipe", swipeDir);
			}
			swipeGameObjects.Clear();
		}
	}
	
	private void InOutCheck(Vector3 pos, LocoTouch touch, string inStr, string outStr) {
		GameObject hitObj = GetNearestHitGameObject(Util.Vec2ToVec3(touch.position,0));
		if(!lastTouchGameObjects.ContainsKey(touch.fingerId)) {
			lastTouchGameObjects[touch.fingerId] = null;
		}
		if(hitObj != lastTouchGameObjects[touch.fingerId]) {
			TriggerInputEvent(lastTouchGameObjects[touch.fingerId], outStr, touch);
			TriggerInputEvent(hitObj, inStr, touch);
			lastTouchGameObjects[touch.fingerId] = hitObj;
		}
	}
	
	private void InCheck(Vector3 pos, LocoTouch touch, string inStr) {
		GameObject hitObj = GetNearestHitGameObject(Util.Vec2ToVec3(touch.position,0));		
		TriggerInputEvent(hitObj, inStr, touch);
		lastTouchGameObjects[touch.fingerId] = hitObj;
	}
	
	private void OutCheck(LocoTouch touch, string outStr) {
		if(lastTouchGameObjects.ContainsKey(touch.fingerId)) {
			TriggerInputEvent(lastTouchGameObjects[touch.fingerId], outStr, touch);
			lastTouchGameObjects.Remove(touch.fingerId);
		}
	}
	
	private void TriggerReceiveEvent(LocoTouch touch) {
		foreach(GameObject go in LocoInputAnalyzer.touchInput.receiverSet) {
			TriggerInputEvent(go, "OnTouchReceive", touch);
		}
	}
	
	private void TriggerInputEvent(Vector3 pos, string eventStr) {
		TriggerInputEvent(pos, eventStr, null);
	}

	private void TriggerInputEvent(Vector3 pos, string eventStr, System.Object message) {
		/*
		foreach(RaycastHit hitInfo in hitInfos) {
			hitInfo.collider.gameObject.SendMessage(eventStr, message,SendMessageOptions.DontRequireReceiver);
		}
		*/
		/*
		GameObject hitObj = GetNearestHitGameObject(pos);
		if(hitObj != null) {
			hitObj.SendMessage(eventStr, message,SendMessageOptions.DontRequireReceiver);
		}
		*/
		//TriggerInputEvent(GetNearestHitGameObject(pos), eventStr, message);
		TriggerInputEvent(GetNearestHitGameObject(pos), eventStr, message);
	}
		
	private void TriggerInputEvent(GameObject hitObj, string eventStr) {
		TriggerInputEvent(hitObj, eventStr, null);
	}
	
	private void TriggerInputEvent(GameObject hitObj, string eventStr, System.Object message) {
		
		if(hitObj != null) {
			hitObj.SendMessageUpwards(eventStr, message,SendMessageOptions.DontRequireReceiver);
		}
	}
	
	private GameObject GetNearestHitGameObject(Vector3 pos) {
		Ray ray = Camera.main.ScreenPointToRay(pos);
		//change the ray range if your scene is larger.
		RaycastHit[] hitInfos = Physics.RaycastAll(ray, LocoInputAnalyzer.touchInput.rayCastRange, LocoInputAnalyzer.touchInput.layerMask);
		/*
		foreach(RaycastHit hitInfo in hitInfos) {
			hitInfo.collider.gameObject.SendMessage(eventStr, message,SendMessageOptions.DontRequireReceiver);
		}
		*/
		return GetNearestHitGameObject(hitInfos);
	}
					
	private GameObject GetNearestHitGameObject(RaycastHit[] hits) {
		if(hits.Length == 0) {
			return null;
		}
		RaycastHit nearestHit;
		float minDist = float.MaxValue;
		foreach(RaycastHit hit in hits) {
			if(hit.distance < minDist) {
				minDist = hit.distance;
				nearestHit = hit;
			}
		}
		return nearestHit.collider.gameObject;
	}
}
