using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Client : MonoBehaviour
{

    #region Private Members
    private int serverPort;
	private Dictionary<string, Transform> prefabs = new Dictionary<string, Transform>();
    #endregion
    
    #region Public Members
    public string serverIP;
	public string portTF;
	
    public delegate void GuiDelegate();
	public GuiDelegate guiRenderer;

	public List<string> prefabKeys;
	public List<Transform> prefabValues;
    #endregion

    #region Initialization
    void Awake(){
		// Creating the prefabs Dictionary
		for (int i = 0; i < prefabValues.Count; i++) {
			prefabs[prefabKeys[i]] = prefabValues[i];
		}
	}

	// Use this for initialization
	void Start () {
		guiRenderer = ClientInitializationGUI;
	}
    #endregion

    #region Connecting
    void ConnectToServer() {
		Debug.Log("Connecting...");
		Network.Connect(serverIP, serverPort);
	}

	void OnConnectedToServer() {
		Debug.Log("Connected to server");
		guiRenderer = ClientConnectedGUI;
		Spawn("Unit");
	}

	void OnFailedToConnect(NetworkConnectionError error) {
		Debug.Log("Could not connect to server: " + error);
		guiRenderer = ClientInitializationGUI;
	}

	void Spawn(string prefabName) {
        Network.Instantiate(prefabs[prefabName],
            new Vector3(Random.Range(-500f, 500f), 35f, Random.Range(-500f, 500f)),
            Quaternion.identity,
            0);
	}
	#endregion

	#region Disconnection
	void DisconnectFromServer() {
		Debug.Log("Disconnecting...");
		Network.Disconnect();
	}

	void OnDisconnectedFromServer(NetworkDisconnection info) {
		if (info == NetworkDisconnection.LostConnection) {
			Debug.Log("Lost connection to the server");
		} else {
			Debug.Log("Successfully diconnected from the server");
		}
		guiRenderer = ClientInitializationGUI;
	}
	#endregion

	#region GUI
	void OnGUI() {
		guiRenderer();
	}

	void ClientInitializationGUI() {
		GUI.Label(new Rect(15, 15, 80, 20), "Server IP");
		serverIP = GUI.TextField(new Rect(95, 15, 120, 20), serverIP, 15);
		
		GUI.Label(new Rect(15, 45, 80, 20), "Server Port");
		portTF = GUI.TextField(new Rect(95, 45, 120, 20), portTF, 15);

		if (GUI.Button(new Rect(95, 70, 120, 30), "Connect")) {
			System.Int32.TryParse(portTF, out serverPort);
			ConnectToServer();
		}
	}

	void ClientConnectedGUI() {
		if (Network.isClient) {
			if (GUILayout.Button("Disconnect")) {
				DisconnectFromServer();
			}
		}
	}
	#endregion

}
