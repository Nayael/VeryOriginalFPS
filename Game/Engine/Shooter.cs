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
	}
    #endregion

    #region Methods
    void EquipWeapon(AWeapon weapon) {
        // First, we make the previous weapon invisible
        if (_weapon != null) {
            _weapon.gameObject.renderer.enabled = false;
            _weapon.enabled = false;
        }

        _weapon = weapon;
        _weapon.gameObject.renderer.enabled = true;
        _weapon.enabled = true;

        _weapon.transform.localPosition = new Vector3(0.29f, -0.215f, 0.61f);
        //networkView.RPC("EquipWeaponRemote", RPCMode.OthersBuffered, _weapon.GetType().ToString(), _weapon.transform.localPosition + _weapon.transform.parent.transform.localPosition);
    }
    #endregion

    #region Network
    void OnSerializeNetworkView(BitStream stream, NetworkMessageInfo info) {
        //Debug.Log(stream.isWriting);
        //float weaponRotX = _weapon.transform.rotation.x;
        //if (stream.isWriting) {
        //    stream.Serialize(ref weaponRotX);
        //} else {
        //    stream.Serialize(ref weaponRotX);
        //    _weapon.transform.rotation = new Quaternion(weaponRotX, _weapon.transform.rotation.y, _weapon.transform.rotation.z, _weapon.transform.rotation.w);
        //}
    } 
    #endregion

    #region RPC
    [RPC]
    void AddFrag() {
        frags++;
    }

    [RPC]
    void EquipWeaponRemote(string weaponType, Vector3 localPosition) {
        /*AWeapon weapon = WeaponManager.Instance.GetWeapon(weaponType);
        _weapon = weapon;
        _weapon.owner = this.GetComponent<Unit>();
        _weapon.transform.parent = _weapon.owner.transform;
        _weapon.transform.position = localPosition;*/
    }

    [RPC]
    public void TakeWeapon(NetworkViewID weaponViewID) {
        if (NetworkView.Find(weaponViewID) == null) {
            Debug.Log("No object found with networkViewID [" + weaponViewID + "]");
            return;
        }
        AWeapon weapon = NetworkView.Find(weaponViewID).gameObject.GetComponent<AWeapon>();

        // If the weapon is already in the inventory, then don't continue
        foreach (AWeapon possessedWeapon in _weapons) {
            if (possessedWeapon.GetType() == weapon.GetType()) {
                return;
            }
        }

        // If it's a new weapon, we get one from the WeaponManager, and add it to the inventory
        Transform fpsCam = GetComponentInChildren<FPSCamera>().transform;
        weapon.transform.parent = fpsCam;
        weapon.owner = GetComponent<Unit>();
        _weapons.Add(weapon);

        // Equip it if necessary
        if (autoEquip == true) {
            EquipWeapon(weapon);
        }
    }
    #endregion
}
