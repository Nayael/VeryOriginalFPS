using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

class FPSController : MonoBehaviour
{
    #region Private Members
    private bool grounded;
    private Vector3 moveDirection = Vector3.zero;
    private float hRotAngle = 0f;

    private bool mouseVisible = false;
    #endregion

    #region Public Members
    public Transform cameraPrefab = null;
    public NetworkPlayer owner;  // The instance's owner on the network

    public float acceptableError = 6f;  // The acceptable difference in Unity units between a predicted value and the server response value
    public float acceptableRotError = 0.3f;  // The acceptable difference in Unity units between a predicted value and the server response value

    public float speed = 40f;
    public float jumpSpeed = 20f;
    public float angularSpeed = 20f;
    public float gravity = 100.0f;
    public bool airControl = true;
    public Vector2 mouseSensitivity = new Vector2(1f, 100f);
    #endregion

    #region Properties
    public Vector3 MoveDirection {
        get { return moveDirection; }
    }

    public bool MouseVisible {
        get { return mouseVisible; }
        set {
            mouseVisible = value;
            Screen.lockCursor = !mouseVisible;
            Screen.showCursor = mouseVisible;
        }
    }
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
        // TAB key is used to unlock the mouse
        if (Input.GetKeyDown(KeyCode.Tab)) {
            MouseVisible = !MouseVisible;
        }
    }

    void FixedUpdate() {
        CharacterController controller = GetComponent<CharacterController>();

        // If we are the owner
        if (Network.player == owner) {
            /* Character translation */
            if (grounded) {
                moveDirection = new Vector3(Input.GetAxis("Horizontal"), 0, Input.GetAxis("Vertical"));
                moveDirection = transform.TransformDirection(moveDirection);
                moveDirection *= speed;
                if (Input.GetAxis("Jump") != 0f) {
                    moveDirection.y = jumpSpeed;
                }
            } else if (airControl) {
                moveDirection.x = Input.GetAxis("Horizontal") * speed;
                moveDirection.z = Input.GetAxis("Vertical") * speed;
                moveDirection = transform.TransformDirection(moveDirection);
            }

            // Apply gravity
            moveDirection.y -= gravity * Time.deltaTime;

            /* Character rotation */
            hRotAngle = angularSpeed * Input.GetAxis("Mouse X") * mouseSensitivity.x * Time.deltaTime;

            networkView.RPC("SendPlayerMovement", RPCMode.Server, moveDirection, hRotAngle);
        }

        // Apply the translation & rotation
        grounded = (controller.Move(moveDirection * Time.deltaTime) & CollisionFlags.Below) != 0;
        transform.RotateAroundLocal(Vector3.up, hRotAngle);
    }
    #endregion

    #region Networking
    void OnSerializeNetworkView(BitStream stream, NetworkMessageInfo info) {

        Vector3 moveDirection = Vector3.zero;
        float hRotAngle = 0f;
        Vector3 pos = transform.position;
        Quaternion rot = transform.localRotation;

        // Writing data (on the server)
        if (stream.isWriting) {
            stream.Serialize(ref this.moveDirection);
            stream.Serialize(ref this.hRotAngle);
            stream.Serialize(ref pos);
            stream.Serialize(ref rot);
            
        // Receiving data (on the clients)
        } else {
            stream.Serialize(ref moveDirection);
            stream.Serialize(ref hRotAngle);
            stream.Serialize(ref pos);
            stream.Serialize(ref rot);

            if (Network.player != owner) {
                this.moveDirection = moveDirection;
                this.hRotAngle = hRotAngle;
                this.transform.position = pos;
                this.transform.localRotation = rot;
            } else {
                if (Vector3.Distance(this.transform.position, pos) > acceptableError) {
                    this.transform.position = Vector3.Lerp(this.transform.position, pos, 0.25f);
                }

                if (Quaternion.Angle(this.transform.localRotation, rot) > acceptableRotError) {
                    this.transform.localRotation = Quaternion.Lerp(this.transform.localRotation, rot, 0.25f);
                }
            }
        }
    }
    #endregion

    #region RPC
    [RPC]
    void SetPlayer(NetworkPlayer player) {
        owner = player;

        // If we are the owner
        if (owner == Network.player) {
            this.enabled = true;    // Enable me
            this.gameObject.tag = "Player";

            if (GetComponent<Shooter>() != null) {
                GetComponent<Shooter>().enabled = true;
                GetComponent<Shooter>().owner = this.owner;
                MouseVisible = false;
            }

            // Use a FPSCamera
            Camera.main.enabled = false;    // Disable the main camera
            Transform fpsCam = (Transform)Instantiate(cameraPrefab);
            fpsCam.parent = this.transform;
            fpsCam.localPosition = new Vector3(0f, 0.025f, 0.26f);
            fpsCam.GetComponent<FPSCamera>().MouseYSensitivity = this.mouseSensitivity.y;
        }
    }

    [RPC]
    void SendPlayerMovement(Vector3 move, float hRotation) {
        if (Network.isServer) {
            moveDirection = move;
            hRotAngle = hRotation;
        }
    }
    #endregion
}