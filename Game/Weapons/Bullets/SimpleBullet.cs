using UnityEngine;
using System.Collections;

public class SimpleBullet : Bullet
{
	
    #region Update
	// Update is called once per frame
	override protected void Update () {
		transform.Translate((direction != Vector3.zero ? direction : Vector3.up) * (100f / weight) * Time.deltaTime);
		base.Update();
    }
    #endregion

}
