using UnityEngine;
using System.Collections;
using LocoCore;

public class TouchSimulator : MonoBehaviour {
	
	private LocoTouch data = new LocoTouch();
	private LocoTouch data2 = new LocoTouch();
	private bool isTouch;
	
	private GameObject secondTouchObj;
	private float SecondTouchSpeed = 20;
	private float secondTouchZ = -5;
	private Vector3 secondTouchScale = Vector3.one * 0.2f;

	// Use this for initialization
	void Start () {
#if UNITY_EDITOR || !(UNITY_ANDROID || UNITY_IPHONE)
		secondTouchObj = GameObject.CreatePrimitive(PrimitiveType.Sphere);
		secondTouchObj.transform.position = new Vector3(0,0, secondTouchZ);
		secondTouchObj.transform.localScale = secondTouchScale;
#endif
	}
	
	// Update is called once per frame
	void Update () {
#if UNITY_EDITOR || !(UNITY_ANDROID || UNITY_IPHONE)
		bool mouseFlag = false;
		if(Input.GetMouseButtonDown(0)) {
			FillInData(Time.deltaTime);
			data.phase = TouchPhase.Began;
			isTouch = true;
			mouseFlag = true;
		}
		else if(Input.GetMouseButtonUp(0)) {
			FillInData(Time.deltaTime);
			data.phase = TouchPhase.Ended;
			isTouch = false;
			mouseFlag = true;
		}
		else if(Input.GetMouseButton(0)) {
			FillInData(Time.deltaTime);
			if(data.deltaPosition == Vector2.zero) {
				data.phase = TouchPhase.Stationary;
			}
			else {
				data.phase = TouchPhase.Moved;
			}
			isTouch = true;
			mouseFlag = true;
		}
		
		LocoInputAnalyzer.touchInput.simulateTouch = data;
		LocoInputAnalyzer.touchInput.simulateHasTouch = mouseFlag;
		
		Vector2 speed = new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical")) * 
			SecondTouchSpeed * Time.deltaTime;
		secondTouchObj.transform.position += Util.Vec2ToVec3(speed, 0);
		bool keyboardFlag = false;
		if(Input.GetKeyDown(KeyCode.Keypad0) || Input.GetKeyDown(KeyCode.Space)) {
			FillInData2(Time.deltaTime);
			data2.phase = TouchPhase.Began;
			isTouch = true;
			keyboardFlag = true;
			secondTouchObj.renderer.material.color = Color.red;
		}
		else if(Input.GetKeyUp(KeyCode.Keypad0) || Input.GetKeyUp(KeyCode.Space)) {
			FillInData2(Time.deltaTime);
			data2.phase = TouchPhase.Ended;
			isTouch = false;
			keyboardFlag = true;
			secondTouchObj.renderer.material.color = Color.white;
		}
		else if(Input.GetKey(KeyCode.Keypad0) || Input.GetKey(KeyCode.Space)) {
			FillInData2(Time.deltaTime);
			if(data2.deltaPosition == Vector2.zero) {
				data2.phase = TouchPhase.Stationary;
			}
			else {
				data2.phase = TouchPhase.Moved;
			}
			isTouch = true;
			keyboardFlag = true;
		}
		
		
		LocoInputAnalyzer.touchInput.simulateTouchSecond = data2;
		LocoInputAnalyzer.touchInput.simulateHasTouchSecond = keyboardFlag;
#endif
	}
	
	private void FillInData(float deltaTime) {
		Vector2 curPos = Util.Vec3ToVec2(Input.mousePosition);
		if(isTouch) {
			data.deltaPosition = curPos - data.position;
		}
		else {
			data.deltaPosition = Vector2.zero;
		}
		data.position = curPos;
		data.deltaTime = deltaTime;
		data.tapCount = 0;
		data.fingerId = 0;
	}
	
	private void FillInData2(float deltaTime) {
		Vector2 curPos = Util.WorldToScreen(Camera.main, secondTouchObj.transform.position);
		if(isTouch) {
			data2.deltaPosition = curPos - data2.position;
		}
		else {
			data2.deltaPosition = Vector2.zero;
		}
		data2.position = curPos;
		data2.deltaTime = deltaTime;
		data2.tapCount = 0;
		data2.fingerId = 1;
	}
}
