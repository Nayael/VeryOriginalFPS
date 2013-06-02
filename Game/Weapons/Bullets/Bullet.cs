using UnityEngine;
using System.Collections;

abstract public class Bullet : MonoBehaviour
{

    #region Public Members
    public Transform prefab;	// The bullet prefab
	public float hp;			// Every bullet has a health points count
	public float weight;		// The bullet's weight (defines its speed)
	public float strength;		// The bullet's strength (damages he inflicts)
	public string weapon;		// The weapon that shoots the bullet
    #endregion

    #region Protected Members
    protected float currentHP;	// The current HP value
	protected Vector3 direction;// The bullet's direction can be defined
	protected bool evil;
    #endregion

    #region Initialization
    void Start() {
		enabled = false;
		gameObject.active = false;
        //GameEventManager.GameOver += GameOver;
	}
    #endregion

    #region Update
    protected virtual void Update() {
		currentHP -= 0.1f;		// The bullet degrades itself
		if (currentHP <= 0f) {	// Once its lifetime is over
			this.Destroy();		// We destroy the bullet
		}
	}
    #endregion

    #region Methods
    public void Fire(Unit character, Vector3 position) {
        //evil = !(character is Hero);
        //currentHP = hp;
        //transform.parent = null;
        //transform.localPosition = position;
        //gameObject.active = true;
        //enabled = true;
	}

	public virtual void Fire(Unit character, Vector3 position, Vector3 direction) {
		this.direction = direction;
		Fire(character, position);
	}

	public void Destroy() {
		enabled = false;
		gameObject.active = false;
		BulletsManager.Instance.pool[weapon].Enqueue(this);	// We put it back in the pool
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

        //// If the bullet hit a object that is not a character, stop here
        //Character target;
        //if ((target = collision.gameObject.GetComponent<Character>()) == null) {
        //    return;
        //}
        //target.Hurt(this.strength);
    }
    #endregion

}