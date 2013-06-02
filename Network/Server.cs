using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

public class Server : MonoBehaviour
{

    #region Private Members
	private Dictionary<string, Transform> prefabs = new Dictionary<string, Transform>();
    private List<NetworkPlayer> players = new List<NetworkPlayer>();
    #endregion

    #region Public Members
    public List<string> prefabKeys;
	public List<Transform> prefabValues;

	public string portTF;
	public int maxCo = 32;
    #endregion
    
    #region Initialization
    void Awake() {
		// Creating the prefabs Dictionary
		for (int i = 0; i < prefabValues.Count; i++) {
			prefabs[prefabKeys[i]] = prefabValues[i];
		}
	}
    #endregion

    #region Methods
    void LaunchServer() {
		Debug.Log("Starting server");
		bool useNat = false;
		int port = 0;
		Int32.TryParse(portTF, out port);
		Network.InitializeServer(maxCo, port, useNat);
	}

	void ShutdownServer() {
		Network.Disconnect();
	}

	void OnPlayerConnected(NetworkPlayer player) {
        players.Add(player);
		Debug.Log("Player " + (players.Count) + " connected from " + player.ipAddress + ":" + player.port);
	}

    void OnPlayerDisconnected(NetworkPlayer player) {
        Debug.Log("Clean up after player " + player);
        DestroyPlayer(player);
        players.Remove(player);
    }

	void OnDisconnectedFromServer(NetworkDisconnection info) {
		if (Network.isServer) {
			Debug.Log("Local server connection disconnected");
            foreach (NetworkPlayer player in players) {
                DestroyPlayer(player);
            }
            players.Clear();
		}
	}

    /**
     * Destroys a player (its RPCs, references, etc.)
     */
    void DestroyPlayer(NetworkPlayer player) {
        Network.RemoveRPCs(player);
        Network.DestroyPlayerObjects(player);
    }
    #endregion

    #region GUI
    void OnGUI() {
		if (!Network.isServer) {
			GUI.Label(new Rect(15, 45, 80, 20), "Server Port");
			portTF = GUI.TextField(new Rect(95, 45, 120, 20), portTF, 15);
			if (GUI.Button(new Rect(Screen.width / 2 - 60, Screen.height / 2 - 15, 120, 30), "Start server")) {
				LaunchServer();
			}
		} else {
			GUI.Box(new Rect(Screen.width / 2 - 60, Screen.height / 2 - 15, 120, 30), "Server is running");
			if (GUILayout.Button("Shutdown")) {
				ShutdownServer();
			}
		}
    }
    #endregion

}
