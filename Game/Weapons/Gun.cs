using System.Collections.Generic;
using UnityEngine;

public class Gun : AWeapon, IWeapon
{

    #region Private Members
    private static float strength = 5f;
    #endregion

    #region Update
    void Update() {
        if (_cooldown > 0) {
            _cooldown--;
        }
    }
    #endregion

    #region Shoot
    override public void Shoot() {
        if (_cooldown != 0 || Network.isServer) {
            return;
        }
        _cooldown = baseCooldown;
        Ammo--;

        Camera fpsCam = owner.GetComponent<FPSController>().FPSCamera;
        Ray ray = fpsCam.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));
		RaycastHit hit;

        if (Physics.Raycast(ray, out hit, Mathf.Infinity)) {
            Collider collider = hit.collider;
            if (collider.GetComponent<Unit>() != null) {
                collider.networkView.RPC("ShootAtMe", RPCMode.Server, Gun.strength, fpsCam.transform.parent.GetComponent<FPSController>().owner);
            }
        }
    }

    // Checks if the gun can hit a given target
    override public bool CanHit(Unit unit) {
        Camera fpsCam = owner.GetComponent<FPSController>().FPSCamera;
        Ray ray = fpsCam.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));
		RaycastHit hit;

        if (Physics.Raycast(ray, out hit, Mathf.Infinity)) {
            return true;
// BUG HERE
            //Collider collider = hit.collider;
            //if (collider.GetComponent<Unit>() == unit) {
            //    return true;
            //}
        }
        return false;
    }
    #endregion
}