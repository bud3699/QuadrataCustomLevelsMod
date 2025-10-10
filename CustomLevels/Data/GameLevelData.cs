using System;
using UnityEngine;

namespace QuadrataPatcher
{
    [Serializable]
    public class GameLevelData
    {
        public Vector2[] characterPositions = new Vector2[2];
        public string entityData = "";
        public int moveCount = 5;
        public bool reversed;
    }
}
