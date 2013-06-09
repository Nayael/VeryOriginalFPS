using System;
using System.Collections.Generic;
using UnityEngine;

public class Gun : AWeapon, IWeapon
{

    public override void Shoot(Vector3 position, Vector3 direction) {
        throw new NotImplementedException();
    }
}
