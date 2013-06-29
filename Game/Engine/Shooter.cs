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
    private int _currentWeapon = 0;
    #endregion

    #region Public Members
    public bool autoEquip = true;
    public int frags = 0;
    public int deaths = 0;
    public NetworkPlayer owner;
    #endregion

    #region Properties
    public AWeapon Weapon {
        get { return _weapon; }
    }
    #endregion

    #region Initialization
    void Awake() {
        enabled = false;
	}

    // We reinitialize the weapons ammo (used for respawn)
    public void Init() {
        foreach (AWeapon possessedWeapon in _weapons) {
            possessedWeapon.Init();
        }
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
        if (Input.GetAxis("Mouse ScrollWheel") > 0f) {
            NextWeapon();
        } else if (Input.GetAxis("Mouse ScrollWheel") < 0f) {
            PreviousWeapon();
        }
        networkView.RPC("SendWeaponRotation", RPCMode.Server, _weapon.transform.rotation);
    }
    #endregion

    #region Methods
    void EquipWeapon(int weaponIndex) {
        // First, we deactivate the previous weapon
        if (_weapon != null) {
            //WeaponManager.Instance.PutWeaponBack(_weapon);
            _weapon.gameObject.SetActiveRecursively(false);
            _weapon.enabled = false;
        }

        // We activate the new weapon
        _weapon = _weapons[weaponIndex];
        _weapon.gameObject.SetActiveRecursively(true);
        _weapon.enabled = true;
        _currentWeapon = weaponIndex;

        _weapon.transform.localPosition = _weapon.positionInCamera;
        Vector3 weaponPosition = _weapon.transform.TransformPoint(Vector3.zero);
        //Debug.Log(_weapon.GetType().ToString() + " weaponPosition " + _weapon.transform.localPosition + weaponPosition);
        networkView.RPC("EquipWeaponRemote", RPCMode.OthersBuffered, _weapon.GetType().ToString(), weaponPosition);
    }

    void NextWeapon() {
        _currentWeapon++;
        if (_currentWeapon >= _weapons.Count) {
            _currentWeapon = 0;
        }
        EquipWeapon(_currentWeapon);
    }

    void PreviousWeapon() {
        _currentWeapon--;
        if (_currentWeapon < 0) {
            _currentWeapon = _weapons.Count - 1;
        }
        EquipWeapon(_currentWeapon);
    }

    #region Networking
    void OnSerializeNetworkView(BitStream stream, NetworkMessageInfo info) {
        if (_weapon != null) {
            Quaternion weaponRot = Quaternion.identity;
            if (stream.isWriting) {
                weaponRot = _weapon.transform.rotation;
                stream.Serialize(ref weaponRot);
            } else if (Network.player != owner) {
                stream.Serialize(ref weaponRot);
                _weapon.transform.rotation = weaponRot;
            }
        }
    }
    #endregion

    #region RPC
    [RPC]
    void AddFrag() {
        frags++;
    }

    /**
     * Called when another client has equiped a weapon
     */
    [RPC]
    void EquipWeaponRemote(string weaponType, Vector3 position) {
        // If there was a previous weapon, put it back in the pool
        if (_weapon != null) {
            WeaponManager.Instance.PutWeaponBack(_weapon);
            _weapon = null;
        }

        // Get an instance of the weapon type, and add it as a child of the owner
        _weapon = WeaponManager.Instance.GetWeapon(weaponType);
        _weapon.gameObject.SetActiveRecursively(true);
        _weapon.enabled = true;
        _weapon.owner = this.GetComponent<Unit>();
        _weapon.transform.position = position;
        _weapon.transform.parent = _weapon.owner.transform;
        Debug.Log("EquipWeaponRemote " + _weapon.transform.localPosition);
    }

    [RPC]
    public void TakeWeapon(string weaponType) {
        if (Network.isClient) {
            // If the weapon is already in the inventory, then don't continue
            foreach (AWeapon possessedWeapon in _weapons) {
                if (possessedWeapon.GetType().ToString() == weaponType) {
                    return;
                }
            }

            // If it's a new weapon, we get one from the WeaponManager, and add it to the inventory
            AWeapon weapon = WeaponManager.Instance.GetWeapon(weaponType);
            Transform fpsCam = GetComponentInChildren<FPSCamera>().transform;
            weapon.transform.parent = fpsCam;
            weapon.owner = this.GetComponent<Unit>();
            _weapons.Add(weapon);

            // Equip it if necessary
            if (autoEquip == true) {
                EquipWeapon(_weapons.IndexOf(weapon));
            }
        }
    }

    [RPC]
    void SendWeaponRotation(Quaternion rotation) {
        if (!Network.isServer) {
            return;
        }
        _weapon.transform.rotation = rotation;
    }
    #endregion

    #endregion
}
