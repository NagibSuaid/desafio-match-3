using System.Collections.Generic;
using DG.Tweening.Plugins.Core;
using UnityEngine;

public class GameController
{
    private List<List<Tile>> _boardTiles;
    private List<int> _tilesTypes;
    private int _tileCount;
    public int points;

    public List<List<Tile>> StartGame(int boardWidth, int boardHeight, List<int> tileTypes)
    {
        _tilesTypes = new List<int>(tileTypes);
        _boardTiles = CreateBoard(boardWidth, boardHeight, _tilesTypes);
        points = 0;
        return _boardTiles;
    }

    public bool IsValidMovement(int fromX, int fromY, int toX, int toY)
    {
        List<List<Tile>> newBoard = CopyBoard(_boardTiles);

        (newBoard[toY][toX], newBoard[fromY][fromX]) = (newBoard[fromY][fromX], newBoard[toY][toX]);
        for (int y = 0; y < newBoard.Count; y++)
        {
            for (int x = 0; x < newBoard[y].Count; x++)
            {
                if (x > 1
                    && newBoard[y][x].type == newBoard[y][x - 1].type
                    && newBoard[y][x - 1].type == newBoard[y][x - 2].type)
                {
                    return true;
                }
                if (y > 1
                    && newBoard[y][x].type == newBoard[y - 1][x].type
                    && newBoard[y - 1][x].type == newBoard[y - 2][x].type)
                {
                    return true;
                }
            }
        }
        return false;
    }

    Dictionary<int, int> tilesDestroyedPerType = new Dictionary<int, int>();
    public List<BoardSequence> SwapTile(int fromX, int fromY, int toX, int toY)
    {
        List<List<Tile>> newBoard = CopyBoard(_boardTiles);

        (newBoard[toY][toX], newBoard[fromY][fromX]) = (newBoard[fromY][fromX], newBoard[toY][toX]);
        List<BoardSequence> boardSequences = new List<BoardSequence>();
        List<TileMatchInfo> matchedTiles;
        while (HasMatch(matchedTiles = FindMatches(newBoard)))
        {


            //Cleaning the matched tiles
            List<Vector2Int> matchedPosition = ClearMatches(newBoard, matchedTiles, boardSequences,
            out List<ExplosionInfo> explodedTiles, out int pointsGained);
            points += pointsGained;
            // Dropping the tiles
            List<MovedTileInfo> movedTiles = DropTiles(newBoard);

            // Filling the board
            List<AddedTileInfo> addedTiles = FillBoard(newBoard);

            BoardSequence sequence = new BoardSequence
            {
                matchedPosition = matchedPosition,
                explosions = explodedTiles,
                movedTiles = movedTiles,
                addedTiles = addedTiles,
                pointsGained = pointsGained,
                newPointTotal = points,
                tilesDestroyedPerType = tilesDestroyedPerType
            };
            boardSequences.Add(sequence);
            tilesDestroyedPerType = new Dictionary<int, int>();
        }

        _boardTiles = newBoard;
        return boardSequences;
    }

    private static float GetMultiplier(List<BoardSequence> boardSequences)
    {
        return 1f + boardSequences.Count / 2f;
    }


    public static int ScoreExplosions(ExplosionInfo explosions, float multiplier)
    {
        return Mathf.CeilToInt(explosions.explodedTiles.Count * 10 * multiplier);
    }

    void DestroyTile(List<List<Tile>> board, int y, int x)
    {
        Tile tile = board[y][x];
        if (!tilesDestroyedPerType.TryGetValue(tile.type, out int oldValue))
        {
            oldValue = 0;
        }
        tilesDestroyedPerType[tile.type] = oldValue + 1;
        board[y][x] = new Tile { id = -1, type = -1 };
    }

    private List<Vector2Int> ClearMatches(
        List<List<Tile>> newBoard,
        List<TileMatchInfo> matchedTiles,
        List<BoardSequence> boardSequences,
        out List<ExplosionInfo> explosions,
        out int pointsGained
        )
    {
        explosions = new List<ExplosionInfo>();
        List<Vector2Int> matchedPosition = new List<Vector2Int>();
        pointsGained = 0;
        float multiplier = GetMultiplier(boardSequences);
        foreach (var match in matchedTiles)
        {
            int x = match.origin.x;
            int y = match.origin.y;
            int type = newBoard[y][x].type;

            for (int i = x - match.tilesLeft; i <= x + match.tilesRight; i++)
            {
                if (newBoard[y][i].type == -1) { continue; }
                DestroyTile(newBoard, y, i);
                matchedPosition.Add(new Vector2Int(i, y));
                pointsGained += Mathf.CeilToInt(10 * multiplier);
            }
            for (int j = y - match.tilesUp; j <= y + match.tilesDown; j++)
            {
                if (newBoard[j][x].type == -1) { continue; }
                DestroyTile(newBoard, j, x);
                matchedPosition.Add(new Vector2Int(x, j));
                pointsGained += Mathf.CeilToInt(10 * multiplier);
            }
            var newExplosion = ExplodeTiles(newBoard, multiplier, match, type, out int pointsGainedInExplosion);
            if (newExplosion.explodedTiles.Count > 0) { explosions.Add(newExplosion); }
            pointsGained += pointsGainedInExplosion;
        }
        return matchedPosition;
    }

    private ExplosionInfo ExplodeTiles(List<List<Tile>> newBoard,
      float multiplier, TileMatchInfo match, int type, out int pointsGained)
    {
        pointsGained = 0;
        ExplosionInfo explosion = new ExplosionInfo
        {
            origin = match.origin,
            explodedTiles = new List<Vector2Int>()
        };

        if (match.TotalTiles >= 5)
        {
            if ((match.tilesUp == 0 && match.tilesDown == 0) || (match.tilesLeft == 0 && match.tilesRight == 0))
            {
                explosion.explodedTiles = ExplodeColor(newBoard, type);
                explosion.type = ExplosionInfo.ExplosionType.Color;
            }
            else
            {
                explosion.explodedTiles = ExplodeArea(newBoard, match.origin);
                explosion.type = ExplosionInfo.ExplosionType.Area;
            }
        }
        else if (match.TotalTiles >= 4)
        {
            explosion.type = ExplosionInfo.ExplosionType.Line;

            if (match.tilesUp + match.tilesDown > match.tilesLeft + match.tilesRight)
            {
                explosion.explodedTiles = ExplodeColumn(newBoard, match.origin);
            }
            else
            {
                explosion.explodedTiles = ExplodeLine(newBoard, match.origin);
            }
        }
        pointsGained += ScoreExplosions(explosion, multiplier);
        return explosion;
    }

    public List<Vector2Int> ExplodeColor(List<List<Tile>> newBoard, int color)
    {
        List<Vector2Int> explodedTiles = new List<Vector2Int>();
        if (color == -1) return explodedTiles;
        for (int y = 0; y < newBoard.Count; y++)
        {
            for (int x = 0; x < newBoard[y].Count; x++)
            {
                if (newBoard[y][x].type == color)
                {
                    explodedTiles.Add(new Vector2Int(x, y));
                    DestroyTile(newBoard, y, x);
                }
            }
        }
        return explodedTiles;
    }
    public List<Vector2Int> ExplodeArea(List<List<Tile>> newBoard, Vector2Int center)
    {
        List<Vector2Int> explodedTiles = new List<Vector2Int>();
        int minY = Mathf.Max(center.y - 2, 0);
        int maxY = Mathf.Min(center.y + 2, newBoard.Count - 1);
        for (int y = minY; y <= maxY; y++)
        {
            int minX = Mathf.Max(center.x - 2, 0);
            int maxX = Mathf.Min(center.x + 2, newBoard[y].Count - 1);
            for (int x = minX; x <= maxX; x++)
            {
                if (newBoard[y][x].type == -1) { continue; }
                explodedTiles.Add(new Vector2Int(x, y));
                DestroyTile(newBoard, y, x);
            }
        }
        return explodedTiles;
    }
    public List<Vector2Int> ExplodeColumn(List<List<Tile>> newBoard, Vector2Int center)
    {
        List<Vector2Int> explodedTiles = new List<Vector2Int>();
        for (int y = 0; y < newBoard.Count; y++)
        {
            if (newBoard[y][center.x].type == -1) { continue; }
            explodedTiles.Add(new Vector2Int(center.x, y));
            DestroyTile(newBoard, y, center.x);
        }
        return explodedTiles;
    }
    public List<Vector2Int> ExplodeLine(List<List<Tile>> newBoard, Vector2Int center)
    {
        List<Vector2Int> explodedTiles = new List<Vector2Int>();
        for (int x = 0; x < newBoard[center.y].Count; x++)
        {
            if (newBoard[center.y][x].type == -1) { continue; }
            explodedTiles.Add(new Vector2Int(x, center.y));
            DestroyTile(newBoard, center.y, x);
        }
        return explodedTiles;
    }


    public static List<MovedTileInfo> DropTiles(List<List<Tile>> newBoard)
    {
        List<MovedTileInfo> movedTilesList = new List<MovedTileInfo>();

        for (int i = 0; i < newBoard[0].Count; i++)
        {
            List<int> emptySlots = new List<int>();
            for (int j = newBoard.Count - 1; j >= 0; j--)
            {
                if (newBoard[j][i].type == -1)
                {
                    emptySlots.Add(j);
                }
            }
            if (emptySlots.Count == 0) { continue; }
            for (int j = emptySlots[0]; j >= 0; j--)
            {
                if (newBoard[j][i].type != -1)
                {
                    movedTilesList.Add(new MovedTileInfo
                    {
                        from = new Vector2Int(i, j),
                        to = new Vector2Int(i, emptySlots[0])
                    });
                    newBoard[emptySlots[0]][i] = newBoard[j][i];
                    newBoard[j][i] = new Tile { id = -1, type = -1 };
                    emptySlots.RemoveAt(0);
                    emptySlots.Add(j);
                    emptySlots.Sort((a, b) => b.CompareTo(a));
                }

            }
        }
        return movedTilesList;
    }

    private List<AddedTileInfo> FillBoard(List<List<Tile>> newBoard)
    {
        List<AddedTileInfo> addedTiles = new List<AddedTileInfo>();
        for (int y = newBoard.Count - 1; y > -1; y--)
        {
            for (int x = newBoard[y].Count - 1; x > -1; x--)
            {
                if (newBoard[y][x].type == -1)
                {
                    int tileType = Random.Range(0, _tilesTypes.Count);
                    Tile tile = newBoard[y][x];
                    tile.id = _tileCount++;
                    tile.type = _tilesTypes[tileType];
                    addedTiles.Add(new AddedTileInfo
                    {
                        position = new Vector2Int(x, y),
                        type = tile.type
                    });
                }
            }
        }
        return addedTiles;
    }

    private static bool HasMatch(List<TileMatchInfo> list)
    {
        return list.Count > 0;
    }

    public static List<TileMatchInfo> FindMatches(List<List<Tile>> newBoard)
    {
        List<TileMatchInfo> matches = new List<TileMatchInfo>();
        for (int y = 0; y < newBoard.Count; y++)
        {
            for (int x = 0; x < newBoard[y].Count; x++)
            {
                int type = newBoard[y][x].type;
                int hCount = 0;
                int leftmostBound = x - 1;
                while (leftmostBound >= 0 && newBoard[y][leftmostBound].type == type)
                {
                    hCount += 1;
                    leftmostBound -= 1;
                }
                int rightmostBound = x + 1;
                while (rightmostBound < newBoard[y].Count && newBoard[y][rightmostBound].type == type)
                {
                    hCount += 1;
                    rightmostBound += 1;
                }

                int vCount = 0;
                int lowerBound = y - 1;
                while (lowerBound >= 0 && newBoard[lowerBound][x].type == type)
                {
                    vCount += 1;
                    lowerBound -= 1;
                }
                int upperBound = y + 1;
                while (upperBound < newBoard.Count && newBoard[upperBound][x].type == type)
                {
                    vCount += 1;
                    upperBound += 1;
                }
                if (vCount < 2 && hCount < 2)
                { continue; }
                if (vCount < 2) { vCount = 0; lowerBound = y - 1; upperBound = y + 1; }
                if (hCount < 2) { hCount = 0; leftmostBound = x - 1; rightmostBound = x + 1; }
                int tilesDown = upperBound - y - 1;
                int tilesUp = y - lowerBound - 1;
                int tilesLeft = x - leftmostBound - 1;
                int tilesRight = rightmostBound - x - 1;
                matches.Add(new TileMatchInfo
                {
                    origin = new Vector2Int(x, y),
                    tilesUp = tilesUp,
                    tilesDown = tilesDown,
                    tilesLeft = tilesLeft,
                    tilesRight = tilesRight,

                });
            }
        }
        matches.Sort((a, b) =>
        {
            if (a.TotalTiles != b.TotalTiles)
            {
                return b.TotalTiles.CompareTo(a.TotalTiles);
            }
            int unbalanceA = Mathf.Abs(a.tilesLeft - a.tilesRight) + Mathf.Abs(a.tilesUp - a.tilesDown);
            int unbalanceB = Mathf.Abs(b.tilesLeft - b.tilesRight) + Mathf.Abs(b.tilesUp - b.tilesDown);
            return unbalanceA.CompareTo(unbalanceB);
        });
        return matches;
    }

    private static List<List<Tile>> CopyBoard(List<List<Tile>> boardToCopy)
    {
        List<List<Tile>> newBoard = new List<List<Tile>>(boardToCopy.Count);
        for (int y = 0; y < boardToCopy.Count; y++)
        {
            newBoard.Add(new List<Tile>(boardToCopy[y].Count));
            for (int x = 0; x < boardToCopy[y].Count; x++)
            {
                Tile tile = boardToCopy[y][x];
                newBoard[y].Add(new Tile { id = tile.id, type = tile.type });
            }
        }

        return newBoard;
    }

    private List<List<Tile>> CreateBoard(int width, int height, List<int> tileTypes)
    {
        List<List<Tile>> board = new List<List<Tile>>(height);
        _tileCount = 0;
        for (int y = 0; y < height; y++)
        {
            board.Add(new List<Tile>(width));
            for (int x = 0; x < width; x++)
            {
                board[y].Add(new Tile { id = -1, type = -1 });
            }
        }

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                List<int> noMatchTypes = new List<int>(tileTypes.Count);
                for (int i = 0; i < tileTypes.Count; i++)
                {
                    noMatchTypes.Add(_tilesTypes[i]);
                }

                if (x > 1
                    && board[y][x - 1].type == board[y][x - 2].type)
                {
                    noMatchTypes.Remove(board[y][x - 1].type);
                }
                if (y > 1
                    && board[y - 1][x].type == board[y - 2][x].type)
                {
                    noMatchTypes.Remove(board[y - 1][x].type);
                }

                board[y][x].id = _tileCount++;
                board[y][x].type = noMatchTypes[Random.Range(0, noMatchTypes.Count)];
            }
        }
        return board;
    }
}
