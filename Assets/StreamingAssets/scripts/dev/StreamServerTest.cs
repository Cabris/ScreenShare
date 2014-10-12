using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;

using rtaNetworking.Streaming;

public class StreamServerTest : MonoBehaviour {
	private ImageStreamingServer server;

	Texture2D tex ;
	Texture2D testTex;
	int bufferSize=100000;
	[SerializeField]
	Renderer testRenderer;
	int w=1024;
	int h=512;

	byte[] buffer;
	byte[] jpgData;
	void Start () {
		if(w>Screen.width)
			w=Screen.width;
		if(h>Screen.height)
			h=Screen.height;
		Application.runInBackground=true;
		tex=new Texture2D(w,h,TextureFormat.RGB24,false);
		server = new ImageStreamingServer();
		server.ClientWork += send;
		server.Interval = 33;
		server.Start();

		testTex=new Texture2D(w,h,TextureFormat.RGB24,false);
		testRenderer.material.mainTexture=testTex;
	}
	
	void OnApplicationQuit() {
		server.Stop();
	}

	string dataStr="XXX";
	bool isReadPixel=true;

	float p=0.03f;
	float c=0;

	void OnPostRender ()
	{
		if(c>=p){
			StartCoroutine("readPixels");
			c=0;
		}else
			c+=Time.deltaTime;

	}

	IEnumerator readPixels(){
		isReadPixel=true;
		//yield return isReadPixel;
		tex.ReadPixels (new Rect (0, 0, w, h), 0, 0, false);
		JPGEncoder jpgEncoder=new JPGEncoder(tex.GetPixels(),tex.width,tex.height,80);
		jpgEncoder.doEncoding();
		jpgData=jpgEncoder.GetBytes();
		length=jpgData.Length;
		//Debug.Log(jpgData.Length);
		testTex.LoadImage(jpgData);
		isReadPixel=false;
		yield return isReadPixel;
	}

	void send (Socket client)
	{
		client.ReceiveBufferSize =bufferSize;
		if (jpgData == null&&!isReadPixel)
			return;
		buffer=new byte[bufferSize];
		for (int i = 0; i < jpgData.Length; i++) {
			buffer [i] = jpgData [i];
		}
		client.Send(buffer,buffer.Length, SocketFlags.None);
		//client.Send(buffer,buffer.Length, SocketFlags.None);
		Debug.Log ("send");
	}
	
	//string dataStr="XXX";
	int length;
	void OnGUI() {
		//GUI.Label(new Rect(0,Screen.height-100,800,50),dataStr+"");
		dataStr = GUI.TextField(new Rect(50,Screen.height-100,200,50),dataStr);
		GUI.Box(new Rect(50,Screen.height-50,100,50),length+"");
	}
}


