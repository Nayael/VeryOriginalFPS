using UnityEngine;
using System.Collections;

public class Ammo : ABonus {

    #region Public Members
    public AWeapon weapon;
    public int quantity = 5; 
    #endregion

    #region Methods
    protected override void Apply(Unit target) {
        base.Apply(target);
        Shooter shooterComponent = target.GetComponent<Shooter>();
        if (shooterComponent != null) {
            AWeapon shooterWeapon = shooterComponent.GetWeaponOfType(this.weapon.GetType());
            if (shooterWeapon != null) {
                shooterWeapon.Ammo += this.quantity;
            }
        }
    } 
    #endregion
}
