using UnityEngine;
using System.Collections;

public static class GameEventManager
{

    #region Delegates
    public delegate void GameEvent();
    #endregion

    #region Static Members
    public static event GameEvent GameStart, GameOver;
    #endregion

    #region Methods
    public static void TriggerGameStart() {
		if (GameStart != null) {
			GameStart();
		}
	}

	public static void TriggerGameOver() {
		if (GameOver != null) {
			GameOver();
		}
    }
    #endregion

}
