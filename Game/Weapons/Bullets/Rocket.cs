using UnityEngine;
using System.Collections;

public class Rocket : Bullet
{
	
    #region Update
	void FixedUpdate () {
        //Debug.Log(_direction);
		transform.Translate(Vector3.forward * (1f / weight) * Time.deltaTime);
    }
    #endregion

}
