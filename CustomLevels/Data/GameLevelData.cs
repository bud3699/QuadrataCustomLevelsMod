using System;
using UnityEngine;

[Serializable]
public class SerializableVector2
{
    public float x;
    public float y;

    public SerializableVector2() { }

    public SerializableVector2(Vector2 v)
    {
        x = v.x;
        y = v.y;
    }
}

[Serializable]
public class GameLevelData
{
    public Vector2[] characterPositions;
    public string entityData;
    public int moveCount;
    public bool reversed;
}