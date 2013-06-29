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
    public Dictionary<string, List<Bullet>> pool = new Dictionary<string, List<Bullet>>();
    #endregion

    #region Initialization
    private BulletsManager() { }

    public void Initialize() {
        if (Network.isClient) {
            return;
        }

        foreach (string bulletName in bulletNames) {
            prefabs[bulletName] = Resources.Load(bulletName);
            AddBulletType(bulletName, prefabs[bulletName]);
        }
    }
    #endregion

    #region Methods
    public void AddBulletType(string type, Object bulletPrefab) {
        if (Network.isClient) {
            return;
        }

		// We create a pool of bullets
		Bullet bullet;
		for (int i = 0; i < poolSize; i++) {
            bullet = CreateBulletInstance(type);
            PutBullet(bullet);
		}
	}

    public void PutBullet(Bullet bullet) {
        if (!pool.ContainsKey(bullet.GetType().ToString())) {
            pool.Add(bullet.GetType().ToString(), new List<Bullet>());
        }
        bullet.Init();
        pool[bullet.GetType().ToString()].Add(bullet);
    }

    public void RemoveBullet(Bullet bullet) {
        pool[bullet.GetType().ToString()].Remove(bullet);
    }

    public Bullet GetBullet(string type) {
        Bullet bullet;
        if (pool[type].Count == 0) {
            bullet = CreateBulletInstance(type);
            PutBullet(bullet);
        }
        bullet = pool[type][0];
        pool[type].RemoveAt(0);
        bullet.Init();
        return bullet;
	}

    public Bullet CreateBulletInstance(string type) {
        Bullet bullet = (Bullet)((GameObject)Network.Instantiate(
            prefabs[type], Vector3.zero, Quaternion.identity, 0
        )).GetComponent(type);
        return bullet;
    }
    #endregion

}
