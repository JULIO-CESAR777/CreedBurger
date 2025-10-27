using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class EndRunUI : MonoBehaviour
{
    [Header("UI")]
    public GameObject panel;   // Desactivado por defecto
    public TMP_Text scoreText;
    public TMP_Text maxText;
    public TMP_Text coinsText;

    void Start()
    {
        if (panel) panel.SetActive(false);

        // Suscribirse al final del timer (si lo usas)
        if (GameTimer.Instance != null)
            GameTimer.Instance.OnTimesUp += ShowResults;
    }

    void OnDestroy()
    {
        if (GameTimer.Instance != null)
            GameTimer.Instance.OnTimesUp -= ShowResults;
    }

    public void ShowResults()
    {
        var s = ScoreSystem.Instance;

        if (scoreText) scoreText.text = $"Score: {s?.Score ?? 0}";
        if (maxText)   maxText.text   = $"Max: {s?.MaxScore ?? 0}";
        if (coinsText) coinsText.text = $"Coins: {s?.Coins ?? 0}";

        if (panel) panel.SetActive(true);
        Time.timeScale = 0f; // Congela la partida
    }

    // Botón "Reintentar"
    public void OnClickRetry()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        GameTimer.Instance.ResetTimer();
    }
}