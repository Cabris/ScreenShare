using UnityEngine;
using System.Collections;

public class VirtualJoystickController : MonoBehaviour {
	
	private bool isFocus = false;
	private int focusFingerID = 0;
	private bool isUnregister = false;
	
	[HideInInspector]
	public Vector2 analogData;
	
	public float radius = 3;
	public GameObject sphere;
	

	// Use this for initialization
	void Start () {
		analogData = Vector2.zero;
	}
	
	// Update is called once per frame
	void Update () {
		Vector2 posXY = LocoCore.Util.Vec3ToVec2(transform.position) + analogData * radius;
		sphere.transform.position = LocoCore.Util.Vec2ToVec3(posXY, sphere.transform.position.z);
	}
	
	void LateUpdate() {
		if(isUnregister) {
			isUnregister = false;
			if(!isFocus) {
				LocoInput.UnregisterInput(gameObject);
			}
		}
	}
	
	void OnTouchDown(LocoTouch touch) {
		if(!isFocus) {
			isFocus = true;
			focusFingerID = touch.fingerId;
			LocoInput.RegisterInput(gameObject);
			SetAnalogData(touch.position);
		}
	}
	
	void OnTouchReceive(LocoTouch touch) {
		if(touch.fingerId == focusFingerID) {
			if(touch.IsOnScreen) {
				SetAnalogData(touch.position);
			}
			else {
				isFocus = false;
				isUnregister = true;
				analogData = Vector2.zero;
			}
		}
	}
	
	
	private void SetAnalogData(Vector2 touchPos) {
		Vector3 touchWorld = LocoCore.Util.ScreenToWorld(Camera.main, 
			LocoCore.Util.Vec2ToVec3(touchPos, 0),
			transform.position.z);
		analogData.x = Mathf.Clamp((touchWorld.x - transform.position.x)/radius, -1, 1);
		analogData.y = Mathf.Clamp((touchWorld.y - transform.position.y)/radius, -1, 1);
		if(analogData.sqrMagnitude > 1) {
			analogData.Normalize();
		}
	}
	
	void OnApplicationPause(bool pause) {
		if(pause) {
			isFocus = false;
			isUnregister = true;
			analogData = Vector2.zero;
		}
	}	
}
