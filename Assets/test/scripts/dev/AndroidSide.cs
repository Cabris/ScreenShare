using UnityEngine;
using System.Collections;
using Phoni;
using System;
using System.Text;


public class AndroidSide : MonoBehaviour {

	public string ipAddress;
	public int port;
	public PhoniControllerForUnity phoniController;
	public CameraImageAccess qrCamera;
	public Renderer r;
	Texture2D tex;
	GUIStyle myStyle;
	int size=50;
	private string _networkInfoPrefKey = "NetworkInfoPref";

	void Awake() {
		#if UNITY_ANDROID
		if(Input.gyro.enabled == false) {
			Input.gyro.enabled = true;
		}
		#endif		
	}

	// Use this for initialization
	void Start () {
		myStyle= new GUIStyle();
		myStyle.fontSize = size;
		phoniController.CommandEventFromGames += ProcessCommand;
		qrCamera.OnQRcodeRead+=this.OnQRcodeRead;
		NetworkInfo info = new NetworkInfo();
		if(info.Parse(PlayerPrefs.GetString(_networkInfoPrefKey, ""))) {
			ipAddress = info.ipAddress;
			port = info.port;
		}
		tex=new Texture2D(128,128);
		r.material.mainTexture=tex;
	}

	[SerializeField]
	float p=0.5f;
	float c=0;
	string dataStr="";

	void NewMethod (PhoniDataPort gamePort)
	{
		dataStr = gamePort.MyString.ReceivedData;
		byte[] data = Convert.FromBase64String (dataStr);
		tex.LoadImage (data);
	}

	// Update is called once per frame
	void Update () {
		if(PhoniInput.Game.Count > 0) {
			// Get the first connected game (not necessarily with ID 0)
			PhoniDataPort gamePort = PhoniInput.Game.GetPortList()[0];

			PhoniMotionData motionData = gamePort.MotionData.SendingData;
			motionData.acceleration = Input.acceleration;
			motionData.gyro = new PhoniGyroscope(Input.gyro);

			if(c>=p){
				try {
					NewMethod (gamePort);
				} catch (Exception e) {
					throw e;
					Debug.Log(e.Message);
				}
				c=0;
			}else
				c+=Time.deltaTime;
		}
	}

	void OnGUI() {
		myStyle = new GUIStyle(GUI.skin.button);
		myStyle.fontSize = size;
		if(PhoniInput.Game.Count == 0) {
			//qrCamera.Pause(false);
			//GUI.BeginGroup(new Rect(Screen.width/2 - 100, Screen.height/2 - 120, 500, 500));
			GUILayout.BeginVertical(myStyle);

			GUILayout.Label( "server address",myStyle);
			ipAddress = GUILayout.TextField(ipAddress,myStyle);
			GUILayout.Label( "server port",myStyle);
			port = int.Parse(GUILayout.TextField(port.ToString(),myStyle));
			if(GUILayout.Button("Connect",myStyle)) {
				PhoniPlayerController.PlayerClientConnect(ipAddress, port);
				PlayerPrefs.SetString(_networkInfoPrefKey, (new NetworkInfo(ipAddress, port)).ToString());
			}

			GUILayout.EndVertical();

		}
		else {
			PhoniDataPort gamePort = PhoniInput.Game.GetPortList()[0];

			GUILayout.BeginVertical(myStyle);

			GUILayout.Label( "Local Address: " + gamePort.LocalIPAddress + " : " 
			                + gamePort.LocalPort,myStyle);
			GUILayout.Label( "Game Address: " + ipAddress + " : " 
			                + port,myStyle);
			if(GUILayout.Button( "Disconnect",myStyle)) {
				PhoniInput.RemovePhoniDataPort(gamePort);
				qrCamera.Pause(false);
			}

			GUILayout.EndVertical();
		}
		if(GUI.Button(new Rect(Screen.width-200,Screen.height-50,200,50),"exit",myStyle))
			Application.Quit();
		GUI.Label(new Rect(0,Screen.height-50,500,50),dataStr.Length+"",myStyle);
	}

	private void ProcessCommand(PhoniDataPort port, PhoniCommandInfo info) {
		PhoniDataBase data = info.data;
		switch(info.command) {
			#if UNITY_ANDROID
		case PhoniCommandCode.COMMAND_RUMBLE:
			Handheld.Vibrate();
			break;
		case PhoniCommandCode.COMMAND_LOST_CONNECTION:
		case PhoniCommandCode.COMMAND_DISCONNECT:
			PhoniDataPort gamePort = PhoniInput.Game.GetPortList()[0];
			PhoniInput.RemovePhoniDataPort(gamePort);
			qrCamera.Pause(false);
			break;
			#endif
		}
	}

	private void OnQRcodeRead(string msg){
		string[] s=msg.Split(':');
		ipAddress=s[0];
		port=int.Parse(s[1]);
		qrCamera.Pause(true);
	}
}
