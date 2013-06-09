using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using FPS.Game.Managers;

public class Networker : MonoBehaviour
{

    #region Private Members
    private int serverPort;
    private Dictionary<string, Transform> prefabs = new Dictionary<string, Transform>();
    #endregion

    #region Public Members
    public string serverIP;
    public string portTF;
    public int maxCo = 32;

    public delegate void GuiDelegate();
    public GuiDelegate guiRenderer;

    public List<string> prefabKeys;
    public List<Transform> prefabValues;
    public List<NetworkPlayer> players = new List<NetworkPlayer>();
    public Dictionary<NetworkPlayer, Unit> playerScripts = new Dictionary<NetworkPlayer, Unit>();
    #endregion

    #region Initialization
    void Awake() {
        // Creating the prefabs Dictionary
        for (int i = 0; i < prefabValues.Count; i++) {
            prefabs[prefabKeys[i]] = prefabValues[i];
        }
    }

    // Use this for initialization
    void Start() {
        guiRenderer = MenuGUI;
    }
    #endregion

    #region GUI
    void OnGUI() {
        guiRenderer();
    }

    void MenuGUI() {
        // Client GUI
        GUI.Label(new Rect(15, 15, 80, 20), "Server IP");
        serverIP = GUI.TextField(new Rect(95, 15, 120, 20), serverIP, 15);

        GUI.Label(new Rect(15, 45, 80, 20), "Server Port");
        portTF = GUI.TextField(new Rect(95, 45, 120, 20), portTF, 15);

        if (GUI.Button(new Rect(95, 70, 120, 30), "Connect")) {
            System.Int32.TryParse(portTF, out serverPort);
            ConnectToServer();
        }

        // Server GUI
        GUI.Label(new Rect(15, 45, 80, 20), "Server Port");
        portTF = GUI.TextField(new Rect(95, 45, 120, 20), portTF, 15);
        if (GUI.Button(new Rect(Screen.width / 2 - 60, Screen.height / 2 - 15, 120, 30), "Start server")) {
            LaunchServer();
        }
    }

    void ClientConnectedGUI() {
        if (Network.isClient) {
            if (GUILayout.Button("Disconnect")) {
                DisconnectFromServer();
            }
            if (GameObject.FindGameObjectWithTag("Player") != null) {
                GameObject playerGO = GameObject.FindGameObjectWithTag("Player");
                GUI.Label(new Rect(0, Screen.height - 20, 80, 20), playerGO.GetComponent<Health>().Current.ToString());
            }
        }
    }

    void ServerInitializedGUI() {
        GUI.Box(new Rect(Screen.width / 2 - 60, Screen.height / 2 - 15, 120, 30), "Server is running");
        if (GUILayout.Button("Shutdown")) {
            ShutdownServer();
        }
    }
    #endregion

    #region Networking

    #region Client
    // Connects the client to the server
    void ConnectToServer() {
        Debug.Log("Connecting...");
        Network.Connect(serverIP, serverPort);
    }

    // Triggered on the client when it's connected to the server
    void OnConnectedToServer() {
        Debug.Log("Connected to server");
        guiRenderer = ClientConnectedGUI;
    }

    // Triggered on the client when the connection failed
    void OnFailedToConnect(NetworkConnectionError error) {
        Debug.Log("Could not connect to server: " + error);
        guiRenderer = MenuGUI;
    }

    // Disconnects the client from the server
    public void DisconnectFromServer() {
        Debug.Log("Disconnecting from " + serverIP + ":" + serverPort);
        Network.RemoveRPCs(Network.player);
        Network.Disconnect();
    }

    [RPC]
    void RemoteDisconnect() {
        Debug.Log("Remote disconnect signal received");
        DisconnectFromServer();
    }
    #endregion

    #region Server
    // Launches the server
    void LaunchServer() {
        Debug.Log("Starting server");
        bool useNat = false;
        int port = 0;
        Int32.TryParse(portTF, out port);
        Network.InitializeServer(maxCo, port, useNat);
    }

    // Triggered when the server is initialized
    void OnServerInitialized() {
        guiRenderer = ServerInitializedGUI;
        //LevelManager lm = LevelManager.Instance;
        //lm.Init();
    }

    // Stops the server
    void ShutdownServer() {
        networkView.RPC("RemoteDisconnect", RPCMode.Others); // Make other players disconnect before the server
        Network.RemoveRPCs(Network.player);
        Network.Disconnect();
    }

    // Triggered when a player connects to the server
    void OnPlayerConnected(NetworkPlayer player) {
        Debug.Log("Player " + (players.Count) + " connected from " + player.ipAddress + ":" + player.port);
        SpawnPlayer(player);
        players.Add(player);
        Debug.Log("add player " + player);
    }

    // Spawns a player over the network
    void SpawnPlayer(NetworkPlayer player) {
        string tempPlayerString = player.ToString();
        int playerNumber = Convert.ToInt32(tempPlayerString);

        // Instanciate the new player over the network
        Transform newPlayerTransform = (Transform)Network.Instantiate(
            prefabs["Unit"],
            new Vector3(UnityEngine.Random.Range(0f, 50f), 15f, UnityEngine.Random.Range(0f, 50f)),
            transform.rotation,
            playerNumber);

        // Get the new player's script
        playerScripts.Add(player, newPlayerTransform.GetComponent<Unit>());

        NetworkView playerNetworkView = newPlayerTransform.networkView;
        playerNetworkView.RPC("SetPlayer", RPCMode.AllBuffered, player);
        playerNetworkView.RPC("TakeWeapon", player, "Gun");             // Give the player a default weapon
    }

    // Triggered when a player disconnects from the server
    void OnPlayerDisconnected(NetworkPlayer player) {
        Debug.Log("Clean up after player " + player);
        DestroyPlayer(player);
    }

    // Destroys a player (its RPCs, references, etc.)
    public void DestroyPlayer(NetworkPlayer player) {
        players.Remove(player);
        Network.RemoveRPCs(player);
        Network.DestroyPlayerObjects(player);
    }
    #endregion

    // Triggered when the server or client is disconnected
    void OnDisconnectedFromServer(NetworkDisconnection info) {
        if (Network.isServer) {
            Debug.Log("Local server connection disconnected");
            foreach (NetworkPlayer player in players) {
                DestroyPlayer(player);
            }
            players.Clear();
        } else {
            if (info == NetworkDisconnection.LostConnection) {
                Debug.Log("Lost connection to the server");
            } else {
                Debug.Log("Successfully diconnected from the server");
                Camera mainCamera = GameObject.FindWithTag("MainCamera").camera;
                mainCamera.enabled = true;
            }
        }
        guiRenderer = MenuGUI;
    }

    [RPC]
    void DestroyBuffered(NetworkViewID viewID) {
        UnityEngine.Object.Destroy(NetworkView.Find(viewID).gameObject);
    }
    #endregion

}