using UnityEngine;
using System.Collections;

public class StreamUdpHoster : MonoBehaviour {
	Texture2D tex;
	UdpSocketHoster hoster;
	[SerializeField]
	GUISkin mySkin;
	// Use this for initialization
	void Start () {
		tex=new Texture2D(64,64,TextureFormat.RGB24,false);
		renderer.material.mainTexture=tex;
		hoster=new UdpSocketHoster("127.0.0.1", 8001);
		hoster.RecievedData+=onRecievedData;
		hoster.RecieverBuffer=NetworkPackage.Size;
	}
	
	// Update is called once per frame
	void Update () {
		if(isLinstening)
			hoster.Listen();
	}

	void onRecievedData(byte[] data){
		Debug.Log("receive:"+data.Length);
		tex.LoadImage(data);
	}
	bool isLinstening=false;
	void OnGUI() {
		GUI.skin=mySkin;
		GUIStyle style=new GUIStyle();
		style.fontSize=50;
		GUILayout.BeginVertical();
		GUILayout.Label( "server address");
		//ipAddress = GUILayout.TextField(ipAddress,myStyle);
		isLinstening=GUILayout.Toggle(isLinstening,"isLinstening");

		if(GUILayout.Button("Exit")) {
			Application.Quit();
		}
		//GUILayout.Label(dataStr,myStyle);
		GUILayout.EndVertical();
	}

}
