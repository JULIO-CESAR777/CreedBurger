using UnityEngine;

public class ClientOrderIndicator : MonoBehaviour
{
    [Header("Refs")]
    public SpriteRenderer iconRenderer; // asigna el SpriteRenderer del hijo
    public Vector3 offset = new Vector3(0f, 2.2f, 0f);

    [Header("Opcional: que mire a la cámara")]
    public bool billboardToCamera = true;

    private Transform _cam;

    void Awake()
    {
        if (iconRenderer == null)
            iconRenderer = GetComponentInChildren<SpriteRenderer>(true);

        _cam = Camera.main != null ? Camera.main.transform : null;

        Hide();
    }

    void LateUpdate()
    {
        // Mantenerlo arriba de la cabeza (por si el modelo se mueve)
        if (iconRenderer != null)
            iconRenderer.transform.position = transform.position + offset;

        if (billboardToCamera && _cam != null && iconRenderer != null)
        {
            // Que siempre mire a cámara
            iconRenderer.transform.forward = _cam.forward;
        }
    }

    public void Show(Sprite sprite)
    {
        if (iconRenderer == null) return;
        iconRenderer.sprite = sprite;
        iconRenderer.enabled = (sprite != null);
    }

    public void Hide()
    {
        if (iconRenderer == null) return;
        iconRenderer.sprite = null;
        iconRenderer.enabled = false;
    }
}