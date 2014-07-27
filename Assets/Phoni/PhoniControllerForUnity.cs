using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Phoni;

public class PhoniControllerForUnity : MonoBehaviour {
	
	public event PhoniCommandCallback CommandEventFromGames;
	public event PhoniCommandCallback CommandEventFromPlayers;
	
	void Awake() {
		// assign local platform
#if UNITY_EDITOR || UNITY_STANDALONE_WIN || UNITY_STANDALONE_OSX
		PhoniGameController.Init(PhoniPlatformCode.PLATFORM_STANDALONE);
		PhoniPlayerController.Init(PhoniPlatformCode.PLATFORM_STANDALONE);
#elif UNITY_ANDROID
		PhoniGameController.Init(PhoniPlatformCode.PLATFORM_ANDROID);
		PhoniPlayerController.Init(PhoniPlatformCode.PLATFORM_ANDROID);
#endif
	}

	// Use this for initialization
	void Start () {
		DontDestroyOnLoad(gameObject);
		// add default command processor
		CommandEventFromGames += ProcessCommand;
		CommandEventFromPlayers += ProcessCommand;;
	}
	
	// Update is called once per frame
	void Update () {
		
		// get command queues and fire command event
		Dictionary<PhoniDataPort, Queue<PhoniCommandInfo>> playerCommandQueues = PhoniInput.Player.GetCommandQueues();
		if(CommandEventFromPlayers != null) {
			foreach(PhoniDataPort dataPort in playerCommandQueues.Keys) {
				Queue<PhoniCommandInfo> queue = playerCommandQueues[dataPort];
				while(queue.Count > 0) {
					CommandEventFromPlayers(dataPort, queue.Dequeue());
				}
			}
		}
		
		Dictionary<PhoniDataPort, Queue<PhoniCommandInfo>> gameCommandQueues = PhoniInput.Game.GetCommandQueues();
		if(CommandEventFromGames != null) {
			foreach(PhoniDataPort dataPort in gameCommandQueues.Keys) {
				Queue<PhoniCommandInfo> queue = gameCommandQueues[dataPort];
				while(queue.Count > 0) {
					CommandEventFromGames(dataPort, queue.Dequeue());
				}
			}
		}
	}
	
	void LateUpdate() {
		
		// update received data and flush sendin data
		// put it in LateUpdate() to make sure every scripte 
		// will get consistent data in the next Update()
		PhoniInput.Player.UpdateData();
		PhoniInput.Game.UpdateData();
		PhoniInput.Player.FlushData();
		PhoniInput.Game.FlushData();
		
	}
	
	private void ProcessCommand(PhoniDataPort port, PhoniCommandInfo info) {
		PhoniDataBase data = info.data;
		switch(info.command) {
		case PhoniCommandCode.COMMAND_LOST_CONNECTION:
		case PhoniCommandCode.COMMAND_DISCONNECT:
			// disconnect a player/game
			PhoniInput.RemovePhoniDataPort(port);
			break;
		case PhoniCommandCode.COMMAND_SUSPEND:
			// set player/game's activity
			port.SetActive(data.GetData<bool>());
			break;
		}
	}
	
	void OnApplicationQuit() {
		PhoniPlayerController.PlayerClientDisconnectAll();
		PhoniGameController.GameClientShutdown();
	}
	
	void OnApplicationPause(bool pause) {
		foreach(PhoniDataPort game in PhoniInput.Game) {
			game.SendCommand(PhoniCommandCode.COMMAND_SUSPEND, new PhoniData<bool>(!pause));
		}
		foreach(PhoniDataPort player in PhoniInput.Player) {
			player.SendCommand(PhoniCommandCode.COMMAND_SUSPEND, new PhoniData<bool>(!pause));
		}
	}
}
