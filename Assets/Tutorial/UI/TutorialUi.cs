using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class TutorialUi : MonoBehaviour
{
    [SerializeField] private GameObject panel;
    [SerializeField] private TMP_Text text;
    [SerializeField] private Image iconImage; // 👈 nuevo

    public void Show(string msg, Sprite icon = null)
    {
        panel.SetActive(true);
        text.text = msg;

        
        if (iconImage == null) return; 
        
        if (icon != null)
        {
            iconImage.gameObject.SetActive(true);
            iconImage.sprite = icon;
        }
        else
        {
            iconImage.gameObject.SetActive(false);
        }
    }

    public void Hide()
    {
        panel.SetActive(false);
    }
}