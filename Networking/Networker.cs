using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Networker : MonoBehaviour
{

    #region Private Members
    private Dictionary<string, Transform> prefabs = new Dictionary<string, Transform>();
    #endregion

    #region Public Members
    public string serverIP;
    public int serverPort;
    public string portTF;
    public int maxCo = 32;

    public delegate void NetworkEvent();
    public static event NetworkEvent ClientConnected, ServerInitialized, Disconnected;

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
    #endregion

    #region Networking

    #region Client
    // Connects the client to the server
    public void ConnectToServer() {
        Debug.Log("Connecting...");
        Network.Connect(serverIP, serverPort);
    }

    // Triggered on the client when it's connected to the server
    void OnConnectedToServer() {
        Debug.Log("Connected to server");
        ClientConnected();
    }

    // Triggered on the client when the connection failed
    void OnFailedToConnect(NetworkConnectionError error) {
        Debug.Log("Could not connect to server: " + error);
        Disconnected();
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
    public void LaunchServer() {
        Debug.Log("Starting server");
        bool useNat = false;
        int port = 0;
        Int32.TryParse(portTF, out port);
        Network.InitializeServer(maxCo, port, useNat);
    }

    // Triggered when the server is initialized
    void OnServerInitialized() {
        ServerInitialized();
    }

    // Stops the server
    public void ShutdownServer() {
        networkView.RPC("RemoteDisconnect", RPCMode.Others); // Make other players disconnect before the server
        Network.RemoveRPCs(Network.player);
        Network.Disconnect();
    }

    // Triggered when a player connects to the server
    void OnPlayerConnected(NetworkPlayer player) {
        Debug.Log("Player " + player + " connected from " + player.ipAddress + ":" + player.port);
        SpawnPlayer(player);
        players.Add(player);
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

        // Give the player a weapon
        //AWeapon newPlayerWeapon = WeaponManager.Instance.GetWeapon("RocketLauncher");
        playerNetworkView.RPC("TakeWeapon", player, "Gun");  // Give the player a default weapon
    }

    // Makes a player respawn
    public void RespawnPlayer(NetworkPlayer player) {
        Vector3 position;
        RaycastHit hit;

        // We make sure he doesn't collide with anything
        do {
            position = new Vector3(UnityEngine.Random.Range(0f, 50f), 15f, UnityEngine.Random.Range(0f, 50f));
        } while (Physics.SphereCast(position, 2f, playerScripts[player].transform.forward, out hit));
        
        // And make him respawn
        playerScripts[player].networkView.RPC("Respawn", RPCMode.AllBuffered, position);
    }

    // Triggered when a player disconnects from the server
    void OnPlayerDisconnected(NetworkPlayer player) {
        Debug.Log("Clean up after player " + player);
        DestroyPlayer(player);
    }

    // Destroys a player (its RPCs, references, etc.)
    public void DestroyPlayer(NetworkPlayer player) {
        foreach (KeyValuePair<NetworkPlayer, Unit> script in playerScripts) {
            if (player == script.Value.GetComponent<FPSController>().owner) {
                Network.RemoveRPCs(script.Value.gameObject.networkView.viewID);
                Network.Destroy(script.Value.gameObject);
                playerScripts.Remove(script.Key);
                break;
            }
        }

        var playerNumber = int.Parse(player + "");
        Network.RemoveRPCs(Network.player, playerNumber);

        Network.RemoveRPCs(player);
        Network.DestroyPlayerObjects(player);

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
            Application.LoadLevel("FPS");
        }
        Disconnected();
    }

    [RPC]
    void DestroyBuffered(NetworkViewID viewID) {
        UnityEngine.Object.Destroy(NetworkView.Find(viewID).gameObject);
    }
    #endregion

}