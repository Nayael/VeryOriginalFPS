using System.Collections.Generic;
using UnityEngine;

public class RocketLauncher : AWeapon, IWeapon
{

    #region Private Members
    private static float strength = 25f;
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

        Camera fpsCam = owner.GetComponent<FPSController>().FPSCamera;
        Ray ray = fpsCam.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));
        Debug.DrawRay(ray.origin, ray.direction * 500000f, Color.red, 556564665416f);
        //Debug.Log(ray.origin + " | " + ray.direction);
		RaycastHit hit;
        Vector3 direction = Vector3.zero;

        if (Physics.Raycast(ray, out hit, Mathf.Infinity)) {
            Collider collider = hit.collider;
// BUG : Never manage to get the correct hit point.
            if (collider.GetComponent<Unit>() != null) {
                Debug.DrawRay(ray.origin, hit.point * 500000f, Color.yellow, 556564665416f);
                direction = hit.point;
                Debug.Log(direction);
            }
        } else {
            direction = ray.direction;
        }

        base.Shoot(direction);
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