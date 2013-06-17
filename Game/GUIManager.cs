using UnityEngine;
using System.Collections;

public class GUIManager : MonoBehaviour
{

    #region Private Members
    private Networker networker;
    #endregion

    #region Public Members
    public delegate void GuiDelegate();
    public GuiDelegate guiRenderer;

    public Texture crosshairTexture;
    public Texture crosshairFireTexture;
    #endregion

    #region Initialization
    void Start () {
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
        guiRenderer();
    }

    void MenuGUI() {
        // Client GUI
        GUI.Label(new Rect(15, 15, 80, 20), "Server IP");
        networker.serverIP = GUI.TextField(new Rect(95, 15, 120, 20), networker.serverIP, 15);

        GUI.Label(new Rect(15, 45, 80, 20), "Server Port");
        networker.portTF = GUI.TextField(new Rect(95, 45, 120, 20), networker.portTF, 15);

        if (GUI.Button(new Rect(95, 70, 120, 30), "Connect")) {
            System.Int32.TryParse(networker.portTF, out networker.serverPort);
            networker.ConnectToServer();
        }

        // Server GUI
        GUI.Label(new Rect(15, 45, 80, 20), "Server Port");
        networker.portTF = GUI.TextField(new Rect(95, 45, 120, 20), networker.portTF, 15);
        if (GUI.Button(new Rect(Screen.width / 2 - 60, Screen.height / 2 - 15, 120, 30), "Start server")) {
            networker.LaunchServer();
        }
    }

    void ClientConnectedGUI() {
        if (!Network.isClient) {
            return;
        }

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
        guiRenderer = ClientConnectedGUI;
    }

    void Networker_ServerInitialized() {
        guiRenderer = ServerInitializedGUI;
    }

    void Networker_Disconnected() {
        guiRenderer = MenuGUI;
    }
    #endregion
}
