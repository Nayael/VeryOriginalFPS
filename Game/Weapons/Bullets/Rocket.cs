using UnityEngine;
using System.Collections;

public class Rocket : Bullet
{
    #region Public Members
    public Transform detonatorPrefab;
    #endregion

    #region Private Members
    private Transform detonator; 
    #endregion

    #region Update
    void FixedUpdate () {
        if (Network.isServer) {
            transform.Translate(transform.forward * (1f / weight) * Time.deltaTime, Space.World);
        }
    }
    #endregion

    #region Methods
    public override void Kill() {
        base.Kill();
        detonator = (Transform)Network.Instantiate(detonatorPrefab, this.transform.position, Quaternion.identity, 0);
        //networkView.RPC("Explode", RPCMode.All);
    }

    [RPC]
    void Explode() {
        detonator = (Transform)Instantiate(detonatorPrefab, this.transform.position, Quaternion.identity);
    }
    #endregion
}
