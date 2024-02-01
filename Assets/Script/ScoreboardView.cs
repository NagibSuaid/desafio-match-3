using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;


public class ScoreboardView : MonoBehaviour
{
    [SerializeField] Text pointsTextField;
    public Tween UpdateScore(int pointsGained, int newTotal)
    {
        Sequence seq = DOTween.Sequence();
        seq.Insert(0f, pointsTextField.transform.DOScale(1 + pointsGained / 200f, .1f));
        seq.Insert(.1f, pointsTextField.transform.DOScale(1, .2f));
        seq.Insert(0f, pointsTextField.DOText(newTotal.ToString(), .3f, true, ScrambleMode.Numerals));
        return seq;
    }

}
