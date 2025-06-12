using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Mineant.SaveSystem
{
    [System.Serializable]
    public class GameData
    {
        public long lastUpdated;

        public GameData()
        {
            lastUpdated = long.MinValue;
        }
    }
}