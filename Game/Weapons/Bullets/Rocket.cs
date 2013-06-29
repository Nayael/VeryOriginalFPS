using UnityEngine;
using System.Collections;

public class Rocket : Bullet
{
    #region Update
    void FixedUpdate () {
        if (Network.isServer) {
            transform.Translate(transform.forward * (1f / weight) * Time.deltaTime, Space.World);
        }
    }
    #endregion
}
