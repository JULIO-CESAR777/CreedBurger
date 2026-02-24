using TMPro;
using UnityEngine;

public class TutorialUi : MonoBehaviour
{
    [SerializeField] private GameObject panel;
    [SerializeField] private TMP_Text text;

    public void Show(string msg)
    {
        panel.SetActive(true);
        text.text = msg;
    }

    public void Hide()
    {
        panel.SetActive(false);
    }
}