using DG.Tweening;
using System;
using UnityEngine;
using UnityEngine.UI;

public class TileSpotView : MonoBehaviour
{
    [SerializeField] private Button _button;

    private int _x;
    private int _y;

    public event Action<int, int> OnClick;

    private void Awake()
    {
        _button.onClick.AddListener(OnTileClick);
    }

    private void OnTileClick()
    {
        OnClick?.Invoke(_x, _y);
    }

    public void SetPosition(int x, int y)
    {
        _x = x;
        _y = y;
    }

    public void SetTile(TileView tile)
    {
        tile.transform.SetParent(transform, false);
        tile.transform.position = transform.position;
    }

    public Tween AnimatedSetTile(TileView tile)
    {
        tile.transform.SetParent(transform);
        tile.transform.DOKill();
        tile.transform.DOScale(1f, .3f);
        return tile.transform.DOMove(transform.position, 0.3f);
    }

    Sequence s;
    public void SetHighlighted(TileView tile, bool value = true)
    {
        if (!value)
        {
            s.Kill();
            s = null;
            tile.transform.DOScale(1f, .3f);
            return;
        }
        s = DOTween.Sequence();
        s.Append(tile.transform.DOScale(1.3f, .5f));
        s.Append(tile.transform.DOScale(1f, .5f));
        s.InsertCallback(1f, () => s.Restart());
    }

}