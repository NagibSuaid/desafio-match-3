using UnityEngine;

public class MovedTileInfo
{
    public Vector2Int from;
    public Vector2Int to;

    // override object.Equals
    public override bool Equals(object obj)
    {
        if (obj == null || GetType() != obj.GetType())
        {
            return false;
        }

        MovedTileInfo other = (MovedTileInfo)obj;
        return other.from == from && other.to == to;
    }

    public override int GetHashCode()
    {
        return from.GetHashCode() + to.GetHashCode();
    }

}