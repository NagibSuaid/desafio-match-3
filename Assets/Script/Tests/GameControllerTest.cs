using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

public class GameControllerTest
{
    private readonly Tile emptyTile = new Tile { id = -1, type = -1 };
    static private Tile mockTile(int type)
    {
        return new Tile { id = -1, type = type };
    }
    [Test]
    public void DropTilesPasses()
    {
        List<List<Tile>> board = new List<List<Tile>>
        {
            new List<Tile>{mockTile(1),mockTile(2),emptyTile  ,emptyTile  },
            new List<Tile>{emptyTile  ,emptyTile  ,mockTile(4),emptyTile  },
            new List<Tile>{emptyTile  ,mockTile(3),emptyTile  ,emptyTile  },
            new List<Tile>{emptyTile  ,emptyTile  ,mockTile(5),mockTile(6)},
        };
        List<MovedTileInfo> movedTiles = GameController.DropTiles(board);

        Assert.IsTrue(movedTiles.Count == 4, $"Count was: {movedTiles.Count}");
        Assert.IsTrue(movedTiles.Contains(new MovedTileInfo { from = new Vector2Int(0, 0), to = new Vector2Int(0, 3) }));
        Assert.IsTrue(movedTiles.Contains(new MovedTileInfo { from = new Vector2Int(1, 0), to = new Vector2Int(1, 2) }));
        Assert.IsTrue(movedTiles.Contains(new MovedTileInfo { from = new Vector2Int(1, 2), to = new Vector2Int(1, 3) }));
        Assert.IsTrue(movedTiles.Contains(new MovedTileInfo { from = new Vector2Int(2, 1), to = new Vector2Int(2, 2) }));
    }

    [Test]
    public void V3MatchPasses()
    {
        List<List<Tile>> board;
        List<TileMatchInfo> matches;
        board = new List<List<Tile>>
         {
             new List<Tile>{mockTile(2),mockTile(1),mockTile(3)},
             new List<Tile>{mockTile(3),mockTile(1),mockTile(1)},
             new List<Tile>{mockTile(2),mockTile(1),mockTile(3)}
         };
        matches = GameController.FindMatches(board);
        Assert.IsTrue(matches.Count == 3);
        Assert.Contains(new TileMatchInfo { origin = new Vector2Int(1, 0), tilesDown = 2, tilesUp = 0, tilesLeft = 0, tilesRight = 0 }, matches);
        Assert.Contains(new TileMatchInfo { origin = new Vector2Int(1, 1), tilesDown = 1, tilesUp = 1, tilesLeft = 0, tilesRight = 0 }, matches);
        Assert.Contains(new TileMatchInfo { origin = new Vector2Int(1, 2), tilesDown = 0, tilesUp = 2, tilesLeft = 0, tilesRight = 0 }, matches);
    }

    [Test]
    public void H3MatchPasses()
    {
        List<List<Tile>> board;
        List<TileMatchInfo> matches;
        board = new List<List<Tile>>
         {
             new List<Tile>{mockTile(2),mockTile(1),mockTile(4)},
             new List<Tile>{mockTile(1),mockTile(1),mockTile(1)},
             new List<Tile>{mockTile(2),mockTile(3),mockTile(4)}
         };
        matches = GameController.FindMatches(board);
        Assert.IsTrue(matches.Count == 3);
        Assert.Contains(new TileMatchInfo { origin = new Vector2Int(0, 1), tilesDown = 0, tilesUp = 0, tilesLeft = 0, tilesRight = 2 }, matches);
        Assert.Contains(new TileMatchInfo { origin = new Vector2Int(1, 1), tilesDown = 0, tilesUp = 0, tilesLeft = 1, tilesRight = 1 }, matches);
        Assert.Contains(new TileMatchInfo { origin = new Vector2Int(2, 1), tilesDown = 0, tilesUp = 0, tilesLeft = 2, tilesRight = 0 }, matches);
    }

    [Test]
    public void Cross3MatchPasses()
    {
        List<List<Tile>> board;
        List<TileMatchInfo> matches;
        board = new List<List<Tile>>
         {
             new List<Tile>{mockTile(2),mockTile(1),mockTile(3)},
             new List<Tile>{mockTile(1),mockTile(1),mockTile(1)},
             new List<Tile>{mockTile(2),mockTile(1),mockTile(3)}
         };
        matches = GameController.FindMatches(board);
        Assert.IsTrue(matches.Count == 5);
        Assert.Contains(new TileMatchInfo { origin = new Vector2Int(1, 0), tilesDown = 2, tilesUp = 0, tilesLeft = 0, tilesRight = 0 }, matches);
        Assert.Contains(new TileMatchInfo { origin = new Vector2Int(1, 1), tilesDown = 1, tilesUp = 1, tilesLeft = 1, tilesRight = 1 }, matches);
        Assert.Contains(new TileMatchInfo { origin = new Vector2Int(1, 2), tilesDown = 0, tilesUp = 2, tilesLeft = 0, tilesRight = 0 }, matches);
        Assert.Contains(new TileMatchInfo { origin = new Vector2Int(0, 1), tilesDown = 0, tilesUp = 0, tilesLeft = 0, tilesRight = 2 }, matches);
        Assert.Contains(new TileMatchInfo { origin = new Vector2Int(2, 1), tilesDown = 0, tilesUp = 0, tilesLeft = 2, tilesRight = 0 }, matches);
    }
    [Test]
    public void SquareMatchPasses()
    {
        List<List<Tile>> board;
        List<TileMatchInfo> matches;
        board = new List<List<Tile>>
         {
             new List<Tile>{mockTile(2),mockTile(1),mockTile(1)},
             new List<Tile>{mockTile(3),mockTile(1),mockTile(1)},
             new List<Tile>{mockTile(1),mockTile(3),mockTile(3)}
         };
        matches = GameController.FindMatches(board);
        Assert.IsTrue(matches.Count == 0);
    }

    [Test]
    public void V4MatchPasses()
    {
        List<List<Tile>> board;
        List<TileMatchInfo> matches;

        board = new List<List<Tile>>
         {
             new List<Tile>{mockTile(2),mockTile(1),mockTile(3),mockTile(3)},
             new List<Tile>{mockTile(2),mockTile(1),mockTile(1),mockTile(3)},
             new List<Tile>{mockTile(2),mockTile(1),mockTile(3),mockTile(1)},
             new List<Tile>{mockTile(3),mockTile(1),mockTile(3),mockTile(1)}
         };
        matches = GameController.FindMatches(board);
        Assert.IsTrue(matches.Count == 7);
        Assert.Contains(new TileMatchInfo { origin = new Vector2Int(0, 0), tilesDown = 2, tilesUp = 0, tilesLeft = 0, tilesRight = 0 }, matches);
        Assert.Contains(new TileMatchInfo { origin = new Vector2Int(0, 1), tilesDown = 1, tilesUp = 1, tilesLeft = 0, tilesRight = 0 }, matches);
        Assert.Contains(new TileMatchInfo { origin = new Vector2Int(0, 2), tilesDown = 0, tilesUp = 2, tilesLeft = 0, tilesRight = 0 }, matches);

        Assert.Contains(new TileMatchInfo { origin = new Vector2Int(1, 0), tilesDown = 3, tilesUp = 0, tilesLeft = 0, tilesRight = 0 }, matches);
        Assert.Contains(new TileMatchInfo { origin = new Vector2Int(1, 1), tilesDown = 2, tilesUp = 1, tilesLeft = 0, tilesRight = 0 }, matches);
        Assert.Contains(new TileMatchInfo { origin = new Vector2Int(1, 2), tilesDown = 1, tilesUp = 2, tilesLeft = 0, tilesRight = 0 }, matches);
        Assert.Contains(new TileMatchInfo { origin = new Vector2Int(1, 3), tilesDown = 0, tilesUp = 3, tilesLeft = 0, tilesRight = 0 }, matches);
    }
    [Test]
    public void H4MatchPasses()
    {
        List<List<Tile>> board;
        List<TileMatchInfo> matches;

        board = new List<List<Tile>>
         {
             new List<Tile>{mockTile(2),mockTile(1),mockTile(3),mockTile(3)},
             new List<Tile>{mockTile(1),mockTile(1),mockTile(1),mockTile(1)},
             new List<Tile>{mockTile(2),mockTile(2),mockTile(3),mockTile(3)},
             new List<Tile>{mockTile(3),mockTile(1),mockTile(3),mockTile(1)}
         };
        matches = GameController.FindMatches(board);
        Assert.IsTrue(matches.Count == 4);

        Assert.Contains(new TileMatchInfo { origin = new Vector2Int(0, 1), tilesDown = 0, tilesUp = 0, tilesLeft = 0, tilesRight = 3 }, matches);
        Assert.Contains(new TileMatchInfo { origin = new Vector2Int(1, 1), tilesDown = 0, tilesUp = 0, tilesLeft = 1, tilesRight = 2 }, matches);
        Assert.Contains(new TileMatchInfo { origin = new Vector2Int(2, 1), tilesDown = 0, tilesUp = 0, tilesLeft = 2, tilesRight = 1 }, matches);
        Assert.Contains(new TileMatchInfo { origin = new Vector2Int(3, 1), tilesDown = 0, tilesUp = 0, tilesLeft = 3, tilesRight = 0 }, matches);
    }
}
