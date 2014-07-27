using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Phoni;

public class TouchViewer : MonoBehaviour {
	
	Dictionary<int, Dictionary<int, GameObject>> playerList = new Dictionary<int, Dictionary<int, GameObject>>();


	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	}
	
	public void ShowTouch(int playerID, PhoniTouchData touchData, Color color) {
		if(!playerList.ContainsKey(playerID)) {
			playerList[playerID] = new Dictionary<int, GameObject>();
		}
		Dictionary<int, GameObject> touchList = playerList[playerID];
		foreach(int fingerID in touchList.Keys) {
			if(touchData.HasTouch(fingerID)) {
				if(touchList[fingerID].active == false) {
					touchList[fingerID].active = true;
				}
				SetTouchObjPos(touchList[fingerID], touchData.GetTouch(fingerID).positionNormalized);
			}
			else if(!touchData.HasTouch(fingerID) &&
				touchList[fingerID].active == true) {
				touchList[fingerID].active = false;
			}
		}
		foreach(PhoniTouch touch in touchData.touches) {
			if(!touchList.ContainsKey(touch.fingerId)) {
				touchList[touch.fingerId] = CreateNewTouchObj(touch.positionNormalized, color);
			}
		}
	}
	
	public void Clear() {
		foreach(Dictionary<int, GameObject> touchList in playerList.Values) {
			foreach(GameObject go in touchList.Values) {
				GameObject.Destroy(go);
			}
			touchList.Clear();
		}
		playerList.Clear();
	}
	
	public void Clear(int playerID) {
		Dictionary<int, GameObject> touchList = playerList[playerID];
		foreach(GameObject go in touchList.Values) {
			GameObject.Destroy(go);
		}
		touchList.Clear();
	}
	
	
	private GameObject CreateNewTouchObj(Vector2 normPos, Color color) {
		GameObject touchObj = GameObject.CreatePrimitive(PrimitiveType.Sphere);
		touchObj.renderer.material.color = color;
		GameObject.Destroy(touchObj.collider);
		SetTouchObjPos(touchObj, normPos);
		return touchObj;
	}
	
	private void SetTouchObjPos(GameObject target, Vector2 normPos) {
		Vector3 screenPos = new Vector3(normPos.x * Screen.width, normPos.y * Screen.height, 30);
		target.transform.position = Camera.main.ScreenToWorldPoint(screenPos);
	}
}
