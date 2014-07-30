using UnityEngine;
using System;
using System.IO;
using System.Collections;

public class StreamUdpHoster : MonoBehaviour {
	Texture2D screenTex,packageTex;
	UdpSocketHoster hoster;
	[SerializeField]
	GUISkin mySkin;
	int jpgLength=0;
	// Use this for initialization
	void Start () {
		screenTex=new Texture2D(Screen.width,Screen.height,TextureFormat.RGB24,false);
		packageTex=new Texture2D(4,4);
		renderer.material.mainTexture=screenTex;
		hoster=new UdpSocketHoster("127.0.0.1", 8001);
		hoster.RecievedData+=onRecievedData;
		hoster.RecieverBuffer=NetworkPackage.Size;
	}
	
	// Update is called once per frame
	void Update () {
		if(isLinstening)
			hoster.Listen();
	}

	int screenWidth,screenHeight;
	void onRecievedData(byte[] data){
		Debug.Log("receive:"+data.Length);
		MemoryStream memStream=new MemoryStream(data);
		int intSize=4;
		int screenWidth=BitConverter.ToInt32(data,intSize*0);
		int screenHeight=BitConverter.ToInt32(data,intSize*1);
		int x=BitConverter.ToInt32(data,intSize*2);
		int y=BitConverter.ToInt32(data,intSize*3);
		int w=BitConverter.ToInt32(data,intSize*4);
		int h=BitConverter.ToInt32(data,intSize*5);
		jpgLength=BitConverter.ToInt32(data,intSize*6);
		byte[] jpgData=new byte[jpgLength];
		memStream.Seek(intSize*7, SeekOrigin.Begin);
		memStream.Read(jpgData,0,jpgLength);
		memStream.Close();

		if(screenTex.width!=screenWidth||screenTex.height!=screenHeight){
			screenTex.Resize(screenWidth,screenHeight);
			screenTex.Apply();
		}
		Debug.Log("x:"+x+", y:"+y+", w:"+w+", h"+h);
		setJpgData(x,y,w,h,jpgData);
	}

	void setJpgData(int x,int y,int w,int h,byte[] data){
		packageTex.LoadImage(data);
		Color[] colors=packageTex.GetPixels();
		screenTex.SetPixels(x,y,w,h,colors);
		screenTex.Apply();
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
		GUILayout.Label(jpgLength+"");
		if(GUILayout.Button("Exit")) {
			Application.Quit();
		}
		GUILayout.EndVertical();
	}
	
	void OnApplicationQuit() {
		hoster.Stop();
	}

}
