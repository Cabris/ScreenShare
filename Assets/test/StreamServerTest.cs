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
	public static int bufferSize=50000;
	byte[] buffer=new byte[bufferSize];
	byte iniByte;
	float p=0.03f;
	float c=0;
	
	void Start () {
		Application.runInBackground=true;
		tex=new Texture2D(0,0,TextureFormat.RGB24,false);
		server = new ImageStreamingServer();
		server.Start();
		iniByte=buffer[0];
	}
	
	void OnApplicationQuit() {
		server.Stop();
	}
	string dataStr="XXX";
	void OnPostRender ()
	{
		if (c >= p) {
			if (server.netStream != null && server.netStream.CanWrite) {
				//send ();
				byte[] byteArray = System.Text.Encoding.Default.GetBytes ( dataStr );
				byte[] lengthData=BitConverter.GetBytes(byteArray.Length);
				for (int i = 0; i < bufferSize; i++) {
					if(i<byteArray.Length)
						buffer [i] = byteArray [i];
					else
						buffer[i]=iniByte;
				}
				server.netStream.Write(lengthData,0,4);
				server.netStream.Write(buffer,0,bufferSize);
			}

			c = 0;
		}
		else
			c += Time.deltaTime;
		
	}
	
	void send ()
	{
		tex.ReadPixels (new Rect (0, 0, 64, 64), 0, 0, false);
		byte[] pngData = tex.EncodeToPNG ();
		length = pngData.Length;
		if (pngData.Length > bufferSize)
			throw new Exception ("buffer too small!");
		
		for (int i = 0; i < bufferSize; i++) {
			if(i<pngData.Length)
				buffer [i] = pngData [i];
			else
				buffer[i]=iniByte;
		}
		server.netStream.Write (buffer, 0, bufferSize);
		server.netStream.Flush ();
		
	}
	
	//string dataStr="XXX";
	int length;
	void OnGUI() {
		//GUI.Label(new Rect(0,Screen.height-100,800,50),dataStr+"");
		GUI.Label(new Rect(50,Screen.height-50,800,50),length+"");
		dataStr = GUI.TextField(new Rect(50,Screen.height-100,800,50),dataStr);
		
	}
}


