using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// This create a sphere of damage around a GameObject. Can apply the effect of a bullet
/// </summary>
public class SphereExplosion : MonoBehaviour {

    #region Private Members
    private List<GameObject> touchedTargets = new List<GameObject>();
    private Bullet bullet;
    private float timeLeft;
    #endregion

    #region Public Members
    public float hitRadius = 5f;    // The radius of the hitbox
    public float duration = 2f;     // The duration of the collision test
    #endregion

    #region Properties
    public GameObject TouchedTarget {
        set { this.touchedTargets.Add(value);  }
    }
    public Bullet Bullet {
        set { this.bullet = value; }
    }
    #endregion

    #region Initialization
    void Start() {
        timeLeft = duration;
    } 
    #endregion

    #region Update
    void Update() {
        timeLeft -= Time.deltaTime; // We update the timer
        if (timeLeft <= 0f) {       // If the timer is over, then we stop the collision now
            this.enabled = false;
        }
    }
    void FixedUpdate() {
        // We create a sphere of effect
        Collider[] targets = Physics.OverlapSphere(this.transform.position, this.hitRadius);
        foreach (Collider overlappedTarget in targets) {
            // We hurt the target only if it has not been already touched by the explosion, and if it is a Unit
            if (overlappedTarget.gameObject.GetComponent<Unit>() != null && !this.touchedTargets.Contains(overlappedTarget.gameObject)) {
                this.touchedTargets.Add(overlappedTarget.gameObject);
                if (this.bullet != null) {
                    this.bullet.Apply(overlappedTarget.gameObject);
                }
            }
        }
    } 
    #endregion
}
