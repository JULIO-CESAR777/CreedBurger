using UnityEngine;
using UnityEngine.EventSystems;
using TMPro;
using DG.Tweening;

public class UIButtonHover : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    private Vector3 originalScale;
    private TMP_Text text;

    public float scale = 1.5f;
    public float duration = 0.2f;

    public Color normalColor = Color.black;
    public Color hoverColor = new Color(1f, 0.85f, 0.2f);

    void Start()
    {
        originalScale = transform.localScale;
        text = GetComponentInChildren<TMP_Text>();
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        transform.DOScale(originalScale * scale, duration).SetEase(Ease.OutBack);

        if (text != null)
            text.DOColor(hoverColor, duration);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        transform.DOScale(originalScale, duration).SetEase(Ease.OutQuad);

        if (text != null)
            text.DOColor(normalColor, duration);
    }
}
