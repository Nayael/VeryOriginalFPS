using UnityEngine;
using System.Collections;
using System.Collections.Generic;

abstract public class AWeapon : MonoBehaviour, IWeapon
{

    #region Public Members
	public Queue<Bullet> bullets = new Queue<Bullet>();
    public int baseAmmo = 0;
    public int baseCooldown = 0;
    public Unit owner;
    public string bulletType;
    public Vector3 positionInCamera = Vector3.zero;
    #endregion
	
    #region Protected Members
    protected int _cooldown = 0;
    protected int _ammo = 0;    
    #endregion

    #region Properties
    public int Cooldown {
        get { return _cooldown; }
    }
    public int Ammo {
        get {
            return _ammo;
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
    public void OnNetworkInstantiate(NetworkMessageInfo info) {
        transform.eulerAngles = new Vector3(90f, transform.eulerAngles.y, transform.eulerAngles.z);
        enabled = false;
        gameObject.renderer.enabled = false;
        Init();
    }

    public void Init() {
        _ammo = baseAmmo;
    }
    #endregion

    #region Methods
    abstract public void Shoot();

	public virtual void Shoot(Vector3 direction) {
        Shoot(this.transform.position, direction);
	}

    public virtual void Shoot(Vector3 position, Vector3 direction) {
        Ammo--;
    }

    public void EndCooldown() {
        _cooldown = 0;
    }

    abstract public bool CanHit(Unit unit);
    #endregion
}