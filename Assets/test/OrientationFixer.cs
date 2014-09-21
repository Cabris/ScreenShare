using UnityEngine;
using System.Collections;

public class OrientationFixer : MonoBehaviour {
	[SerializeField]
	Transform child;
//	[SerializeField]
//	Transform matchTarget;
	[SerializeField]
	Transform TiltingFixer;
	// Use this for initialization
	void Start () {
	
	}
	[SerializeField]
	Vector3 offset;
	// Update is called once per frame
	void Update () {
		Quaternion childRotation = child.localRotation;
		//Quaternion targetRotation = matchTarget.rotation;
		Quaternion inv = Quaternion.Inverse (childRotation);
		Quaternion fix = inv;
		//offset = targetRotation - childRotation;
		if (Input.GetKeyDown (KeyCode.F)) {
			Quaternion lr=fix;
			TiltingFixer.localRotation=lr;

		}
	}
}









