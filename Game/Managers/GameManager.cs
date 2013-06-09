using UnityEngine;
using System.Collections;

public class GameManager : MonoBehaviour
{

    #region Static Members
    public static Transform mainTransform;
	public static GameManager instance;
    #endregion

    #region Public Members
    public float scrollSpeed;
    #endregion

    #region Private Members
    private bool running;	// Is the game running ?
    #endregion

    #region Initialization
    void Awake() {
		instance = this;
	}

    void Start () {
		running = false;
		GameEventManager.GameStart += GameStart;
		GameEventManager.GameOver += GameOver;
	}
    #endregion

    #region Update
    void Update () {
        //if (!running && Input.GetKeyDown(KeyCode.Space)) {	// If the game is not running yet, and the player presses Space
        //    GameEventManager.TriggerGameStart();
        //} else if (running) {

        //}
	}
    #endregion

    #region Methods
    private void GameStart() {
		running = true;
	}

	private void GameOver() {
		running = false;
	}
    #endregion

}