using System;
using System.Collections.Generic;
using UnityEngine;

class FPSControllerAuthoritative : MonoBehaviour
{
    #region Private Members
    private float lastClientHInput = 0f;
    private float lastClientVInput = 0f;
    private float lastClientJumpInput = 0f;
    private float serverCurrentHInput = 0f;
    private float serverCurrentVInput = 0f;
    private float serverCurrentJumpInput = 0f;

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
    }
    #endregion

    #region Update
    void FixedUpdate() {
        CharacterController controller = GetComponent<CharacterController>();

        // Get the inputs only if we are the owner
        if (Network.player == owner) {
            float hInput = Input.GetAxis("Horizontal");
            float vInput = Input.GetAxis("Vertical");
            float jumpInput = Input.GetAxis("Jump");

            // If the input has changed since the last time
            if (lastClientHInput != hInput || lastClientVInput != vInput || lastClientJumpInput != jumpInput) {
                // We keep the new input as the last input
                lastClientHInput = hInput;
                lastClientVInput = vInput;
                lastClientJumpInput = jumpInput;

                if (Network.isServer) {
                    SendMovementInput(hInput, vInput, jumpInput);
                } else if (Network.isClient) {
                    networkView.RPC("SendMovementInput", RPCMode.Server, hInput, vInput, jumpInput);
                }
            }
        }

        // The movements are done on the server
        if (Network.isServer) {
            if (grounded) {
                moveDirection = new Vector3(serverCurrentHInput, 0, serverCurrentVInput);
                moveDirection = transform.TransformDirection(moveDirection);
                moveDirection *= speed;
                if (serverCurrentJumpInput != 0f) {
                    moveDirection.y = jumpSpeed;
                }
            } else if (airControl) {
                moveDirection.x = serverCurrentHInput * speed;
                moveDirection.z = serverCurrentVInput * speed;
                moveDirection = transform.TransformDirection(moveDirection);
            }

            // Apply gravity
            moveDirection.y -= gravity * Time.deltaTime;

            // Move the controller
            grounded = (controller.Move(moveDirection * Time.deltaTime) & CollisionFlags.Below) != 0;

            //// Mouse movement
            //float mHorizontal = Input.GetAxis("Mouse X");
            //float mVertical = Input.GetAxis("Mouse Y");
            //float sideTurn = angularSpeed * mHorizontal * mouseSensitivity.x * Time.deltaTime;
            //float verticalTurn = -1 * mVertical * mouseSensitivity.y * Time.deltaTime;

            //// Vertical rotation
            //Camera.main.transform.Rotate(verticalTurn, 0f, 0f);

            //// Capping the vertical rotation of the camera
            //if (Camera.main.transform.localRotation.x < -maxVerticalAngle) {
            //    Camera.main.transform.localRotation = new Quaternion(-maxVerticalAngle, Camera.main.transform.localRotation.y, Camera.main.transform.localRotation.z, Camera.main.transform.localRotation.w);
            //} else if (Camera.main.transform.localRotation.x > maxVerticalAngle) {
            //    Camera.main.transform.localRotation = new Quaternion(maxVerticalAngle, Camera.main.transform.localRotation.y, Camera.main.transform.localRotation.z, Camera.main.transform.localRotation.w);
            //}

            //// Side Rotation
            //transform.RotateAroundLocal(Vector3.up, sideTurn);
        }
    }
    #endregion

    #region Networking
    void OnSerializeNetworkView(BitStream stream, NetworkMessageInfo info) {
        // Writing data (on the server)
        if (stream.isWriting) {
            Vector3 pos = transform.position;
            Quaternion rot = transform.rotation;
            Quaternion camRot = Camera.main.transform.localRotation;

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
            stream.Serialize(ref rotReceive);

            transform.position = posReceive;
            transform.rotation = rotReceive;
            Camera.main.transform.localRotation = camRotReceive;
        }
    }
    #endregion

    #region RPC
    [RPC]
    void SetPlayer(NetworkPlayer player) {
        owner = player;
        if (player == Network.player) {
            enabled = true;

            Camera.main.transform.parent = transform;
            Camera.main.transform.localPosition = Vector3.zero;
            Camera.main.transform.localRotation = Quaternion.identity;
        }
    }

    [RPC]
    void SendMovementInput(float hInput, float vInput, float jumpInput) {
        serverCurrentHInput = hInput;
        serverCurrentVInput = vInput;
        serverCurrentJumpInput = jumpInput;
    }
    #endregion
}