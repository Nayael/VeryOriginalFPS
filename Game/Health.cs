using UnityEngine;

public class Health : MonoBehaviour
{

    #region Private Members
    private int current;
    #endregion

    #region Public Members
    public int baseHP = 100;
    #endregion

    #region Properties
    public int Current {
        get { return current; }
        set {
            if (value < 0) {
                value = 0;
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

}
