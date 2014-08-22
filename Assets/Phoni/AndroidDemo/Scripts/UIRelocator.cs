using UnityEngine;
using System.Collections;

public class UIRelocator : MonoBehaviour {
	
	public GameObject fixPoint;
	
	public float fixXRatio;
	public float fixYRatio;

	// Use this for initialization
	void Start () {
		Vector2 screenFixPoint = new Vector2(fixXRatio * Screen.width, fixYRatio * Screen.height);
		Vector3 targetFixPoint = LocoCore.Util.ScreenToWorld(Camera.main, 
			screenFixPoint, fixPoint.transform.position.z);
		transform.position += (targetFixPoint - fixPoint.transform.position);
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
