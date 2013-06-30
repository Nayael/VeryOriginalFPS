using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Rocket : Bullet
{
    #region Public Members
    public Transform detonatorPrefab;
    #endregion

    #region Private Members
    //private List<Transform> detonators = new List<Transform>();
    #endregion

    #region Update
    void FixedUpdate () {
        if (Network.isServer) {
            transform.Translate(transform.forward * (1f / weight) * Time.deltaTime, Space.World);
        }
    }
    #endregion

    #region Methods
    void Explode(GameObject touchedTarget) {
        Transform detonator = (Transform)Network.Instantiate(detonatorPrefab, this.transform.position, Quaternion.identity, 0);
        detonator.gameObject.GetComponent<SphereExplosion>().TouchedTarget = touchedTarget;   // We pass to the explosion instance the GameObject that the bullet touched, so that it doesn't get affected by the explosion
        detonator.gameObject.GetComponent<SphereExplosion>().Bullet = this;
    }

    protected override void OnTriggerEnter(Collider other) {
        base.OnTriggerEnter(other);
        if (Network.isServer) {
            Explode(other.gameObject);
        }
    }
    #endregion
}
