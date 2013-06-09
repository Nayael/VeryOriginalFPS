using UnityEngine;
using System.Collections;
using System.Collections.Generic;

abstract public class AWeapon : MonoBehaviour, IWeapon
{

    #region Public Members
	public Queue<Bullet> bullets = new Queue<Bullet>();
	public Bullet bulletPrefab;
    #endregion
	
    #region Protected Members
	protected Unit owner;
    #endregion

    #region Properties
    public Unit Owner {
        get;
        set;
    }
    #endregion

    #region Initialization
    void Start() {
        //if (!BulletsManager.Instance.pool.ContainsKey(this.GetType().Name)) {
        //    BulletsManager.Instance.AddBulletType(this.GetType().Name, bulletPrefab);
        //}
	}
    #endregion

    #region Methods
    /**
	 * The weapon shoots a bullet
	 */
	public void Shoot(Vector3 direction) {
		Shoot(owner.transform.localPosition, direction);
	}

	abstract public void Shoot(Vector3 position, Vector3 direction);
    #endregion

}