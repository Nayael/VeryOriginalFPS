using UnityEngine;
using System.Collections;

public class Medkit : ABonus {

    #region Public Members
    public float value = 10.9f; 
    #endregion

    #region Methods
    protected override void Apply(Unit target) {
        base.Apply(target);
        target.GetComponent<Health>().Current += this.value;
    }
    #endregion
}
