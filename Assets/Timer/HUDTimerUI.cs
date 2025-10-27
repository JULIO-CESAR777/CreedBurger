using TMPro;
using UnityEngine;

public class HUDTimerUI : MonoBehaviour
{
    public TMP_Text timerText;

    void Start()
    {
        if (GameTimer.Instance != null)
            GameTimer.Instance.OnTick += Refresh;
        // pinta el valor inicial
        if (GameTimer.Instance != null) Refresh(GameTimer.Instance.Remaining);
    }

    void OnDestroy()
    {
        if (GameTimer.Instance != null)
            GameTimer.Instance.OnTick -= Refresh;
    }

    void Refresh(float remaining)
    {
        int total = Mathf.CeilToInt(remaining);
        int mm = total / 60;
        int ss = total % 60;
        timerText.text = $"{mm:00}:{ss:00}";
    }
}