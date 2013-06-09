using System.Collections.Generic;
using UnityEngine;

namespace FPS.Game.Managers
{
    /// <summary>
    /// Level Manager Singleton
    /// </summary>
    public sealed class LevelManager
    {
        #region Singleton Stuff
        private static readonly LevelManager instance = new LevelManager();

        private LevelManager() { }

        public static LevelManager Instance {
            get {
                return instance; 
            }
        }
        #endregion
        
    }
}
