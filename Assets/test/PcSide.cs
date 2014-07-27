using UnityEngine;
using System.Collections;
using Phoni;
using System.Net;
using System;
using System.Text;

public class PcSide : MonoBehaviour {
	public PhoniControllerForUnity phoniController;
	public ScreenImg screenImg;
	public QRcodeCreater qrCreater;
	public int localPort;
	private string _portPrefKey = "PortPref";
	private Texture2D tex;
	
	void Start () {
		phoniController.CommandEventFromPlayers += ProcessCommand;
		Application.runInBackground=true;
		localPort = PlayerPrefs.GetInt(_portPrefKey, 4000);
		tex=screenImg.tex;
	}
	
	[SerializeField]
	float p;
	[SerializeField]
	float c=0;

	void sendImg (PhoniDataPort port)
	{
		if (c >= p) {
			string data = "PC:" + Time.time;
			byte[] pngBytes = tex.EncodeToPNG ();
			data = Convert.ToBase64String (pngBytes);
			port.MyString.SendingData=data;
			Debug.Log ("data:" + data.Length);
			c = 0;
		}
		else
			c += Time.deltaTime;
	}
	
	// Update is called once per frame
	void Update () {
		if(PhoniGameController.IsStarted) {
			if(PhoniInput.Player.Contains(0)) {
				PhoniDataPort port=PhoniInput.Player[0];
				if(port.IsActive&&port.RemotePlatform == PhoniPlatformCode.PLATFORM_ANDROID) {
					sendImg (port);
				}
			}
		}
	
	}
	bool isCreateQr=false;
	void OnGUI() {
		if(!PhoniGameController.IsStarted) {
			isCreateQr=false;
			if(GUI.Button(new Rect(Screen.width/2-50,Screen.height/2-30,100,60), "Start Server")) {
				PhoniGameController.GameClientStart();
				PlayerPrefs.SetInt(_portPrefKey, localPort);
			}
			
		}
		else {
			if(!isCreateQr){
				qrCreater.CreateCode(PhoniGameController.GameIPAddress + ":" + PhoniGameController.GamePort);
				isCreateQr=true;
			}
			// server info:
			GUI.Label(new Rect(20, 20, 300, 25), "Game Address: " + PhoniGameController.GameIPAddress + " : " + PhoniGameController.GamePort);
			GUI.Label(new Rect(320, 20, 300, 25), "Connected Players: " + PhoniInput.Player.Count);
			GUI.Label(new Rect(520, 20, 300, 25), "Active Players: " + PhoniInput.Player.GetPortListByActivity(true).Count);
			if(GUI.Button(new Rect(20, 45, 180, 25), "Shutdown Game")) {
				PhoniGameController.GameClientShutdown();
			}
			if(PhoniInput.Player.Count > 0) {
				if(GUI.Button(new Rect(220, 45, 180, 25), "Disconnect All Players")) {
					PhoniInput.Player.Clear();
				}
			}
			
			// for the sample, only show data for playerID 0 - 3 (not necessarily the first 4 connected players)
			if(PhoniInput.Player.Contains(0)) {
				PlayerInfoGUI(new Rect(40, 180, 500, 350), PhoniInput.Player[0], Color.green);
				//	CreateAvatar(0, Color.green);
			}
		}
	}
	
	private void PlayerInfoGUI(Rect groupRect, PhoniDataPort port, Color touchColor) {
		LineHelper top = new LineHelper(0);		
		GUI.BeginGroup(groupRect);
		GUI.Label(new Rect(0, top.Use(25), 300, 25), "Player ID: " + port.ID);
		GUI.Label(new Rect(0, top.Use(25), 300, 25), "Platform: " + port.RemotePlatform);
		GUI.Label(new Rect(0, top.Use(25), 300, 25), "Address: " + port.RemoteIPAddress + " : " + port.RemotePort);
		GUI.Label(new Rect(0, top.Use(25), 300, 25), "State: " + (port.IsActive ? "Active" : "Inactive"));
		// active
		if(port.IsActive) {
			if(port.RemotePlatform == PhoniPlatformCode.PLATFORM_ANDROID) {
				
				GUI.Label(new Rect(0, top.Use(25), 300, 25), "Acceleration: " + port.MotionData.ReceivedData.acceleration);
				GUI.Label(new Rect(0, top.Use(25), 300, 25), "Angular Velocity gyro: " + port.MotionData.ReceivedData.gyro.rotationRate);
				
				if(port.RemotePlatform == PhoniPlatformCode.PLATFORM_ANDROID) {
					if(GUI.Button(new Rect(0, top.Get(), 70, 25), "Rumble")) {
						port.SendCommand(PhoniCommandCode.COMMAND_RUMBLE);
					}
				}
			}
		}
		if(GUI.Button(new Rect(120, top.Use(25), 100, 25), "Disconnect")) {
			PhoniInput.RemovePhoniDataPort(port);
		}
		GUI.EndGroup();
	}
	
	private void ProcessCommand(PhoniDataPort port, PhoniCommandInfo info) {
		PhoniDataBase data = info.data;
		switch(info.command) {
		case PhoniCommandCode.COMMAND_LOST_CONNECTION:
		case PhoniCommandCode.COMMAND_DISCONNECT:
			break;
		}
	}
}
