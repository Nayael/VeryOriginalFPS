﻿using UnityEngine;
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
    public string[] weaponNames = new string[] { "RocketLauncher", "Gun" };
    public int poolSize = 3;
    public Dictionary<string, Object> prefabs = new Dictionary<string, Object>();
    public Dictionary<string, Queue<AWeapon>> pool = new Dictionary<string, Queue<AWeapon>>();
    #endregion

    #region Initialization
    private WeaponManager() {
        foreach (string weaponName in weaponNames) {
            prefabs[weaponName] = Resources.Load(weaponName);
            AddAWeaponType(weaponName);
        }
    }
    #endregion

    #region Methods
    public void AddAWeaponType(string type) {
		// We create a pool of AWeapon
		Queue<AWeapon> queue = new Queue<AWeapon>();
        AWeapon weapon;
		for (int i = 0; i < poolSize; i++) {
            weapon = CreateWeaponInstance(type);
            queue.Enqueue(weapon);
		}
		pool[type] = queue;
	}

    public AWeapon GetWeapon(string type) {
        AWeapon weapon;
        // If there is no instance available on the pool, we create another one
        if (pool[type].Count == 0) {
            weapon = CreateWeaponInstance(type);
            pool[type].Enqueue(weapon);
        }
        weapon = pool[type].Dequeue();
        weapon.Init();
        weapon.transform.parent = null;
        return weapon;
	}

    public void PutWeaponBack(AWeapon weapon) {
        weapon.transform.parent = null;
        weapon.gameObject.SetActiveRecursively(false);
        weapon.enabled = false;
        pool[weapon.GetType().ToString()].Enqueue(weapon);
    }

    public AWeapon CreateWeaponInstance(string type) {
        AWeapon weapon = (AWeapon)((GameObject)Object.Instantiate(prefabs[type])).GetComponent(type);
        weapon.gameObject.SetActiveRecursively(false);
        weapon.enabled = false;
        return weapon;
    }
    #endregion

}
