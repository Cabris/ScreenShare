using UnityEngine;
using System.Collections;
using Phoni;

public class VirtualButtonController : MonoBehaviour {
	
	public ButtonType button;
	public bool IsDown {
		get {
			return fingerCount > 0;
		}
	}
	
	public PhoniButton phoniButton {
		get {
			return (PhoniButton)((uint)button);
		}
	}
	
	private Vector3 originalScale;
	private int fingerCount;
	private bool isPrevDown;

	// Use this for initialization
	void Start () {
		fingerCount = 0;
		originalScale = transform.localScale;
		isPrevDown = false;
	}
	
	// Update is called once per frame
	void Update () {
		if(isPrevDown != IsDown) {
			if(IsDown) {
				transform.localScale = originalScale * 1.2f;
			}
			else {
				transform.localScale = originalScale;
			}
			isPrevDown = IsDown;
		}
	}
	
	void OnTouchIn() {
		++fingerCount;
	}
	
	void OnTouchOut() {
		--fingerCount;
	}
	
	void OnApplicationPause(bool pause) {
		if(pause) {
			fingerCount = 0;
		}
	}	
}

public enum ButtonType {
	Triangle = (int)PhoniButton.Triangle,
	Square = (int)PhoniButton.Square,
	Circle = (int)PhoniButton.Circle,
	Cross = (int)PhoniButton.Cross,
}
