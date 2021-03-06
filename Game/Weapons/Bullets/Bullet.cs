﻿using UnityEngine;
using System.Collections;

abstract public class Bullet : MonoBehaviour
{

    #region Public Members
    public Health health;	    // Every bullet has a health points count
	public float weight;		// The bullet's weight (defines its speed)
    public float strength;		// The bullet's strength (damages it causes)
    public AWeapon weapon;
    #endregion

    #region Protected Members
	protected Vector3 _direction;   // The bullet's direction
    protected NetworkPlayer _shooter;
    #endregion

    #region Initialization
    /// <summary>
    /// Initializes the bullet (use after the bullet was extracted from the pool)
    /// </summary>
    public void Init() {
        Deactivate();
        _direction = Vector3.zero;
        health.Fill();
    }

    void OnNetworkInstantiate(NetworkMessageInfo info) {
        if (!networkView.isMine) {
            BulletsManager.Instance.PutBullet(this);
        }
    }
    #endregion

    #region Update
    protected virtual void Update() {
        // Only the server takes care of the bullets' health
        if (Network.isServer) {
            health.Current--;		    // The bullet degrades itself
            if (health.Current <= 0) {	// Once its lifetime is over
                Kill();		    // We destroy the bullet
            }
        }
	}
    #endregion

    #region Methods
    /// <summary>
    /// Fires the bullet
    /// </summary>
    /// <param name="owner">The bullet's owner</param>
    /// <param name="localPosition">The bullet's starting localPosition</param>
    public void Fire(Unit owner, Vector3 position) {
        transform.parent = null;
        transform.position = position;
        transform.LookAt(_direction);
        transform.Translate(transform.forward * owner.GetComponent<FPSController>().MoveDirection.magnitude * Time.deltaTime, Space.World);
        Activate();
        networkView.RPC("FireRemote", RPCMode.OthersBuffered, Network.player, position, _direction);
	}

    /// <summary>
    /// Fires the bullet
    /// </summary>
    /// <param name="owner">The bullet's owner</param>
    /// <param name="localPosition">The bullet's starting localPosition</param>
    /// <param name="direction">The direction where the bullet will go</param>
	public virtual void Fire(Unit owner, Vector3 position, Vector3 direction) {
		_direction = direction;
		Fire(owner, position);
	}

    /// <summary>
    /// Applies the effect of the bullet
    /// </summary>
    /// <param name="target">The target that was touched by the bullet</param>
    public void Apply(GameObject target) {
        if (target.GetComponent<Unit>() == null) {
            return;
        }
        target.GetComponent<Unit>().networkView.RPC("Hurt", target.GetComponent<FPSController>().owner, this.strength, this._shooter);
        target.GetComponent<Unit>().Hurt(this.strength, this._shooter);
    }

    /// <summary>
    /// Makes the bullet visible and activates it
    /// </summary>
    public void Activate() {
        enabled = true;
        transform.FindChild("Body").renderer.enabled = true;
        GetComponent<CapsuleCollider>().enabled = true;
    }

    /// <summary>
    /// Makes the bullet invisible and deactivates it
    /// </summary>
    public void Deactivate() {
        enabled = false;
        transform.FindChild("Body").renderer.enabled = false;
        GetComponent<CapsuleCollider>().enabled = false;
    }

    /// <summary>
    /// Deactivates the bullet and puts it back into the pool
    /// </summary>
	public virtual void Kill() {
		Deactivate();
        _direction = Vector3.zero;
        BulletsManager.Instance.PutBullet(this);	// We put it back in the pool
        networkView.RPC("PutInPool", RPCMode.OthersBuffered);
	}

    #region RPC
    [RPC]
    void FireRemote(NetworkPlayer shooter, Vector3 position, Vector3 direction) {
        transform.parent = null;
        if (Network.isServer) {
            Activate();
            _direction = direction;
            _shooter = shooter;
            transform.position = position;
            transform.LookAt(_direction);
            networkView.RPC("ValidateBulletShoot", _shooter);    // We tell the shooterComponent that the server is taking care of the bullet's movement
        
        // If we are a client, then we just make the bullet visible
        } else {
            transform.FindChild("Body").renderer.enabled = true;
        }
    }

    [RPC]
    void RemoveFromPool() {
        BulletsManager.Instance.RemoveBullet(this);
    }

    [RPC]
    void PutInPool() {
        BulletsManager.Instance.PutBullet(this);
    }

    [RPC]
    /// <summary>
    /// Called from the server to the client who shot. It tells him that the server is taking care of the bullet's movement
    /// </summary>
    void ValidateBulletShoot() {
        GetComponent<CapsuleCollider>().enabled = false;
    }
    #endregion

    #endregion

    #region Events
    /// <summary>
    /// Triggered when the bullet is collided
    /// </summary>
    /// <param name="other">The collider of the colliding object</param>
    protected virtual void OnTriggerEnter(Collider other) {
        if (Network.isServer) {
            // If the bullet has touched a Unit, we apply the effect of the bullet
            if (other.gameObject.GetComponent<Unit>() != null) {
                Apply(other.gameObject);
            }
            Kill(); // The bullet gets detroyed once it's touched
        }
    }
    #endregion

}