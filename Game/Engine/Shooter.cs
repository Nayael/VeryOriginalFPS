using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// A Component for any GameObject that can shoot
/// </summary>
public class Shooter : MonoBehaviour
{

    #region Private Members
    private AWeapon _weapon;
    private List<AWeapon> _weapons = new List<AWeapon>();
    #endregion

    #region Public Members
    public bool autoEquip = true;
    #endregion

    #region Initialization
    void Awake() {
        enabled = false;
	}
    #endregion

    #region Update
    void Update() {
        if (_weapon == null) {
            return;
        }
        if (Input.GetButton("Fire1")) {
            if (_weapon.Cooldown == 0) {
                _weapon.Shoot();
            }
        }
        if (Input.GetButtonUp("Fire1")) {
            _weapon.EndCooldown();
        }
	}
    #endregion

    #region RPC
    void EquipWeapon(AWeapon weapon) {
        // First, we make the previous weapon invisible
        if (_weapon != null) {
            _weapon.transform.renderer.enabled = false;
        }

        _weapon = weapon;
        _weapon.transform.renderer.enabled = true;

        _weapon.transform.localPosition = new Vector3(0.34f, -0.35f, 0.61f);
        networkView.RPC("EquipWeaponRemote", RPCMode.OthersBuffered, _weapon.GetType().ToString());
    }

    [RPC]
    void EquipWeaponRemote(string weaponType) {

        // Change weapon
        AWeapon weapon = WeaponManager.Instance.GetWeapon(weaponType);
        _weapon = weapon;
        Camera fpsCam = GetComponent<FPSController>().FPSCamera;
        _weapon.transform.parent = fpsCam.transform;
        _weapon.transform.localPosition = new Vector3(0.34f, -0.35f, 0.61f);
    }

    [RPC]
    void TakeWeapon(string weaponType) {
        // If the weapon is already in the inventory, then don't continue
        foreach (AWeapon possessedWeapon in _weapons) {
            if (possessedWeapon.GetType().ToString() == weaponType) {
                return;
            }
        }

        // If it's a new weapon, we get one from the WeaponManager, and add it to the inventory
        AWeapon weapon = WeaponManager.Instance.GetWeapon(weaponType);
        Camera fpsCam = GetComponent<FPSController>().FPSCamera;
        weapon.transform.parent = fpsCam.transform;
        weapon.Owner = GetComponent<Unit>();
        _weapons.Add(weapon);

        // Equip it if necessary
        if (autoEquip == true) {
            EquipWeapon(weapon);
        }
    }
    #endregion
}
