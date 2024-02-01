using UnityEngine;


public class TileMatchInfo
{
    public Vector2Int origin;
    public int tilesLeft;
    public int tilesRight;
    public int tilesUp;
    public int tilesDown;
    public int TotalTiles
    {
        get => 1 + tilesLeft + tilesRight + tilesUp + tilesDown;
    }
    public int HorizontalTiles
    {
        get => 1 + tilesLeft + tilesRight;
    }
    public int VerticalTiles
    {
        get => 1 + tilesDown + tilesUp;
    }

    public override string ToString()
    {
        return $"{origin} : {tilesLeft}-{tilesRight}-{tilesUp}-{tilesDown}";
    }


    public override bool Equals(object obj)
    {

        if (obj == null || GetType() != obj.GetType())
        {
            return false;
        }

        TileMatchInfo other = (TileMatchInfo)obj;
        return origin == other.origin &&
        tilesLeft == other.tilesLeft &&
        tilesRight == other.tilesRight &&
        tilesUp == other.tilesUp &&
        tilesDown == other.tilesDown;
    }

    public override int GetHashCode()
    {
        return origin.GetHashCode();
    }
}