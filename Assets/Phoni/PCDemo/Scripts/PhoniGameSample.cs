using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using Phoni;

public class PhoniGameSample : MonoBehaviour {
	
	
	public int localPort;
	
	private bool useFixedPort = true;
	
	public PhoniControllerForUnity phoniController;
	
	private TouchViewer touchViewer;
	
	private bool[] isAlterUIOn;
	private GameObject[] avatars;
	
	
	private string _portPrefKey = "PortPref";
	private string _useFixedPortPrefKey = "UseFixedPortPref";
	
	// Use this for initialization
	void Start () {
		phoniController.CommandEventFromPlayers += ProcessCommand;
		touchViewer = GetComponent<TouchViewer>();
		isAlterUIOn = new bool[4];
		for(int i = 0; i < 4; ++i) {
			isAlterUIOn[i] = false;
		}
		avatars = new GameObject[4];
		
		localPort = PlayerPrefs.GetInt(_portPrefKey, 4000);
		useFixedPort = (PlayerPrefs.GetInt(_useFixedPortPrefKey, 0) == 1);
	}
	
	// Update is called once per frame
	void Update () {
	}
			
	void OnGUI() {
		if(!PhoniGameController.IsStarted) {
			useFixedPort = GUI.Toggle(new Rect(400, 280, 200, 25), useFixedPort, "Manually Select Port");
			if(useFixedPort) {
				GUI.Label(new Rect(400,300,100,60), "listen port");
				localPort = int.Parse(GUI.TextField(new Rect(400,320,200,20),localPort.ToString()));
			}
			if(GUI.Button(new Rect(430,360,100,60), "Start Server")) {
				if(useFixedPort) {
					// try start server with manually selected port
					PhoniGameController.GameClientStart(localPort);
				}
				else {
					// start server with a system assigned port
					PhoniGameController.GameClientStart();
				}
				PlayerPrefs.SetInt(_portPrefKey, localPort);
				PlayerPrefs.SetInt(_useFixedPortPrefKey, (useFixedPort ? 1 : 0));
			}
		}
		else {
			
			// server info:
			GUI.Label(new Rect(20, 20, 300, 25), "Game Address: " + PhoniGameController.GameIPAddress + " : " + PhoniGameController.GamePort);
			GUI.Label(new Rect(320, 20, 300, 25), "Connected Players: " + PhoniInput.Player.Count);
			GUI.Label(new Rect(520, 20, 300, 25), "Active Players: " + PhoniInput.Player.GetPortListByActivity(true).Count);
			if(GUI.Button(new Rect(20, 45, 180, 25), "Shutdown Game")) {
				PhoniGameController.GameClientShutdown();
				touchViewer.Clear();
			}
			if(PhoniInput.Player.Count > 0) {
				if(GUI.Button(new Rect(220, 45, 180, 25), "Disconnect All Players")) {
					PhoniInput.Player.Clear();
					touchViewer.Clear();
				}
			}
			
			// for the sample, only show data for playerID 0 - 3 (not necessarily the first 4 connected players)
			if(PhoniInput.Player.Contains(0)) {
				PlayerInfoGUI(new Rect(40, 80, 500, 350), PhoniInput.Player[0], Color.green);
				CreateAvatar(0, Color.green);
			}
			else {
				DestroyAvatar(0);
				if(isAlterUIOn[0]) {
					isAlterUIOn[0] = false;
				}
			}
			if(PhoniInput.Player.Contains(1)) {
				PlayerInfoGUI(new Rect(40, 430, 500, 350), PhoniInput.Player[1], Color.red);
				CreateAvatar(1, Color.red);
			}
			else {
				DestroyAvatar(1);
				if(isAlterUIOn[1]) {
					isAlterUIOn[1] = false;
				}
			}
			if(PhoniInput.Player.Contains(2)) {
				PlayerInfoGUI(new Rect(540, 80, 500, 350), PhoniInput.Player[2], Color.yellow);
				CreateAvatar(2, Color.yellow);
			}
			else {
				DestroyAvatar(2);
				if(isAlterUIOn[2]) {
					isAlterUIOn[2] = false;
				}
			}
			if(PhoniInput.Player.Contains(3)) {
				PlayerInfoGUI(new Rect(540, 430, 500, 350), PhoniInput.Player[3], Color.blue);
				CreateAvatar(3, Color.blue);
			}
			else {
				DestroyAvatar(3);
				if(isAlterUIOn[3]) {
					isAlterUIOn[3] = false;
				}
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
			if(port.RemotePlatform == PhoniPlatformCode.PLATFORM_ANDROID ||
				port.RemotePlatform == PhoniPlatformCode.PLATFORM_VITA) {
				GUI.color = touchColor;
				GUI.Label(new Rect(0, top.Use(25), 300, 25), "Touch Count: " + port.TouchData.ReceivedData.TouchCount);
				GUI.color = Color.white;
				if(touchViewer != null) {
					touchViewer.ShowTouch(port.ID, port.TouchData.ReceivedData, touchColor);
				}
				GUI.Label(new Rect(0, top.Use(25), 300, 25), "Acceleration: " + port.MotionData.ReceivedData.acceleration);
				GUI.Label(new Rect(0, top.Use(25), 300, 25), "Angular Velocity: " + port.MotionData.ReceivedData.gyro.rotationRate);
				
				if(port.RemotePlatform == PhoniPlatformCode.PLATFORM_ANDROID) {
					if(isAlterUIOn[port.ID]) {
						GUI.Label(new Rect(0, top.Use(25), 300, 25), "Buttons: " + port.ButtonData.ReceivedData.buttons); 
						GUI.Label(new Rect(0, top.Use(25), 300, 25), "Analog Left: " + port.AnalogData.ReceivedData.left);
					}
					if(GUI.Button(new Rect(0, top.Get(), 70, 25), "Rumble")) {
						port.SendCommand(PhoniCommandCode.COMMAND_RUMBLE);
					}
					if(GUI.Button(new Rect(80, top.Use(30), 160, 25), isAlterUIOn[port.ID] ? "Hide Alternative UI" : "Show Alternative UI")) {
						isAlterUIOn[port.ID] = !isAlterUIOn[port.ID];
						port.SendCommand(PhoniCommandCode.COMMAND_UI_SWITCH, new PhoniData<bool>(isAlterUIOn[port.ID]));
					}
				}
				else if(port.RemotePlatform == PhoniPlatformCode.PLATFORM_VITA) {
					GUI.Label(new Rect(0, top.Use(25), 300, 25), "Buttons: " + port.ButtonData.ReceivedData.buttons); 
					GUI.Label(new Rect(0, top.Use(25), 300, 25), "Analog Left: " + port.AnalogData.ReceivedData.left);
					GUI.Label(new Rect(0, top.Use(25), 300, 25), "Analog Right: " + port.AnalogData.ReceivedData.right);
				}
			}
		}
		else {
			touchViewer.Clear(port.ID);
			DestroyAvatar(port.ID);
		}
		if(GUI.Button(new Rect(0, top.Use(25), 100, 25), "Disconnect")) {
			touchViewer.Clear(port.ID);
			PhoniInput.RemovePhoniDataPort(port);
		}
		GUI.EndGroup();
	}
	
	private void CreateAvatar(int playerID, Color color) {
		if(avatars[playerID] != null) {
			return;
		}
		GameObject go = GameObject.CreatePrimitive(PrimitiveType.Cube);
		go.renderer.material.color = color;
		GameObject.Destroy(go.collider);
		go.transform.localScale = new Vector3(1.5f,2f,0.5f);
		go.transform.eulerAngles = new Vector3(90,90,0);
		AvatarController controller = go.AddComponent<AvatarController>();
		controller.Init(playerID);
		avatars[playerID] = go;
	}
	
	private void DestroyAvatar(int playerID) {
		if(avatars[playerID] != null) {
			GameObject.Destroy(avatars[playerID]);
			avatars[playerID] = null;
		}
	}
	
	private Rect GetRect(float left, float top, float width, float height, Vector2 offset) {
		return new Rect(left + offset.x, top + offset.y, width, height);
	}
	
	
	private void ProcessCommand(PhoniDataPort port, PhoniCommandInfo info) {
		PhoniDataBase data = info.data;
		switch(info.command) {
		case PhoniCommandCode.COMMAND_LOST_CONNECTION:
		case PhoniCommandCode.COMMAND_DISCONNECT:
			touchViewer.Clear(port.ID);
			break;
		}
	}
	
	
}

public class LineHelper {
	float height;
	
	public LineHelper(float height) {
		this.height = 0;
	}
	
	public float Get() {
		return height;
	}
	
	public float Use(float height) {
		float result = this.height;
		this.height += height;
		return result;
	}
}
