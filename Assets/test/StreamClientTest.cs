using UnityEngine;
using System.Collections;
using System.Net.Sockets;
using System;
public class StreamClientTest : MonoBehaviour {
	GUIStyle myStyle;
	Texture2D tex;
	TcpClient tcpClient;
	NetworkStream netStream;
	static int bufferSize=StreamServerTest.bufferSize;
	byte[] buffer=new byte[bufferSize];
	string ipAddress="127.0.0.1";

	float p=0.03f;
	float c=0;

	void Start () {
		// Create a texture in DXT1 format
		Application.runInBackground=true;
		tex=new Texture2D(4, 4, TextureFormat.RGB24, false);
		renderer.material.mainTexture = tex;
		tcpClient = new TcpClient ();
		ipAddress="192.168.1.48";
	}
	string dataStr="XXX";
	int length;
	// Update is called once per frame
	void Update () {

		if (c >= p) {
			if (tcpClient.Connected&&netStream.CanRead)
			{
				//receive ();
				byte[] lengthData=new byte[4];
				netStream.Read(lengthData,0,4);
				netStream.Read(buffer,0,bufferSize);
				length=BitConverter.ToInt32(lengthData,0);
				byte[] data=new byte[length];
				for(int i=0;i<length;i++){
					data[i]=buffer[i];
				}

				dataStr = System.Text.Encoding.Default.GetString ( data );

				if(length==0){
					throw new Exception();
				}
				Debug.Log(dataStr.GetType());
			}
			c = 0;
		}
		else
			c += Time.deltaTime;

	}

	void receive ()
	{
		netStream.Read (buffer, 0, bufferSize);
		tex.LoadImage (buffer);
	}

	void connect ()
	{
		tcpClient.Connect (ipAddress, 8080);
		netStream = tcpClient.GetStream ();
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
		GUILayout.Label(dataStr,myStyle);
		GUILayout.EndVertical();
	}
}
