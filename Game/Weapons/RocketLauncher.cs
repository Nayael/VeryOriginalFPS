﻿using System.Collections.Generic;
using UnityEngine;

public class RocketLauncher : AWeapon, IWeapon
{

    #region Update
    void Update() {
        if (_cooldown > 0) {
            _cooldown--;
        }
    }
    #endregion

    #region Shoot
    override public void Shoot() {
        if (_cooldown != 0 || Network.isServer || _ammo <= 0) {
            return;
        }

        Camera fpsCam = owner.GetComponent<FPSController>().FPSCamera;
        Ray ray = fpsCam.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));
		RaycastHit hit;
        Vector3 destination = Vector3.zero;

        if (Physics.Raycast(ray, out hit, Mathf.Infinity)) {
            // If the raycast hit something, make the bullet go to the hit point
            
            Collider collider = hit.collider;
            // If the player was aiming at a unit
            if (collider.gameObject.GetComponent<Unit>() != null) {

            }
            destination = hit.point;
        } else {
            // Otherwise, the player was aiming at the void (the sky for example), so make the bullet go to a point very very far
            destination = fpsCam.ViewportToWorldPoint(new Vector3(0.5f, 0.5f, 999999999999f));    // Look at the point to infinity (hard coded value, because it's impossible to use Mathf.Infinity for LookAt)
        }

        base.Shoot(destination);
    }

    public override bool CanHit(Unit unit) {
        throw new System.NotImplementedException();
    }

    override public void Shoot(Vector3 position, Vector3 direction) {
        if (_cooldown != 0) {
            return;
        }
        _cooldown = baseCooldown;
        base.Shoot(position, direction);
        _cooldown = baseCooldown;
        Rocket bullet = (Rocket)BulletsManager.Instance.GetBullet(bulletType);
        bullet.Fire(owner, position, direction);   // Fire the bullet
    }
    #endregion
}