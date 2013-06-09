using UnityEngine;
using System.Collections;
using System.Collections.Generic;

abstract public class AWeapon : MonoBehaviour, IWeapon
{

    #region Public Members
	public Queue<Bullet> bullets = new Queue<Bullet>();
    public int baseAmmo = 0;
    public int baseCooldown = 0;
    public string bulletType;
    #endregion
	
    #region Protected Members
	protected Unit _owner;
    protected int _cooldown = 0;
    protected int _ammo = 0;
    #endregion

    #region Properties
    public Unit Owner {
        get { return _owner; }
        set { _owner = value; }
    }
    public int Cooldown {
        get { return _cooldown; }
    }
    public int Ammo {
        get {
            return _cooldown;
        }
        set {
            if (value < 0) {
                value = 0;
            }
            _ammo = value;
        }
    }
    #endregion

    #region Initialization
    public void Init() {
        
	}
    #endregion

    #region Methods
    abstract public void Shoot();

	public virtual void Shoot(Vector3 direction) {
        Shoot(this.transform.position, direction);
	}

    public virtual void Shoot(Vector3 position, Vector3 direction) {
        Ammo--;
        //Debug.Log("Shoot " + bulletType);
    }
    #endregion


    public void EndCooldown() {
        _cooldown = 0;
    }
}