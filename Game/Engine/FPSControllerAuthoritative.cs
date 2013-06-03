using System;
using System.Collections.Generic;
using UnityEngine;

class FPSControllerAuthoritative : MonoBehaviour
{
    #region Private Members
    private float lastClientHInput = 0f;
    private float lastClientVInput = 0f;
    private float serverCurrentHInput = 0f;
    private float serverCurrentVInput = 0f;
    private bool grounded;

    //private Vector3 moveDirection = Vector3.zero;
    //private CharacterController controller;
    #endregion

    #region Public Members
    public NetworkPlayer owner;  // The instance's owner on the network

    public float speed = 6.0f;
    public float jumpSpeed = 8.0f;
    public float angularSpeed = 8.0f;
    public float maxVerticalAngle = 0.5f;
    public float gravity = 100.0f;
    public bool airControl = true;
    //public Vector2 mouseSensitivity;
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
    void Update() {
        // Get the inputs only if we are the owner
        if (owner != null && Network.player == owner) {
            float HInput = Input.GetAxis("Horizontal");
            float VInput = Input.GetAxis("Vertical");

            // If the input has changed since the last time
            if (lastClientHInput != HInput || lastClientVInput != VInput) {
                // We keep the new input as the last input
                lastClientHInput = HInput;
                lastClientVInput = VInput;

                if (Network.isServer) {
                    SendMovementInput(HInput, VInput);
                } else if (Network.isClient) {
                    networkView.RPC("SendMovementInput", RPCMode.Server, HInput, VInput);
                }
            }
        }

        if (Network.isServer) {
            Vector3 moveDirection = new Vector3(serverCurrentHInput, 0, serverCurrentVInput);
            float speed = 5;
            transform.Translate(speed * moveDirection * Time.deltaTime);
            Debug.Log(transform.position);

            //Vector3 moveDirection = new Vector3(serverCurrentHInput, 0, serverCurrentVInput);
            //float speed = 5;
            //transform.Translate(speed * moveDirection * Time.deltaTime);
            //if (grounded) {
            //    moveDirection = new Vector3(serverCurrentHInput, -0.75f, serverCurrentVInput);
            //    moveDirection = transform.TransformDirection(moveDirection);
            //    moveDirection *= speed;
            //    if (Input.GetButton("Jump")) {
            //        moveDirection.y = jumpSpeed;
            //    }
            //}

            //// Apply gravity
            //moveDirection.y -= gravity * Time.deltaTime;

            //// Move the controller
            //grounded = (controller.Move(moveDirection * Time.deltaTime) & CollisionFlags.Below) != 0;    // Translation
        }
    }
    #endregion

    #region Networking
    void OnSerializeNetworkView(BitStream stream, NetworkMessageInfo info) {
        if (stream.isWriting) {
            Vector3 pos = transform.position;
            stream.Serialize(ref pos);
        } else {
            Vector3 posReceive = Vector3.zero;
            stream.Serialize(ref posReceive);
            transform.position = posReceive;
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
    void SendMovementInput(float HInput, float VInput) {
        serverCurrentHInput = HInput;
        serverCurrentVInput = VInput;
    }
    #endregion
}