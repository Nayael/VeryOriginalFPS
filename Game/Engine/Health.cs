using UnityEngine;

public class Health : MonoBehaviour
{

    #region Private Members
    private float current;
    #endregion

    #region Public Members
    public float baseHP = 100f;
    public float maxHP = 200f;
    #endregion

    #region Properties
    public float Current {
        get { return current; }
        set {
            if (value < 0f) {
                value = 0f;
            }
            if (value > maxHP) {
                value = maxHP;
            }
            current = value;
        }
    }
    #endregion

    #region Initialization
    void Awake() {
        current = baseHP;
    }
    #endregion

    #region Methods
    void Update() {
        if (current > baseHP) {
            current -= Time.deltaTime * 0.5f;
            if (current < baseHP) {
                current = baseHP;
            }
        }
    }

    public void Fill() {
        current = baseHP;
    }
    #endregion
}
