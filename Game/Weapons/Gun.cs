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
        if (_cooldown != 0) {
            return;
        }
        _cooldown = baseCooldown;
        Ammo--;

        Camera fpsCam = _owner.GetComponent<FPSController>().FPSCamera;
        Ray ray = fpsCam.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));
        //Debug.DrawRay(ray.origin, ray.direction * 500000f, Color.red, 556564665416f);
        //Debug.Log(ray.origin + " | " + ray.direction);
		RaycastHit hit;
        Vector3 direction = Vector3.zero;

        if (Physics.Raycast(ray, out hit, Mathf.Infinity)) {
            Collider collider = hit.collider;
            if (collider.GetComponent<Unit>() != null) {
                NetworkPlayer target = collider.GetComponent<FPSController>().owner;

                collider.GetComponent<Unit>().networkView.RPC("ShootAt", RPCMode.Server, Gun.strength, target);
                direction = hit.point;
                //Debug.DrawRay(ray.origin, hit.point * 500000f, Color.yellow, 556564665416f);
                //Debug.Log(direction);
            }
        } else {
            direction = ray.direction;
        }

        //base.Shoot(direction);
    }

    //override public void Shoot(Vector3 position, Vector3 direction) {
    //    if (_cooldown != 0) {
    //        return;
    //    }
    //    base.Shoot(position, direction);
    //    _cooldown = baseCooldown;
    //    GunBullet bullet = (GunBullet)BulletsManager.Instance.GetBullet(bulletType);
    //    bullet.Fire(_owner, position, direction);   // Fire the bullet
    //}
    #endregion
}