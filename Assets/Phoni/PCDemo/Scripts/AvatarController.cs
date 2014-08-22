using UnityEngine;
using System.Collections;
using Phoni;

public class AvatarController : MonoBehaviour {
	
	private bool isInit = false;
	private int playerID;
	private float moveSpeed = 10;
	
	private bool isScale;
	private Vector3 scaleBase;
	private float distBase;
	private int fingerId1;
	private int fingerId2;

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
		if(isInit) {
			if(PhoniInput.Player.Contains(playerID)) {
				PhoniDataPort port = PhoniInput.Player[playerID];
				Vector3 moveVec = new Vector3(port.AnalogData.ReceivedData.left.x, 
					port.AnalogData.ReceivedData.left.y, 0);
				transform.position += moveVec * moveSpeed * Time.deltaTime;
				
				transform.Rotate(port.MotionData.ReceivedData.gyro.rotationRate);
				
				if(port.ButtonData.ReceivedData.GetButton(PhoniButton.Cross)) {
					transform.position = Vector3.zero;
					transform.localScale = new Vector3(1.5f,2f,0.5f);
					transform.eulerAngles = new Vector3(90,90,0);
				}
				
				PhoniTouchData touchData = port.TouchData.ReceivedData;
				if(isScale) {
					PhoniTouch touch1 = touchData.GetTouch(fingerId1);
					PhoniTouch touch2 = touchData.GetTouch(fingerId2);
					if(touch1 != null && touch2 != null) {
						transform.localScale = scaleBase * (Vector2.Distance(touch1.position, touch2.position)/distBase);
					}
					else {
						isScale = false;
					}
				}
				else if(touchData.TouchCount >= 2) {
					fingerId1 = touchData.touches[0].fingerId;
					fingerId2 = touchData.touches[1].fingerId;
					scaleBase = transform.localScale;
					distBase = Vector2.Distance(touchData.touches[0].position, touchData.touches[1].position);
					isScale = true;
				}
			}
		}
	}
	
	public void Init(int id) {
		playerID = id;
		isInit = true;
	}
	
}
