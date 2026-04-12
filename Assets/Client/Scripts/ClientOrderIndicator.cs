using UnityEngine;

public class ClientOrderIndicator : MonoBehaviour
{
    [Header("Refs")]
    public SpriteRenderer iconRenderer;
    public Vector3 offset = new Vector3(0f, 2.2f, 0f);

    public bool lookAtWorldTarget = true;
    public Transform worldTarget;

    public GameObject marcoComida;

    void Awake()
    {
        if (iconRenderer == null)
            iconRenderer = GetComponentInChildren<SpriteRenderer>(true);

        Hide();
    }

    void LateUpdate()
    {
        if (iconRenderer != null)
            iconRenderer.transform.position = transform.position + offset;

        if (lookAtWorldTarget && worldTarget != null && iconRenderer != null)
        {
            Vector3 dir = worldTarget.position - iconRenderer.transform.position;

            if (dir.sqrMagnitude > 0.0001f)
            {
                if (marcoComida != null)
                    marcoComida.transform.forward = dir.normalized;

                iconRenderer.transform.forward = dir.normalized;
            }
        }
    }

    public void Show(Sprite sprite)
    {
        if (iconRenderer == null) return;

        iconRenderer.sprite = sprite;
        iconRenderer.enabled = (sprite != null);

        if (marcoComida != null)
            marcoComida.SetActive(sprite != null);
    }

    public void Hide()
    {
        if (iconRenderer == null) return;

        iconRenderer.sprite = null;
        iconRenderer.enabled = false;

        if (marcoComida != null)
            marcoComida.SetActive(false);
    }
}