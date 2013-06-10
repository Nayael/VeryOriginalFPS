using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class BulletsManager
{

    #region Singleton Stuff
    private static readonly BulletsManager instance = new BulletsManager();

    public static BulletsManager Instance {
        get {
            return instance;
        }
    }
    #endregion

    #region Public Members
	public int poolSize = 50;
    public string[] bulletNames = new string[] {"Rocket"};
    public Dictionary<string, Object> prefabs = new Dictionary<string, Object>();
    public Dictionary<string, Queue<Bullet>> pool = new Dictionary<string, Queue<Bullet>>();
    #endregion

    #region Initialization
    private BulletsManager() {
        foreach (string bulletName in bulletNames) {
            prefabs[bulletName] = Resources.Load(bulletName);
            AddBulletType(bulletName, prefabs[bulletName]);
        }
    }
    #endregion

    #region Methods
    public void AddBulletType(string type, Object bulletPrefab) {
		// We create a pool of bullets
		Queue<Bullet> queue = new Queue<Bullet>();
		Bullet bullet;
		for (int i = 0; i < poolSize; i++) {
            bullet = (Bullet)((GameObject)Object.Instantiate(bulletPrefab)).GetComponent(type);
			queue.Enqueue(bullet);
		}
        pool[type] = queue;
	}

    public Bullet GetBullet(string type) {
        Bullet bullet;
        if (pool[type].Count == 0) {
            Object bulletPrefab = GetBulletPrefab(type);
            bullet = (Bullet)((GameObject)Object.Instantiate(bulletPrefab)).GetComponent(type);
            pool[type].Enqueue(bullet);
        }
        bullet = pool[type].Dequeue();
        bullet.Init();
        return bullet;
	}

    public Object GetBulletPrefab(string type) {
        return Object.Instantiate(prefabs[type]);
    }
    #endregion

}
