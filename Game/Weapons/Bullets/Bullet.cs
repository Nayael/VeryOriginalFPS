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
    void Awake() {
        Init();
		//GameEventManager.GameOver += GameOver;
	}

    public void Init() {
        enabled = false;
        gameObject.SetActiveRecursively(false);
        _direction = Vector3.zero;
        health.Fill();
    }
    #endregion

    #region Update
    protected virtual void Update() {
        health.Current--;		    // The bullet degrades itself
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
        gameObject.SetActiveRecursively(true);
        enabled = true;
	}

	public virtual void Fire(Unit owner, Vector3 position, Vector3 direction) {
		_direction = direction;
		Fire(owner, position);
	}

	public void Destroy() {
		enabled = false;
		gameObject.SetActiveRecursively(false);
        _direction = Vector3.zero;
        BulletsManager.Instance.pool[GetType().ToString()].Enqueue(this);	// We put it back in the pool
	}

	protected void GameOver () {
		Destroy();
	}
    #endregion

    #region Events
    public void OnCollisionEnter(Collision collision) {
        //// If the bullet was shot by the hero and hits him, then we stop the function
        //if (collision.gameObject.GetComponent<Hero>() != null && !this.evil
        //|| collision.gameObject.GetComponent<Hero>() == null && this.evil) {
        //    return;
        //}
        //this.Destroy();	// Everytime the bullet hits something, it explodes

        //// If the bullet hit a object that is not a owner, stop here
        //Character target;
        //if ((target = collision.gameObject.GetComponent<Character>()) == null) {
        //    return;
        //}
        //target.Hurt(this.strength);
    }
    #endregion

}