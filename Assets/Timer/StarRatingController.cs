using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class StarRatingController : MonoBehaviour
{
    [Header("Referencias")]
    [SerializeField] private Image[] starImages;     // 3 Image (UI)
    [SerializeField] private Transform[] starScales; // normalmente los mismos objetos

    [Header("Sprites / Estilo")]
    [SerializeField] private Sprite spriteEmpty;
    [SerializeField] private Sprite spriteFull;

    [Tooltip("Color para NO ganadas (opacidad tenue).")]
    [SerializeField] private Color emptyColor = new Color(1f, 1f, 1f, 0.45f);

    [Tooltip("Color para ganadas (usa alpha=1).")]
    [SerializeField] private Color fullColor  = Color.white;

    [Header("Intro (Pop)")]
    [SerializeField] private bool useIntroPop = true;
    [SerializeField] private float introStartScale = 0.2f;

    [Header("Animación")]
    [SerializeField] private float introDelayBetweenStars = 0.25f;
    [SerializeField] private float popDuration = 0.45f;
    [SerializeField] private float punchScale = 0.25f;
    [SerializeField] private int punchVibrato = 10;
    [SerializeField] private float punchElasticity = 0.9f;
    [SerializeField] private Ease popEase = Ease.OutBack;

    [Header("FX opcional")]
    [SerializeField] private GameObject[] starShines; // uno por estrella (puede estar vacío)

    [Header("Lógica de Puntuación")]
    [SerializeField] private int[] thresholds = new int[] { 2, 5, 10 };

    public System.Action<int> OnStarEarned; // opcional

    private Sequence seq;

    void OnDisable() => seq?.Kill();

    public void PlayStars(int score)
    {
        int earned = ScoreToStars(score);

        // Estado inicial visible
        for (int i = 0; i < starImages.Length; i++)
        {
            var img = starImages[i];
            if (!img) continue;

            img.type = Image.Type.Simple;
            img.preserveAspect = true;
            img.enabled = true;

            // Comienza como "vacía"
            if (spriteEmpty) img.sprite = spriteEmpty;

            var c = emptyColor;
            if (c.a < 0.2f) c.a = 0.45f; // asegúrate de verla
            img.color = c;

            if (starScales != null && i < starScales.Length && starScales[i])
                starScales[i].localScale = Vector3.one * (useIntroPop ? introStartScale : 1f);

            if (starShines != null && i < starShines.Length && starShines[i] != null)
                starShines[i].SetActive(false);
        }

        seq?.Kill();
        seq = DOTween.Sequence().SetUpdate(true); // unscaled time, animará en pausa

        for (int i = 0; i < starImages.Length; i++)
        {
            int idx = i;

            seq.AppendInterval(idx * introDelayBetweenStars);
            seq.AppendCallback(() =>
            {
                // Intro pop (sube a 1)
                if (useIntroPop && starScales != null && idx < starScales.Length && starScales[idx])
                    starScales[idx].DOScale(1f, popDuration).SetEase(popEase).SetUpdate(true);

                // Si está ganada: sprite lleno + color full + punch
                if (idx < earned)
                {
                    var img = starImages[idx];
                    if (img)
                    {
                        if (spriteFull) img.sprite = spriteFull;
                        var fc = fullColor; if (fc.a < 0.9f) fc.a = 1f; // opacidad a tope
                        img.DOColor(fc, 0.15f).SetUpdate(true);
                    }

                    if (starScales != null && idx < starScales.Length && starScales[idx])
                        starScales[idx].DOPunchScale(Vector3.one * punchScale, popDuration * 0.8f, punchVibrato, punchElasticity).SetUpdate(true);

                    if (starShines != null && idx < starShines.Length && starShines[idx] != null)
                    {
                        starShines[idx].SetActive(true);
                        DOVirtual.DelayedCall(1.2f, () =>
                        {
                            if (starShines[idx]) starShines[idx].SetActive(false);
                        }, ignoreTimeScale: true);
                    }

                    OnStarEarned?.Invoke(idx);
                }
            });
        }
    }

    public int ScoreToStars(int score)
    {
        int r = 0;
        for (int i = 0; i < 3 && i < thresholds.Length; i++)
        {
            if (score >= thresholds[i]) r++; else break;
        }
        return Mathf.Clamp(r, 0, 3);
    }
}
