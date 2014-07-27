using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Phoni;

public class PhoniPlayerSample : MonoBehaviour {
	
	
	public string ipAddress;
	public int port;
	
	public PhoniControllerForUnity phoniController;
		
	private Rect touchRect;
	
	public VirtualJoystickController joystick;
	public VirtualButtonController[] virtualButtons;
	
	public GameObject alterUI;
	private bool isAlterUIOn;
	
	private string _networkInfoPrefKey = "NetworkInfoPref";
		
	void Awake() {
#if UNITY_ANDROID
		if(Input.gyro.enabled == false) {
			Input.gyro.enabled = true;
		}
#endif		
		touchRect = new Rect(0, 0, Screen.width, Screen.height);
	}
	
	// Use this for initialization
	void Start () {
		phoniController.CommandEventFromGames += ProcessCommand;
		alterUI.SetActiveRecursively(false);
		isAlterUIOn = false;
		//read preference
		NetworkInfo info = new NetworkInfo();
		if(info.Parse(PlayerPrefs.GetString(_networkInfoPrefKey, ""))) {
			ipAddress = info.ipAddress;
			port = info.port;
		}
	}
	
	// Update is called once per frame
	void Update () {
		if(PhoniInput.Game.Count > 0) {
			// Get the first connected game (not necessarily with ID 0)
			PhoniDataPort gamePort = PhoniInput.Game.GetPortList()[0];
			
			PhoniTouchData touchData = gamePort.TouchData.SendingData;	
			touchData.touches = PhoniTouch.ConvertTouchArray(Input.touches, touchRect);
			touchData.multiTouchEnabled = Input.multiTouchEnabled;
			
			PhoniMotionData motionData = gamePort.MotionData.SendingData;
			motionData.acceleration = Input.acceleration;
			motionData.gyro = new PhoniGyroscope(Input.gyro);
			
			if(isAlterUIOn) {
				gamePort.AnalogData.SendingData.left = joystick.analogData;
				
				PhoniButton buttonData = PhoniButton.None;
				foreach(VirtualButtonController button in virtualButtons) {
					if(button.IsDown) {
						buttonData |= button.phoniButton;
					}
				}
				gamePort.ButtonData.SendingData.buttons = buttonData;
			}
						
		}
	}
	
	void FixedUpdate () {
		
	}
	
	
	void OnGUI() {
		if(PhoniInput.Game.Count == 0) {
			GUI.BeginGroup(new Rect(Screen.width/2 - 100, Screen.height/2 - 120, 300, 200));
			GUI.Label(new Rect(0,0,100,25), "server address");
			ipAddress = GUI.TextField(new Rect(0,25,200,20),ipAddress);
			GUI.Label(new Rect(0,50,100,25), "server port");
			port = int.Parse(GUI.TextField(new Rect(0,75,200,20),port.ToString()));
			if(GUI.Button(new Rect(0,120,100,40), "Connect")) {
				PhoniPlayerController.PlayerClientConnect(ipAddress, port);
				PlayerPrefs.SetString(_networkInfoPrefKey, (new NetworkInfo(ipAddress, port)).ToString());
			}
			GUI.EndGroup();
			if(isAlterUIOn) {
				isAlterUIOn = false;
				alterUI.SetActiveRecursively(false);
			}
		}
		else {
			PhoniDataPort gamePort = PhoniInput.Game.GetPortList()[0];
			GUI.Label(new Rect(20, 20, 300, 25), "Local Address: " + gamePort.LocalIPAddress + " : " + gamePort.LocalPort);
			GUI.Label(new Rect(280, 20, 300, 25), "Game Address: " + ipAddress + " : " + port);
			if(GUI.Button(new Rect(540,10,100,40), "Disconnect")) {
				PhoniInput.RemovePhoniDataPort(gamePort);
			}
		}
		
	}
	
	private void ProcessCommand(PhoniDataPort port, PhoniCommandInfo info) {
		PhoniDataBase data = info.data;
		switch(info.command) {
#if UNITY_ANDROID
		case PhoniCommandCode.COMMAND_RUMBLE:
			Handheld.Vibrate();
			break;
#endif
		case PhoniCommandCode.COMMAND_UI_SWITCH:
			if(isAlterUIOn != data.GetData<bool>()) {
				isAlterUIOn = data.GetData<bool>();
				if(isAlterUIOn) {
					alterUI.SetActiveRecursively(true);
				}
				else {
					alterUI.SetActiveRecursively(false);
					TurnOffAlterUI();
				}
			}
			break;
		}
	}
	
	private void TurnOffAlterUI() {
		if(PhoniInput.Game.Count > 0) {
			PhoniDataPort gamePort = PhoniInput.Game.GetPortList()[0];
			
			gamePort.AnalogData.SendingData.left = Vector2.zero;
			gamePort.ButtonData.SendingData.buttons = PhoniButton.None;
		}
	}
	
}
