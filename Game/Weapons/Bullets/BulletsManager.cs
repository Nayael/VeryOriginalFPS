using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class BulletsManager : MonoBehaviour
{
    
    #region Public Members
	public int poolSize;
	public Dictionary<string, Queue<Bullet>> pool = new Dictionary<string, Queue<Bullet>>();
    #endregion

    #region Static Members
    private static BulletsManager instance;
    #endregion

    #region Properties
    public static BulletsManager Instance {
        get {
            if (instance == null) {
                instance = GameObject.FindObjectOfType(typeof(BulletsManager)) as BulletsManager;
            }
            return instance;
        }
    }
    #endregion

    #region Initialization
    private BulletsManager() { }

    void Start () {
		instance = this;
	}
    #endregion

    #region Methods
    public void AddBulletType(string weapon, Bullet bulletPrefab) {
		// We create a pool of bullets
		Queue<Bullet> queue = new Queue<Bullet>();
		Bullet bullet;
		for (int i = 0; i < instance.poolSize; i++) {
			bullet = (Bullet)Instantiate(bulletPrefab);
			bullet.weapon = weapon;
			queue.Enqueue(bullet);
		}
		pool[weapon] = queue;
	}

	public Bullet GetBullet(string weapon) {
		return pool[weapon].Dequeue();
	}
    #endregion

}
