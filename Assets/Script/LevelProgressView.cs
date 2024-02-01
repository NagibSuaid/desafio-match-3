using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class LevelProgressView : MonoBehaviour
{

    [SerializeField] public Text movesText;
    [SerializeField] public List<TilesDestroyedView> tilesDestroyedUIs;
    public void OnProgressChange(LevelProgress progress)
    {
        foreach (var td in tilesDestroyedUIs)
        {
            td.gameObject.SetActive(false);
        }
        foreach (var (type, current, total) in progress.tilesProgress)
        {
            tilesDestroyedUIs[type].gameObject.SetActive(true);
            tilesDestroyedUIs[type].text.DOText($"{current}/{total}", .2f);
        }
        movesText.DOText($"{progress.movesLeft}", .2f);
    }
}