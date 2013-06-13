using UnityEngine;
using System.Collections;

public class FPSCamera : MonoBehaviour {

    #region Private Members
    private float vRotAngle = 0f;
    private float mouseYSensitivity = 100f;
    #endregion

    #region Public Members
    public float maxVerticalAngle;
    #endregion

    #region Properties
    public float MouseYSensitivity { get; set; }
    #endregion

    #region Update
    void Update() {
        vRotAngle = -1 * Input.GetAxis("Mouse Y") * mouseYSensitivity * Time.deltaTime;

        this.transform.Rotate(vRotAngle, 0f, 0f);

        // Capping the vertical rotation of the camera
        if (this.transform.localRotation.x < -maxVerticalAngle) {
            Quaternion correctRot = this.transform.localRotation;
            correctRot.x = -maxVerticalAngle;
            this.transform.localRotation = correctRot;
        } else if (this.transform.localRotation.x > maxVerticalAngle) {
            Quaternion correctRot = this.transform.localRotation;
            correctRot.x = maxVerticalAngle;
            this.transform.localRotation = correctRot;
        }
    }
    #endregion
}
