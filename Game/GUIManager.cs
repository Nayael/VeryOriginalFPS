﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

public class GUIManager : MonoBehaviour
{

    #region Private Members
    private Networker networker;
    #endregion

    #region Public Members
    public delegate void GuiDelegate();
    public GuiDelegate guiRenderer;

    public Dictionary<string, Texture> textures = new Dictionary<string, Texture>();
    public GUISkin skin;

    public List<string> texturesKeys;
    public List<Texture> texturesInstances;
    #endregion

    #region Initialization
    void Awake() {
        // Creating the Dictionary
        for (int i = 0; i < texturesInstances.Count; i++) {
            textures[texturesKeys[i]] = texturesInstances[i];
        }
    }

    void Start() {
        networker = GameObject.FindGameObjectWithTag("Networker").GetComponent<Networker>();
        guiRenderer = MenuGUI;

        // Adding listeners to the networker's events
        Networker.ClientConnected += Networker_ClientConnected;
        Networker.ServerInitialized += Networker_ServerInitialized;
        Networker.Disconnected += Networker_Disconnected;
    }
    #endregion

    #region GUI
    void OnGUI() {
        GUI.skin = skin;
        guiRenderer();
    }

    void MenuGUI() {
        GUILayout.BeginVertical(skin.GetStyle("mainMenu"), GUILayout.Height(Screen.height), GUILayout.Width(Screen.width));
        GUILayout.FlexibleSpace();

        // Main Title
        GUILayout.BeginArea(new Rect(0f, 0f, Screen.width, 100f));
        GUILayout.BeginHorizontal();
        GUILayout.FlexibleSpace();  // Center Horizontally

        GUILayout.BeginVertical();
        GUILayout.FlexibleSpace();  // Center Vertically

        GUILayout.Label("Very Original FPS", skin.GetStyle("mainTitle"));

        //GUILayout.FlexibleSpace();
        GUILayout.EndVertical();

        GUILayout.FlexibleSpace();
        GUILayout.EndHorizontal();
        GUILayout.EndArea();

        // Begin Server/Client GUI
        GUILayout.BeginArea(new Rect(30f, 100f, Screen.width - 60f, Screen.height - 100f));
        GUILayout.BeginHorizontal(skin.GetStyle("padding"), GUILayout.ExpandWidth(true));    // This customStyle contains the padding values

        // Server GUI
        GUILayout.BeginVertical(skin.GetStyle("menuColumn"));

        // Server Title
        GUILayout.Label("Host Game", skin.GetStyle("title"));

        GUILayout.BeginVertical(GUILayout.ExpandWidth(false));
        GUILayout.FlexibleSpace();  // Used to center the fields vertically

        // Server Port Field
        GUILayout.BeginHorizontal();
        GUILayout.Label("Server Port", skin.GetStyle("label"));
        networker.portTF = GUILayout.TextField(networker.portTF, 15);
        GUILayout.EndHorizontal();

        // Launch Button
        GUILayout.FlexibleSpace();
        GUILayout.BeginHorizontal();
        GUILayout.FlexibleSpace();
        if (GUILayout.Button("Launch server")) {
            networker.LaunchServer();
        }
        GUILayout.EndHorizontal();

        GUILayout.EndVertical();
        GUILayout.EndVertical();
        // End Server GUI

        // Client GUI
        GUILayout.BeginVertical(skin.GetStyle("menuColumn"));

        // Client Title
        GUILayout.Label("Join Game", skin.GetStyle("title"));

        GUILayout.BeginVertical(GUILayout.ExpandWidth(false));
        GUILayout.FlexibleSpace();  // Used to center the fields vertically

        // Server IP Field
        GUILayout.BeginHorizontal();
        GUILayout.Label("Server IP", skin.GetStyle("label"));
        networker.serverIP = GUILayout.TextField(networker.serverIP, 15);
        GUILayout.EndHorizontal();

        // Server Port Field
        GUILayout.BeginHorizontal();
        GUILayout.Label("Server Port", skin.GetStyle("label"));
        networker.portTF = GUILayout.TextField(networker.portTF, 15);
        GUILayout.EndHorizontal();

        // Join Button
        GUILayout.FlexibleSpace();
        GUILayout.BeginHorizontal();
        GUILayout.FlexibleSpace();
        if (GUILayout.Button("Join")) {
            System.Int32.TryParse(networker.portTF, out networker.serverPort);
            networker.ConnectToServer();
        }

        GUILayout.EndVertical();
        GUILayout.EndVertical();
        // End Client GUI

        GUILayout.EndHorizontal();
        GUILayout.EndArea();
        // End Server/Client GUI

        GUILayout.EndVertical();
    }

    void GameGUI() {
        if (!Network.isClient) {
            return;
        }
        Texture crosshairTexture = textures["Crosshair"];
        Texture crosshairFireTexture = textures["CrosshairFire"];

        if (GUILayout.Button("Disconnect")) {
            networker.DisconnectFromServer();
        }

        if (GameObject.FindGameObjectWithTag("Player") != null) {
            GUI.contentColor = Color.black;

            GameObject playerGO = GameObject.FindGameObjectWithTag("Player");
            Shooter shooterScript = playerGO.GetComponent<Shooter>();

            Rect position = new Rect((Screen.width - crosshairTexture.width) / 2, (Screen.height - crosshairTexture.height) / 2, crosshairTexture.width, crosshairTexture.height);
            GUI.DrawTexture(position, (Input.GetButton("Fire1") && shooterScript.Weapon.Ammo > 0) ? crosshairFireTexture : crosshairTexture);

            GUI.Label(new Rect(0, 20, 80, 30), " Frags : " + shooterScript.frags.ToString());
            GUI.Label(new Rect(0, 40, 130, 30), " Deaths : " + shooterScript.deaths.ToString());
            GUI.Label(new Rect(0, Screen.height - 20, 80, 20), " + " + playerGO.GetComponent<Unit>().health.Current.ToString());

            if (playerGO.GetComponent<Shooter>().Weapon != null) {
                GUI.Label(new Rect(Screen.width - 80, Screen.height - 20, 80, 20), " Ammo : " + playerGO.GetComponent<Shooter>().Weapon.Ammo);
            }
        }
    }

    void ServerInitializedGUI() {
        if (!Network.isServer) {
            return;
        }

        GUI.Box(new Rect(Screen.width / 2 - 60, Screen.height / 2 - 15, 120, 30), "Server is running");
        if (GUILayout.Button("Shutdown")) {
            networker.ShutdownServer();
        }
    }
    #endregion

    #region Event Listeners
    void Networker_ClientConnected() {
        guiRenderer = GameGUI;
    }

    void Networker_ServerInitialized() {
        guiRenderer = ServerInitializedGUI;
    }

    void Networker_Disconnected() {
        guiRenderer = MenuGUI;
    }
    #endregion
}
