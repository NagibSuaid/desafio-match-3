using System;
using UnityEngine;
using UnityEngine.UI;

public class TileView : MonoBehaviour
{
    public Image img;
    private void Awake() {
        img = GetComponentInChildren<Image>();
    }
}