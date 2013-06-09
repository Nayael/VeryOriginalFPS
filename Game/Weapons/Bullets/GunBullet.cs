using UnityEngine;
using System.Collections;

public class GunBullet : Bullet
{
	
    #region Update
	override protected void Update () {
		transform.Translate((direction != Vector3.zero ? direction : Vector3.up) * (100f / weight) * Time.deltaTime);
		base.Update();
    }
    #endregion

}
