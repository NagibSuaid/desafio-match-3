using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UI;

public class TilesDestroyedView : MonoBehaviour
{
    [SerializeField] public Image img;
    [SerializeField] public Text text;

    void Awake()
    {
        img = GetComponentInChildren<Image>();
        text = GetComponentInChildren<Text>();
    }
}