using UnityEngine;
using System.Collections;

public class ABonus : MonoBehaviour {

    #region Public Members
    public float respawnTime = 20f; 
    #endregion

    #region Private Members
    private float respawnTimer = 5f;
    private bool visible = true;
    #endregion

    #region Initialization
    void Start() {
        respawnTimer = respawnTime;
    } 
    #endregion

    #region Methods
    void Update() {
        if (!this.visible) {
            respawnTimer -= Time.deltaTime;
            if (respawnTimer <= 0f) {
                Spawn();
            }
        } else {
            this.transform.RotateAround(Vector3.up, 0.05f);
        }
	}

    void OnTriggerEnter(Collider other) {
        if (this.visible && other.GetComponent<Unit>() != null) {
            this.Apply(other.GetComponent<Unit>());
        }
    }

    void Spawn() {
        Renderer[] childrenRenderers = this.GetComponentsInChildren<Renderer>();
        foreach (Renderer meshRenderer in childrenRenderers) {
            meshRenderer.enabled = true;
        }
        visible = true;
        respawnTimer = respawnTime;
    }

    [RPC]
    protected virtual void Apply(Unit target) {
        Renderer[] childrenRenderers = this.GetComponentsInChildren<Renderer>();
        foreach (Renderer meshRenderer in childrenRenderers) {
            meshRenderer.enabled = false;
        }
        visible = false;
    }
    #endregion
}
