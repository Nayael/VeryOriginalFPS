using UnityEngine;
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
	protected Vector3 _direction;   // The bullet's _direction can be defined
    #endregion

    #region Initialization
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
        health.Current--;		    // The bullet degrades itself
        Debug.Log(health);
        if (health.Current <= 0) {	// Once its lifetime is over
			Destroy();		    // We destroy the bullet
		}
	}
    #endregion

    #region Methods
    public void Fire(Unit owner, Vector3 position) {
        transform.parent = null;
        transform.position = position;
        transform.LookAt(_direction);
        Activate();
        networkView.RPC("FireRemote", RPCMode.OthersBuffered, Network.player, position, _direction);
	}

	public virtual void Fire(Unit owner, Vector3 position, Vector3 direction) {
		_direction = direction;
		Fire(owner, position);
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

	public void Destroy() {
		Deactivate();
        _direction = Vector3.zero;
        BulletsManager.Instance.PutBullet(this);	// We put it back in the pool
        networkView.RPC("PutInPool", RPCMode.OthersBuffered);
	}

    protected void GameOver() {
        Destroy();
    }

    #region RPC
    [RPC]
    void FireRemote(NetworkPlayer shooter, Vector3 position, Vector3 direction) {
        transform.parent = null;
        if (Network.isServer) {
            Activate();
            _direction = direction;
            transform.position = position;
            transform.LookAt(_direction);
            networkView.RPC("ValidateBulletShoot", shooter);    // We tell the shooter that the server is taking care of the bullet's movement
        
        // If we are a client, then we just make the bullet visible
        } else {
            transform.FindChild("Body").renderer.enabled = true;
        }
    }

    [RPC]
    void RemoveFromPool() {
        Debug.Log("RemoveFromPool");
        BulletsManager.Instance.RemoveBullet(this);
    }

    [RPC]
    void PutInPool() {
        Debug.Log("PutInPool");
        BulletsManager.Instance.PutBullet(this);
    }

    [RPC]
    /// <summary>
    /// Called from the server to the client who shot. It tells him that the server is taking care of the bullet's movement
    /// </summary>
    void ValidateBulletShoot() {
        enabled = false;
        GetComponent<CapsuleCollider>().enabled = false;
    }
    #endregion

    #endregion

    #region Events
    protected void OnTriggerEnter(Collider other) {
        if (Network.isServer) {
            if (true) {

            }
        }
    }
    #endregion

}