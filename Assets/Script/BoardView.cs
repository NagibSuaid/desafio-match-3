using DG.Tweening;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BoardView : MonoBehaviour
{
    [SerializeField] private TileSpotView tileSpotPrefab;

    [SerializeField] private TilePrefabRepository tilePrefabRepository;

    [SerializeField] private GridLayoutGroup boardContainer;

    private TileSpotView[][] _tileSpots;

    private TileView[][] _tiles;

    public event Action<int, int> OnTileClick;

    public void CreateBoard(List<List<Tile>> board)
    {
        boardContainer.constraintCount = board[0].Count;

        _tileSpots = new TileSpotView[board.Count][];
        _tiles = new TileView[board.Count][];

        for (int y = 0; y < board.Count; y++)
        {
            _tileSpots[y] = new TileSpotView[board[0].Count];
            _tiles[y] = new TileView[board[0].Count];

            for (int x = 0; x < board[0].Count; x++)
            {
                TileSpotView tileSpot = Instantiate(tileSpotPrefab);
                tileSpot.transform.SetParent(boardContainer.transform, false);
                tileSpot.SetPosition(x, y);
                tileSpot.OnClick += OnTileSpotClick;

                _tileSpots[y][x] = tileSpot;

                int tileTypeIndex = board[y][x].type;
                if (tileTypeIndex >= 0)
                {
                    TileView tilePrefab = tilePrefabRepository.tileTypePrefabList[tileTypeIndex];
                    TileView tile = Instantiate(tilePrefab);
                    tileSpot.SetTile(tile);

                    _tiles[y][x] = tile;
                }
            }
        }
    }

    public void DestroyBoard()
    {
        if (_tiles == null || _tileSpots == null) { return; }
        for (int y = 0; y < _tiles.Length; y++)
        {
            for (int x = 0; x < _tiles[y].Length; x++)
            {
                Destroy(_tiles[y][x].gameObject);
                Destroy(_tileSpots[y][x].gameObject);
            }
        }

        _tileSpots = null;
        _tiles = null;
    }

    public Tween SwapTiles(int fromX, int fromY, int toX, int toY)
    {
        Sequence swapSequence = DOTween.Sequence();
        swapSequence.Append(_tileSpots[fromY][fromX].AnimatedSetTile(_tiles[toY][toX]));
        swapSequence.Join(_tileSpots[toY][toX].AnimatedSetTile(_tiles[fromY][fromX]));

        (_tiles[toY][toX], _tiles[fromY][fromX]) = (_tiles[fromY][fromX], _tiles[toY][toX]);
        return swapSequence;
    }

    public Tween DestroyTiles(List<Vector2Int> matchedPosition)
    {
        Sequence seq = DOTween.Sequence();
        for (int i = 0; i < matchedPosition.Count; i++)
        {
            Vector2Int position = matchedPosition[i];
            var tile = _tiles[position.y][position.x];
            _tiles[position.y][position.x] = null;
            seq.Join(DOVirtual.DelayedCall(0.1f, () =>
            {
                tile.transform.DOScale(0, .1f).OnComplete(() => Destroy(tile.gameObject));
            }));

        }
        return seq;
    }

    public Tween ExplodeTiles(List<ExplosionInfo> explosions)
    {
        Sequence seq = DOTween.Sequence();
        foreach (var explosion in explosions)
        {
            seq.AppendInterval(.2f);
            Sequence subSeq = ExplosionTween(explosion);
            seq.Append(subSeq);
        }
        seq.AppendInterval(.2f);
        return seq;
    }

    private Sequence ExplosionTween(ExplosionInfo explosion)
    {
        Sequence subSeq = DOTween.Sequence();
        foreach (var position in explosion.explodedTiles)
        {
            var tile = _tiles[position.y][position.x];
            _tiles[position.y][position.x] = null;
            float t;
            switch (explosion.type)
            {
                case ExplosionInfo.ExplosionType.Color:
                    t = .15f * Vector2.Distance(position, explosion.origin);
                    subSeq.Insert(0f, DOTween.Shake(() => tile.transform.position, (x) => tile.transform.position = x, t + .15f));
                    subSeq.Insert(t, tile.img.DOFade(0, .15f));
                    subSeq.InsertCallback(t + .151f, () => Destroy(tile.gameObject));
                    break;
                case ExplosionInfo.ExplosionType.Area:
                    t = .15f * Vector2.Distance(position, explosion.origin);
                    subSeq.Insert(t, tile.transform.DOScale(0, .15f));
                    subSeq.InsertCallback(t + .151f, () => Destroy(tile.gameObject));
                    break;
                case ExplosionInfo.ExplosionType.Line:
                    Vector2 difference = position - explosion.origin;
                    t = .15f * difference.magnitude;
                    if (difference.y == 0)
                    {
                        subSeq.Insert(t, tile.transform.DOScaleY(0, .1f));
                    }
                    else
                    {
                        subSeq.Insert(t, tile.transform.DOScaleX(0, .1f));
                    }
                    subSeq.InsertCallback(t + .11f, () => Destroy(tile.gameObject));
                    break;
                default:
                    subSeq.Append(tile.transform.DOScale(0, .1f));
                    subSeq.AppendCallback(() => Destroy(tile.gameObject));
                    break;
            }


        }

        return subSeq;
    }

    public Tween MoveTiles(List<MovedTileInfo> movedTiles)
    {
        TileView[][] tiles = new TileView[_tiles.Length][];
        for (int y = 0; y < _tiles.Length; y++)
        {
            tiles[y] = new TileView[_tiles[y].Length];
            for (int x = 0; x < _tiles[y].Length; x++)
            {
                tiles[y][x] = _tiles[y][x];
            }
        }

        Sequence sequence = DOTween.Sequence();
        for (int i = 0; i < movedTiles.Count; i++)
        {
            MovedTileInfo movedTileInfo = movedTiles[i];

            Vector2Int from = movedTileInfo.from;
            Vector2Int to = movedTileInfo.to;

            sequence.Join(_tileSpots[to.y][to.x].AnimatedSetTile(_tiles[from.y][from.x]));

            tiles[to.y][to.x] = _tiles[from.y][from.x];
        }

        _tiles = tiles;
        return sequence;
    }

    public Tween CreateTile(List<AddedTileInfo> addedTiles)
    {
        Sequence seq = DOTween.Sequence();
        for (int i = 0; i < addedTiles.Count; i++)
        {
            AddedTileInfo addedTileInfo = addedTiles[i];
            Vector2Int position = addedTileInfo.position;

            TileSpotView tileSpot = _tileSpots[position.y][position.x];

            TileView tilePrefab = tilePrefabRepository.tileTypePrefabList[addedTileInfo.type];
            TileView tile = Instantiate(tilePrefab);
            tileSpot.SetTile(tile);

            _tiles[position.y][position.x] = tile;

            tile.transform.localScale = Vector2.zero;
            seq.Join(tile.transform.DOScale(1.0f, 0.2f));
        }

        return seq;
    }

    private void OnTileSpotClick(int x, int y)
    {
        OnTileClick?.Invoke(x, y);
    }

    public void HighlightTile(int x, int y)
    {
        _tileSpots[y][x].SetHighlighted(_tiles[y][x], true);
    }

    public void ClearHighlightedTiles(int x, int y)
    {
        _tileSpots[y][x].SetHighlighted(_tiles[y][x], false);
    }

}