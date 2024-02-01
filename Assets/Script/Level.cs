using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "TilePrefabRepository", menuName = "Gameplay/Level")]
public class Level : ScriptableObject
{
    public int boardWidth;
    public int boardHeight;
    public List<int> colorsUsed;
    public int movesLimit;
    public List<DestroyTilesObjective> destroyTilesObjectives;
}

[Serializable]
public class DestroyTilesObjective
{
    public int tileType;
    public int quantity;
}