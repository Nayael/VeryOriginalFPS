using System;
using System.Collections.Generic;
using UnityEngine;

class FPSControllerAuthoritative : MonoBehaviour
{
    #region Private Members
    private Dictionary<string, float> lastClientInput = new Dictionary<string, float>();
    private Dictionary<string, float> serverCurrentInput = new Dictionary<string, float>();

    private bool grounded;
    private Vector3 moveDirection = Vector3.zero;
    #endregion

    #region Public Members
    public NetworkPlayer owner;  // The instance's owner on the network

    public float speed = 6.0f;
    public float jumpSpeed = 8.0f;
    public float angularSpeed = 8.0f;
    public float maxVerticalAngle = 0.5f;
    public float gravity = 100.0f;
    public bool airControl = true;
    public Vector2 mouseSensitivity = new Vector2(1f, 100f);
    #endregion

    #region Initialization
    void Awake() {
        // If we are not the server, then immediately disable this script
        if (Network.isClient) {
            enabled = false;
        }

        lastClientInput.Add("Horizontal", 0f);
        lastClientInput.Add("Vertical", 0f);
        lastClientInput.Add("Jump", 0f);
        lastClientInput.Add("Mouse X", 0f);
        lastClientInput.Add("Mouse Y", 0f);

        serverCurrentInput.Add("Horizontal", 0f);
        serverCurrentInput.Add("Vertical", 0f);
        serverCurrentInput.Add("Jump", 0f);
        serverCurrentInput.Add("Mouse X", 0f);
        serverCurrentInput.Add("Mouse Y", 0f);
    }
    #endregion

    #region Update
    void FixedUpdate() {
        CharacterController controller = GetComponent<CharacterController>();

        // Get the inputs only if we are the owner
        if (Network.player == owner) {
            Dictionary<string, float> currentClientInput = new Dictionary<string, float>();

            currentClientInput.Add("Horizontal", Input.GetAxis("Horizontal"));
            currentClientInput.Add("Vertical", Input.GetAxis("Vertical"));
            currentClientInput.Add("Jump", Input.GetAxis("Jump"));
            currentClientInput.Add("Mouse X", Input.GetAxis("Mouse X"));
            currentClientInput.Add("Mouse Y", Input.GetAxis("Mouse Y"));

            // If the input has changed since the last time
            if (lastClientInput["Horizontal"] != currentClientInput["Horizontal"] || lastClientInput["Vertical"] != currentClientInput["Vertical"] || lastClientInput["Jump"] != currentClientInput["Jump"]) {
                // We keep the new input as the last input
                lastClientInput["Horizontal"] = currentClientInput["Horizontal"];
                lastClientInput["Vertical"] = currentClientInput["Vertical"];
                lastClientInput["Jump"] = currentClientInput["Jump"];

                if (Network.isServer) {
                    SendMovementInput(currentClientInput["Horizontal"], currentClientInput["Vertical"], currentClientInput["Jump"]);
                } else if (Network.isClient) {
                    networkView.RPC("SendMovementInput", RPCMode.Server, currentClientInput["Horizontal"], currentClientInput["Vertical"], currentClientInput["Jump"]);
                }
            }

            // If the mouse has moved
            if (currentClientInput["Mouse X"] != lastClientInput["Mouse X"] || currentClientInput["Mouse Y"] != lastClientInput["Mouse Y"]) {
                // We keep the new input as the last input
                lastClientInput["Mouse X"] = currentClientInput["Mouse X"];
                lastClientInput["Mouse Y"] = currentClientInput["Mouse Y"];

                if (Network.isServer) {
                    SendMouseInput(currentClientInput["Mouse X"], currentClientInput["Mouse Y"]);
                } else if (Network.isClient) {
                    networkView.RPC("SendMouseInput", RPCMode.Server, currentClientInput["Mouse X"], currentClientInput["Mouse Y"]);
                }
            }
        }

        // The movements are done on the server
        if (Network.isServer) {
            if (grounded) {
                moveDirection = new Vector3(serverCurrentInput["Horizontal"], 0, serverCurrentInput["Vertical"]);
                moveDirection = transform.TransformDirection(moveDirection);
                moveDirection *= speed;
                if (serverCurrentInput["Jump"] != 0f) {
                    moveDirection.y = jumpSpeed;
                }
            } else if (airControl) {
                moveDirection.x = serverCurrentInput["Horizontal"] * speed;
                moveDirection.z = serverCurrentInput["Vertical"] * speed;
                moveDirection = transform.TransformDirection(moveDirection);
            }

            // Apply gravity
            moveDirection.y -= gravity * Time.deltaTime;

            // Move the controller
            grounded = (controller.Move(moveDirection * Time.deltaTime) & CollisionFlags.Below) != 0;

            // Mouse movement
            float sideTurn = angularSpeed * serverCurrentInput["Mouse X"] * mouseSensitivity.x * Time.deltaTime;
            float verticalTurn = -1 * serverCurrentInput["Mouse Y"] * mouseSensitivity.y * Time.deltaTime;

            // Vertical rotation
            Camera fpsCamera = transform.camera;
            fpsCamera.transform.Rotate(verticalTurn, 0f, 0f);

            // Capping the vertical rotation of the camera
            if (fpsCamera.transform.localRotation.x < -maxVerticalAngle) {
                var correctRot = fpsCamera.camera.transform.localRotation;
                correctRot.x = -maxVerticalAngle;
                fpsCamera.camera.transform.localRotation = correctRot;
            } else if (fpsCamera.transform.localRotation.x > maxVerticalAngle) {
                var correctRot = fpsCamera.camera.transform.localRotation;
                correctRot.x = maxVerticalAngle;
                fpsCamera.camera.transform.localRotation = correctRot;
            }

            // Side Rotation
            transform.RotateAroundLocal(Vector3.up, sideTurn);
        }
    }
    #endregion

    #region Networking
    void OnSerializeNetworkView(BitStream stream, NetworkMessageInfo info) {
        Camera fpsCamera = transform.camera;

        // Writing data (on the server)
        if (stream.isWriting) {
            Debug.Log(transform.GetComponentInChildren<Camera>());
            Vector3 pos = transform.position;
            Quaternion rot = transform.rotation;
            Quaternion camRot = fpsCamera.transform.localRotation;

            stream.Serialize(ref pos);
            stream.Serialize(ref rot);
            stream.Serialize(ref camRot);

        // Receiving data (on the clients)
        } else {
            Vector3 posReceive = Vector3.zero;
            Quaternion rotReceive = Quaternion.identity;
            Quaternion camRotReceive = Quaternion.identity;

            stream.Serialize(ref posReceive);
            stream.Serialize(ref rotReceive);
            stream.Serialize(ref camRotReceive);

            transform.position = posReceive;
            transform.rotation = rotReceive;
            fpsCamera.transform.localRotation = camRotReceive;
        }
    }
    #endregion

    #region RPC
    [RPC]
    void SetPlayer(NetworkPlayer player) {
        owner = player;
        if (player == Network.player) {
            enabled = true;

            if (Network.isClient) {
                transform.camera.enabled = true;
                transform.camera.transform.localRotation = Quaternion.identity;
                transform.camera.transform.localPosition = Vector3.zero;
                Camera.main.enabled = false;
            }
        }
    }

    [RPC]
    void SendMovementInput(float clientInputHorizontal, float clientInputVertical, float clientInputJump) {
        serverCurrentInput["Horizontal"] = clientInputHorizontal;
        serverCurrentInput["Vertical"] = clientInputVertical;
        serverCurrentInput["Jump"] = clientInputJump;
    }

    [RPC]
    void SendMouseInput(float clientInputMouseX, float clientInputMouseY) {
        serverCurrentInput["Mouse X"] = clientInputMouseX;
        serverCurrentInput["Mouse Y"] = clientInputMouseY;
    }
    #endregion
}