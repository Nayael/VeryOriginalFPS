using UnityEngine;
using System.Collections;

[RequireComponent(typeof(CharacterController))]
public class FPSController : MonoBehaviour
{

    #region Private Members
    private Vector3 moveDirection = Vector3.zero;
    private CharacterController controller;
    #endregion

    #region Public Members
    public float speed = 6.0f;
    public float jumpSpeed = 8.0f;
    public float angularSpeed = 8.0f;
    public float maxVerticalAngle = 0.5f;
    public float gravity = 100.0f;
    public bool airControl = true;
    public Vector2 mouseSensitivity;
    private bool grounded;
    #endregion

    #region Properties
    public Vector3 MoveDirection {
        get { return moveDirection; }
    }
    #endregion

    #region Initialization
    void Start() {
        controller = GetComponent<CharacterController>();
    }
    #endregion

    #region Update
    void FixedUpdate () {
        /*if (grounded) {
            moveDirection = new Vector3(Input.GetAxis("Horizontal"), -0.75f, Input.GetAxis("Vertical"));
            moveDirection = transform.TransformDirection(moveDirection);
            moveDirection *= speed;
            if (Input.GetButton("Jump")) {
                moveDirection.y = jumpSpeed;
            }
        } else if (airControl) {
            moveDirection.x = Input.GetAxis("Horizontal") * speed;
            moveDirection.z = Input.GetAxis("Vertical") * speed;
            moveDirection = transform.TransformDirection(moveDirection);
        }
        
        // Apply gravity
        moveDirection.y -= gravity * Time.deltaTime;

        // Move the controller
        grounded = (controller.Move(moveDirection * Time.deltaTime) & CollisionFlags.Below) != 0;    // Translation

        // Mouse movement
        float mHorizontal = Input.GetAxis("Mouse X");
        float mVertical = Input.GetAxis("Mouse Y");
        float sideTurn = angularSpeed * mHorizontal * mouseSensitivity.x * Time.deltaTime;
        float verticalTurn = -1 * mVertical * mouseSensitivity.y * Time.deltaTime;

        // Vertical rotation
        Camera.main.transform.Rotate(verticalTurn, 0f, 0f);
        
        // Capping the vertical rotation of the camera
        if (Camera.main.transform.localRotation.x < -maxVerticalAngle) {
            Camera.main.transform.localRotation = new Quaternion(-maxVerticalAngle, Camera.main.transform.localRotation.y, Camera.main.transform.localRotation.z, Camera.main.transform.localRotation.w);
        } else if (Camera.main.transform.localRotation.x > maxVerticalAngle) {
            Camera.main.transform.localRotation = new Quaternion(maxVerticalAngle, Camera.main.transform.localRotation.y, Camera.main.transform.localRotation.z, Camera.main.transform.localRotation.w);
        }

        // Side Rotation
        transform.RotateAroundLocal(Vector3.up, sideTurn);*/
	}
    #endregion

}
