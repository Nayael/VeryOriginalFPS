using UnityEngine;
using System.Collections;

public class Unit : MonoBehaviour
{

    #region Private Members
    
    #endregion

    #region Public Members
    public float speed;
    public float jumpSpeed;
	public float angularSpeed;
    public Vector2 mouseSensitivity;
    #endregion

    #region Initialization
    void Awake() {
        networkView.observed = this;
    }

    // Use this for initialization
	void OnNetworkInstantiate(NetworkMessageInfo info) {
        if (!networkView.isMine) {
            enabled = false;
            return;
        }
        FPSController fpsController = gameObject.AddComponent<FPSController>();
        fpsController.speed = speed;
        fpsController.jumpSpeed = jumpSpeed;
        fpsController.angularSpeed = angularSpeed;
        fpsController.mouseSensitivity = mouseSensitivity;

        Camera.main.transform.parent = this.transform;
        Camera.main.transform.localPosition = Vector3.zero;
        Camera.main.transform.localRotation = Quaternion.identity;
	}
    #endregion

    #region Networking
    void OnSerializeNetworkView(BitStream stream, NetworkMessageInfo info) {
        //Vector3 moveDirection = Vector3.zero;

        //// Sending
        //if (stream.isWriting) {
        //    moveDirection = GetComponent<FPSController>().MoveDirection * Time.deltaTime;
        //    stream.Serialize(ref moveDirection);
        //    Debug.Log("Sender " + moveDirection);

        //// Receiveing
        //} else {
        //    stream.Serialize(ref moveDirection);
        //    Debug.Log("Receiver " + moveDirection);
        //}
    }
    #endregion

}
