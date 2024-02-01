
using System.Collections.Generic;
using UnityEngine;

public class ExplosionInfo
{
    public ExplosionType type;
    public Vector2Int origin;
    public List<Vector2Int> explodedTiles;
    public enum ExplosionType
    {
        Line,
        Area,
        Color
    }
}