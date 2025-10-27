using TMPro;
using UnityEngine;

public class HUDScoreUI : MonoBehaviour
{
    public TMP_Text coinsText;
    public TMP_Text scoreText;
    public TMP_Text maxScoreText;

    void Start()
    {
        if (ScoreSystem.Instance == null) return;
        ScoreSystem.Instance.OnScoreChanged += Refresh;
        Refresh();
    }

    void OnDestroy()
    {
        if (ScoreSystem.Instance != null)
            ScoreSystem.Instance.OnScoreChanged -= Refresh;
    }

    void Refresh()
    {
        var s = ScoreSystem.Instance;
        coinsText.text = $"Coins: {s.Coins}";
        scoreText.text = $"Score: {s.Score}";
        maxScoreText.text = $"Max: {s.MaxScore}";
    }
}