using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FPS.Utils
{
    public sealed class Singleton<T> where T : class, new()
    {
        /// <summary>
        /// Singleton implementation, readonly and static ensure thread safeness.
        /// </summary>
        public static readonly T Instance = new T();
    }
}
