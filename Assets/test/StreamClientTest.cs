using UnityEngine;
using System.Collections;
using System.Net.Sockets;
using System.Net;
using System;
using System.Threading;

public class StreamClientTest : MonoBehaviour {
	GUIStyle myStyle;
	Texture2D tex;
	
	Socket client;
	int bufferSize=100000;
	byte[] buffer;
	string ipAddress="127.0.0.1";

	int Interval=33;
	Thread _Thread;
	
	void Start () {
		buffer=new byte[bufferSize];
		Application.runInBackground=true;
		tex=new Texture2D(4, 4, TextureFormat.RGB24, false);
		renderer.material.mainTexture = tex;
		client = new Socket(AddressFamily.InterNetwork,SocketType.Stream, ProtocolType.Tcp);
		client.ReceiveBufferSize =bufferSize;
		//ipAddress="10.120.168.114";
		_Thread = new Thread(new ThreadStart(receive));
		_Thread.IsBackground = true;
	}
	string dataStr="XXX";
	int length;
	// Update is called once per frame
	void Update () {
			if(!isReadData)
				tex.LoadImage (buffer);
	}
	
	void OnApplicationQuit() {
		isRunning = false;
		if (_Thread.IsAlive) {
			_Thread.Join ();
			_Thread.Abort ();
		}
	}
	bool isRunning=false;
	bool isReadData=true;

	void receive ()
	{
		Debug.Log ("start receive: "+client.Connected);
		isRunning = true;
		while (client.Connected&&isRunning) {
			isReadData=true;
			Debug.Log("receiveing");
			buffer = new byte[bufferSize];
			client.Receive (buffer, bufferSize, SocketFlags.None);
			Debug.Log("received");

			//dataStr = System.Text.Encoding.Default.GetString (buffer);
			string b64str = Convert.ToBase64String (buffer);
			//Debug.Log ("str:" + dataStr );
			length=client.Available;
			Debug.Log("length:" + length + ", dataStr.Length:" + b64str.Length);
			Debug.Log ("b64:" + b64str);
			isReadData=false;
			Thread.Sleep(Interval);
		}
	}
	
	void connect ()
	{
		IPAddress IP = IPAddress.Parse(ipAddress);
		IPEndPoint IPE = new IPEndPoint(IP, 8080);
		client.Connect(IPE);

		_Thread.Start();
		
	}
	
	void OnGUI() {
		
		GUI.Label(new Rect(50,Screen.height-50,800,50),length+"");
		
		myStyle = new GUIStyle(GUI.skin.button);
		myStyle.fontSize = 50;
		GUILayout.BeginVertical(myStyle);
		GUILayout.Label( "server address",myStyle);
		ipAddress = GUILayout.TextField(ipAddress,myStyle);
		if(GUILayout.Button("Connect",myStyle)) {
			connect ();
		}
		//GUILayout.Label(dataStr,myStyle);
		GUILayout.EndVertical();
	}
}
