using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

class FPSControllerAdvanced : MonoBehaviour
{
    #region Private Members
    private Dictionary<string, float> lastClientInput = new Dictionary<string, float>();
    private Dictionary<string, Dictionary<string, float>> serverInput = new Dictionary<string, Dictionary<string, float>>();
    private Dictionary<string, ArrayList> predictedMoves = new Dictionary<string, ArrayList>();
    private int predictionId = 0;

    // The ID of the currently treated inputs on the server
    private int serverCurrentInputID = 0;
    private int serverLastTreatedInputID = -1;

    private bool grounded;
    private Vector3 moveDirection = Vector3.zero;
    private Camera fpsCam;
    #endregion

    #region Public Members
    public Transform cameraPrefab = null;
    public NetworkPlayer owner;  // The instance's owner on the network

    public float acceptableError = 1f;  // The acceptable difference in Unity units between a predicted value and the server response value
    public float acceptableRotError = 0.1f;  // The acceptable difference in Unity units between a predicted value and the server response value

    public float speed = 40f;
    public float jumpSpeed = 20f;
    public float angularSpeed = 20f;
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
    }
    #endregion

    #region Update
    void FixedUpdate() {
        CharacterController controller = GetComponent<CharacterController>();
        Vector3 moveInput = Vector3.zero;
        Vector3 mouseInput = Vector3.zero;

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

                moveInput = new Vector3(
                    currentClientInput["Horizontal"],
                    currentClientInput["Vertical"],
                    currentClientInput["Jump"]
                );
            }

            // If the mouse has moved
            if (currentClientInput["Mouse X"] != lastClientInput["Mouse X"] || currentClientInput["Mouse Y"] != lastClientInput["Mouse Y"]) {
                // We keep the new input as the last input
                lastClientInput["Mouse X"] = currentClientInput["Mouse X"];
                lastClientInput["Mouse Y"] = currentClientInput["Mouse Y"];

                mouseInput = new Vector3(
                    currentClientInput["Mouse X"],
                    currentClientInput["Mouse Y"],
                    0f
                );
            }
        }

        // The movements are done on the server, predicted on the client
        if (Network.isServer || Network.player == owner) {
            Dictionary<string, float> userInput = null;
            float sideTurn = 0f;
            float verticalTurn = 0f;

            if (Network.isServer) {
                if (serverInput.Count > 0) {
                    serverLastTreatedInputID++;
                    userInput = serverInput[serverLastTreatedInputID.ToString()];
                }
                Debug.Log("Process inputs " + serverLastTreatedInputID);
                Debug.Log(serverInput.Count);
            } else {
                userInput = lastClientInput;
            }

            if (userInput != null) {
                // ROTATION
                // Mouse movement
                sideTurn = angularSpeed * userInput["Mouse X"] * mouseSensitivity.x * Time.deltaTime;
                verticalTurn = -1 * userInput["Mouse Y"] * mouseSensitivity.y * Time.deltaTime;

                // Vertical rotation
                fpsCam.transform.Rotate(verticalTurn, 0f, 0f);

                // Capping the vertical rotation of the camera
                if (fpsCam.transform.localRotation.x < -maxVerticalAngle) {
                    Quaternion correctRot = fpsCam.camera.transform.localRotation;
                    correctRot.x = -maxVerticalAngle;
                    fpsCam.camera.transform.localRotation = correctRot;
                } else if (fpsCam.transform.localRotation.x > maxVerticalAngle) {
                    Quaternion correctRot = fpsCam.camera.transform.localRotation;
                    correctRot.x = maxVerticalAngle;
                    fpsCam.camera.transform.localRotation = correctRot;
                }

                // Side Rotation
                transform.RotateAroundLocal(Vector3.up, sideTurn);

                // TRANSLATION
                if (grounded) {
                    moveDirection = new Vector3(userInput["Horizontal"], 0, userInput["Vertical"]);
                    moveDirection = transform.TransformDirection(moveDirection);
                    moveDirection *= speed;
                    if (userInput["Jump"] != 0f) {
                        moveDirection.y = jumpSpeed;
                    }
                } else if (airControl) {
                    moveDirection.x = userInput["Horizontal"] * speed;
                    moveDirection.z = userInput["Vertical"] * speed;
                    moveDirection = transform.TransformDirection(moveDirection);
                }
            }

            // Apply gravity
            moveDirection.y -= gravity * Time.deltaTime;

            // Move the controller
            grounded = (controller.Move(moveDirection * Time.deltaTime) & CollisionFlags.Below) != 0;

            // On the client side, keep track of the predicted moves
            if (Network.player == owner && (moveDirection != Vector3.zero || sideTurn != 0f || verticalTurn != 0f)) {
                //Debug.Log("client " + transform.position);
                ArrayList prediction = new ArrayList();
                prediction.Add(transform.position);
                prediction.Add(transform.rotation);
                prediction.Add(fpsCam.transform.localRotation);
                predictedMoves.Add(predictionId.ToString(), prediction);

                networkView.RPC("SendMovementInput", RPCMode.Server, predictionId, moveInput);
                networkView.RPC("SendMouseInput", RPCMode.Server, predictionId, mouseInput);

                predictionId++;
            }
            //else
            //    Debug.Log("server " + transform.position);
        }
    }
    #endregion

    #region Networking
    void OnSerializeNetworkView(BitStream stream, NetworkMessageInfo info) {

        // Writing data (on the server)
        if (stream.isWriting) {

            if (serverLastTreatedInputID == -1) {
                return;
            }
            Vector3 pos = transform.position;
            Quaternion rot = transform.rotation;
            Quaternion camRot = fpsCam.transform.localRotation;

            stream.Serialize(ref serverLastTreatedInputID);
            stream.Serialize(ref pos);
            stream.Serialize(ref rot);
            stream.Serialize(ref camRot);
            Debug.Log("server send " + serverLastTreatedInputID + " | " + pos);

            // Receiving data (on the clients)
        } else {
            Vector3 posReceive = Vector3.zero;
            Quaternion rotReceive = Quaternion.identity;
            Quaternion camRotReceive = Quaternion.identity;

            stream.Serialize(ref serverCurrentInputID);
            stream.Serialize(ref posReceive);
            stream.Serialize(ref rotReceive);
            stream.Serialize(ref camRotReceive);

            if (Network.player == owner && predictedMoves.ContainsKey(serverCurrentInputID.ToString())) {

                // First, retrieve the predicted moves corresponding to the received input from the server
                ArrayList predictedMove = predictedMoves[serverCurrentInputID.ToString()];
                predictedMoves.Remove(serverCurrentInputID.ToString());

                Vector3 predictedPos = (Vector3)predictedMove[0];
                Quaternion predictedRot = (Quaternion)predictedMove[1];
                Quaternion predictedCamRot = (Quaternion)predictedMove[2];

                Debug.Log("recieve " + serverCurrentInputID);
                Debug.Log("predicted : " + predictedPos + " | " + predictedRot + " | " + predictedCamRot);
                Debug.Log("recieved : " + posReceive + " | " + rotReceive + " | " + camRotReceive);

                // Then calculate the difference between the predicted values and the recieved values
                Vector3 diffPos = posReceive - predictedPos;
                Quaternion diffRot = new Quaternion(
                    rotReceive.x - predictedRot.x,
                    rotReceive.y - predictedRot.y,
                    rotReceive.z - predictedRot.z,
                    rotReceive.w - predictedRot.w
                );
                Quaternion diffCamRot = new Quaternion(
                    camRotReceive.x - predictedCamRot.x,
                    camRotReceive.y - predictedCamRot.y,
                    camRotReceive.z - predictedCamRot.z,
                    camRotReceive.w - predictedCamRot.w
                );

                // If the difference is too big, correct the values on the client side
                if (Mathf.Abs(diffPos.x) > acceptableError
                    || Mathf.Abs(diffPos.y) > acceptableError
                    || Mathf.Abs(diffPos.z) > acceptableError) {
                    //Debug.Log("Correct position");
                    transform.position = posReceive;
                }
                if (Mathf.Abs(diffRot.x) > acceptableRotError
                    || Mathf.Abs(diffRot.y) > acceptableRotError
                    || Mathf.Abs(diffRot.z) > acceptableRotError
                    || Mathf.Abs(diffRot.w) > acceptableRotError) {
                    //Debug.Log("Correct rotation");
                    transform.rotation = rotReceive;
                }
                if (Mathf.Abs(diffCamRot.x) > acceptableRotError
                    || Mathf.Abs(diffCamRot.y) > acceptableRotError
                    || Mathf.Abs(diffCamRot.z) > acceptableRotError
                    || Mathf.Abs(diffCamRot.w) > acceptableRotError) {
                    //Debug.Log("Correct FPS camera rotation");
                    fpsCam.transform.localRotation = camRotReceive;
                }
            }
        }
    }
    #endregion

    #region RPC
    [RPC]
    void SetPlayer(NetworkPlayer player) {
        owner = player;
        if (player == Network.player) {
            enabled = true;
            Camera.main.enabled = false;
            SetCamera();
        }
    }

    [RPC]
    public void SetCamera() {
        Vector3 camPos = Vector3.zero;
        Quaternion camRot = Quaternion.identity;

        if (Network.isClient) {
            fpsCam = GameObject.Find("FPSCamera").camera;
            fpsCam.transform.parent = this.transform;
            fpsCam.enabled = true;
            fpsCam.transform.localPosition = camPos;
            fpsCam.transform.localRotation = camRot;
        } else {
            fpsCam = ((Transform)Instantiate(cameraPrefab, camPos, camRot)).camera;
            fpsCam.transform.parent = this.transform;
            fpsCam.enabled = false;
        }
    }

    [RPC]
    void SendMovementInput(int inputID, Vector3 clientInput) {
        Debug.Log("recieve inputID " + inputID);
        Dictionary<string, float> serverCurrentInput = new Dictionary<string, float>();
        serverCurrentInput.Add("Horizontal", clientInput.x);
        serverCurrentInput.Add("Vertical", clientInput.y);
        serverCurrentInput.Add("Jump", clientInput.z);
        serverInput.Add(inputID.ToString(), serverCurrentInput);
    }

    [RPC]
    void SendMouseInput(int inputID, Vector3 clientInput) {
        if (serverInput.ContainsKey(inputID.ToString())) {
            serverInput[inputID.ToString()].Add("Mouse X", clientInput.x);
            serverInput[inputID.ToString()].Add("Mouse Y", clientInput.y);
        } else {
            Dictionary<string, float> serverCurrentInput = new Dictionary<string, float>();
            serverCurrentInput.Add("Mouse X", clientInput.x);
            serverCurrentInput.Add("Mouse Y", clientInput.y);
            serverInput.Add(inputID.ToString(), serverCurrentInput);
        }
    }
    #endregion
}