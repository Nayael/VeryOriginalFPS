using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// Weapon Manager Singleton. Contains the weapons pool.
/// </summary>
public sealed class WeaponManager
{

    #region Singleton Stuff
    private static readonly WeaponManager instance = new WeaponManager();

    public static WeaponManager Instance {
        get {
            return instance; 
        }
    }
    #endregion

    #region Public Members
    public int poolSize = 3;
    public Dictionary<string, Object> prefabs = new Dictionary<string, Object>();
    public Dictionary<string, Queue<AWeapon>> pool = new Dictionary<string, Queue<AWeapon>>();
    #endregion

    #region Properties
    #endregion

    #region Initialization
    private WeaponManager() {
        prefabs["Gun"] = Resources.Load("Gun");
        AddAWeaponType("Gun", prefabs["Gun"]);
    }
    #endregion

    #region Methods
    public void AddAWeaponType(string type, Object weaponPrefab) {
		// We create a pool of AWeapon
		Queue<AWeapon> queue = new Queue<AWeapon>();
        AWeapon weapon;
		for (int i = 0; i < poolSize; i++) {
            weapon = (AWeapon)( (GameObject)Object.Instantiate(weaponPrefab) ).GetComponent(type);
            queue.Enqueue(weapon);
		}
		pool[type] = queue;
	}

    public AWeapon GetWeapon(string type) {
        AWeapon weapon;
        if (pool[type].Count == 0) {
            Object weaponPrefab = GetWeaponPrefab(type);
            weapon = (AWeapon)((GameObject)Object.Instantiate(weaponPrefab)).GetComponent(type);
            pool[type].Enqueue(weapon);
        }
        weapon = pool[type].Dequeue();
        weapon.Init();
        return weapon;
	}

    public Transform GetWeaponPrefab(string type) {
        return (Transform)Object.Instantiate(prefabs[type]);
    }
    #endregion

}
