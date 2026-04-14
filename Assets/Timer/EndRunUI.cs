using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using DG.Tweening;
using UnityEngine.UI;

public class EndRunUI : MonoBehaviour
{
    [Header("UI")]
    public GameObject panel;   // Desactivado por defecto
    public TMP_Text scoreText;
    public TMP_Text maxText;
    public TMP_Text coinsText;

    [Header("Stars")]
    [SerializeField] private StarRatingController starRating;

    CanvasGroup panelCg;
    RectTransform panelRt;

    void Awake()
    {
        // Autodescubre StarRating si no lo arrastraste
        if (starRating == null)
        {
            if (panel != null) starRating = panel.GetComponentInChildren<StarRatingController>(true);
            if (starRating == null) starRating = GetComponentInChildren<StarRatingController>(true);
        }

        // CanvasGroup para el fade
        if (panel != null)
        {
            panelRt = panel.GetComponent<RectTransform>();
            panelCg = panel.GetComponent<CanvasGroup>();
            if (!panelCg) panelCg = panel.AddComponent<CanvasGroup>();
        }
    }

    void Start()
    {
        if (panel) panel.SetActive(false);

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
        int score = s?.Score ?? 0;

        if (scoreText) scoreText.text = $"{score}";
        if (maxText)   maxText.text   = $"{s?.MaxScore ?? 0}";
        if (coinsText) coinsText.text = $"{s?.Coins ?? 0}";

        if (panel)
        {
            panel.SetActive(true);

            // POP-UP bonito del panel (unscaled time)
            panelCg.alpha = 0f;
            if (panelRt) panelRt.localScale = Vector3.one * 0.85f;

            DOTween.Kill(panelCg);
            DOTween.Kill(panelRt);

            panelCg.DOFade(1f, 0.25f).SetUpdate(true);
            if (panelRt) panelRt.DOScale(1f, 0.35f).SetEase(Ease.OutBack).SetUpdate(true);
        }

        // Estrellas (también usan unscaled time)
        if (starRating) starRating.PlayStars(score);

        // Pausa después de disparar tweens (usan SetUpdate(true), así que animan en pausa)
        Time.timeScale = 0f;
    }

    public void OnClickRetry()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        if (GameTimer.Instance != null) GameTimer.Instance.ResetTimer();
    }
    
    public void OnClickMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(0);  
        if (GameTimer.Instance != null) GameTimer.Instance.ResetTimer();
    }
}
