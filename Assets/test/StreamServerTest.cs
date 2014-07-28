using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;

using rtaNetworking.Streaming;

public class StreamServerTest : MonoBehaviour {
	Texture2D tex ;
	private ImageStreamingServer server;

	byte iniByte;
	float p=0.1f;
	float c=0;
	int w=64;
	int h=64;
	byte[] pngData;
	void Start () {
		Application.runInBackground=true;
		tex=new Texture2D(w,h,TextureFormat.RGB24,false);
		server = new ImageStreamingServer();
		server.ClientWork += send;
		//server.Interval = 100;
		server.Start();
	}
	
	void OnApplicationQuit() {
		server.Stop();
	}

	string dataStr="XXX";
	void OnPostRender ()
	{
		tex.ReadPixels (new Rect (0, 0, w, h), 0, 0, false);
		pngData = tex.EncodeToPNG ();
//		if (c >= p) {
//			if (server.Clients.Count>0) {
//
//			}
//
//			c = 0;
//		}
//		else
//			c += Time.deltaTime;
		
	}
	
	void send_1(Socket client){

		byte[] byteArray = System.Text.Encoding.Default.GetBytes ( dataStr );
//		for (int i = 0; i < bufferSize; i++) {
//			if(i<byteArray.Length)
//				buffer [i] = byteArray [i];
//		}

		//buffer [bufferSize - 1] = 16;
		//if(client.Connected)
		client.Send(byteArray,byteArray.Length, SocketFlags.None);
		Debug.Log ("send");

	}

	void send (Socket client)
	{
		if (pngData == null)
			return;
		client.Send(pngData,pngData.Length, SocketFlags.None);
		Debug.Log ("send");
	}
	
	//string dataStr="XXX";
	int length;
	void OnGUI() {
		//GUI.Label(new Rect(0,Screen.height-100,800,50),dataStr+"");
		GUI.Label(new Rect(50,Screen.height-50,800,50),length+"");
		dataStr = GUI.TextField(new Rect(50,Screen.height-100,800,50),dataStr);
		
	}
}


