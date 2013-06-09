using UnityEngine;

public class Health : MonoBehaviour
{

    #region Private Members
    private float current;
    #endregion

    #region Public Members
    public float baseHP = 100f;
    #endregion

    #region Properties
    public float Current {
        get { return current; }
        set {
            if (value < 0f) {
                value = 0f;
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
    public void Fill() {
        current = baseHP;
    }
    #endregion
}
