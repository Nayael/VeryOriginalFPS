using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Unit : MonoBehaviour
{
    #region Public Members
    public float speed;
    public float jumpSpeed;
	public float angularSpeed;
    public Vector2 mouseSensitivity;
    #endregion

    #region Initialization
    void OnNetworkInstantiate(NetworkMessageInfo info) {
        //if (!networkView.isMine) {
        //    enabled = false;
        //    return;
        //}

        //FPSController fpsController = gameObject.AddComponent<FPSController>();
        //fpsController.speed = speed;
        //fpsController.jumpSpeed = jumpSpeed;
        //fpsController.angularSpeed = angularSpeed;
        //fpsController.mouseSensitivity = mouseSensitivity;

        //Camera.main.transform.parent = this.transform;
        //Camera.main.transform.localPosition = Vector3.zero;
        //Camera.main.transform.localRotation = Quaternion.identity;
	}
    #endregion

    #region Networking
    /*void OnSerializeNetworkView(BitStream stream, NetworkMessageInfo info) {
        float health = 0f;
        //Vector3 moveDirection = Vector3.zero;

        // Sending
        if (stream.isWriting) {
            health = Random.Range(0f, 100f);
            //moveDirection = GetComponent<FPSController>().MoveDirection * Time.deltaTime;
            //stream.Serialize(ref moveDirection);
            stream.Serialize(ref health);
            //Debug.Log("Sender " + osef);

        // Receiveing
        } else {
            //stream.Serialize(ref moveDirection);
            stream.Serialize(ref health);
            //GetComponent<FPSController>().MoveDirection = moveDirection;
            //Debug.Log("Receiver " + health);
        }
    }*/
    #endregion

}
