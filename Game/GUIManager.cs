using UnityEngine;
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
        skin.GetStyle("menuColumn").padding.left = (int)((Screen.width - 60f) / 8f);
        GUILayout.BeginVertical(skin.GetStyle("menuColumn"));

        // Server Title
        GUILayout.Label("Host Game", skin.GetStyle("title"));

        GUILayout.BeginVertical(GUILayout.ExpandWidth(false));
        GUILayout.FlexibleSpace();  // Used to center the fields vertically

        // Server Port Field
        GUILayout.BeginHorizontal();
        GUILayout.Label("Listen Port", skin.GetStyle("label"));
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

        if (GUILayout.Button("Disconnect")) {
            networker.DisconnectFromServer();
        }

        if (GameObject.FindGameObjectWithTag("Player") != null) {
            GameObject playerGO = GameObject.FindGameObjectWithTag("Player");
            Shooter shooterScript = playerGO.GetComponent<Shooter>();

            Rect position = new Rect((Screen.width - crosshairTexture.width) / 2, (Screen.height - crosshairTexture.height) / 2, crosshairTexture.width, crosshairTexture.height);
            GUI.DrawTexture(position, crosshairTexture);

            GUILayout.BeginArea(new Rect(Screen.width - 120f, 30f, 100f, 80f));
            GUILayout.BeginVertical();

            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            GUILayout.Box(textures["Frags"], GUILayout.Width(24f), GUILayout.Height(24f));
            GUILayout.FlexibleSpace();
            GUILayout.Label(shooterScript.frags.ToString(), skin.GetStyle("HUDText"));
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();

            GUILayout.FlexibleSpace();

            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            GUILayout.Box(textures["Deaths"], GUILayout.Width(24f), GUILayout.Height(32f));
            GUILayout.FlexibleSpace();
            GUILayout.Label(shooterScript.deaths.ToString(), skin.GetStyle("HUDText"));
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();

            GUILayout.EndVertical();
            GUILayout.EndArea();

            GUILayout.BeginArea(new Rect(20f, Screen.height - 50f, 100f, 44f));
            GUILayout.BeginHorizontal();
            
            GUILayout.Box(textures["Health"]);

            GUILayout.BeginVertical();
            GUILayout.Space(5f);
            GUILayout.Label(playerGO.GetComponent<Unit>().health.Current.ToString(), skin.GetStyle("HUDText"));
            GUILayout.EndVertical();
            
            GUILayout.EndHorizontal();
            GUILayout.EndArea();

            if (playerGO.GetComponent<Shooter>().Weapon != null) {
                string weaponName = playerGO.GetComponent<Shooter>().Weapon.GetType().ToString();
                Texture ammoTexture = textures[weaponName];

                GUILayout.BeginArea(new Rect(Screen.width - 120f, Screen.height - 60f, 110f, 60f));
                GUILayout.BeginHorizontal();
                GUILayout.Box(ammoTexture, GUILayout.Width(44f), GUILayout.Height(44f));
                GUILayout.FlexibleSpace();
                GUILayout.BeginVertical();
                GUILayout.Space(11f);
                GUILayout.Label(playerGO.GetComponent<Shooter>().Weapon.Ammo.ToString(), skin.GetStyle("HUDText"));
                GUILayout.EndVertical();
                GUILayout.FlexibleSpace();
                GUILayout.EndHorizontal();
                GUILayout.EndArea();
            }
        }
    }

    void ServerGUI() {
        if (!Network.isServer) {
            return;
        }

        if (GUILayout.Button("Shutdown")) {
            networker.ShutdownServer();
        }

        GUILayout.BeginArea(new Rect(0f, 0f, Screen.width, Screen.height));
        GUILayout.BeginVertical();
        GUILayout.FlexibleSpace();
        GUILayout.Box("Server is running", skin.GetStyle("title"));
        GUILayout.FlexibleSpace();
        GUILayout.EndVertical();
        GUILayout.EndArea();
    }
    #endregion

    #region Event Listeners
    void Networker_ClientConnected() {
        guiRenderer = GameGUI;
    }

    void Networker_ServerInitialized() {
        guiRenderer = ServerGUI;
    }

    void Networker_Disconnected() {
        guiRenderer = MenuGUI;
    }
    #endregion
}
