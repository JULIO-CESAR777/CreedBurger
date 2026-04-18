using System;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class TutorialUi : MonoBehaviour
{
    [SerializeField] private GameObject panel;
    [SerializeField] private TMP_Text text;
    [SerializeField] private Image iconImage; // 👈 nuevo
    
    [SerializeField] private GameObject pausePanel;

    public bool isPaused;

    private void Start()
    {
        GameManager.GetInstance().onChangeGameState += OnChangeGameStateCallback;
    }
    
    public void OnChangeGameStateCallback(GameState newState)
    {
        isPaused = (newState == GameState.Pause);
        ApplyPauseState();
    }

    public void ApplyPauseState()
    {
        if (GameManager.GetInstance().gameState == GameState.Pause)
        {
            pausePanel.SetActive(true);
        }
        else
        {
            pausePanel.SetActive(false);
        }
    }

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