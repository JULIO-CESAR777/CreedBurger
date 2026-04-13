using UnityEngine;

public class ClientOrderIndicator : MonoBehaviour
{
    [Header("Refs")]
    public SpriteRenderer iconRenderer;
    public GameObject marcoComida;

    [Header("Seguimiento")]
    public Transform target;
    public Vector3 offset = new Vector3(0f, 2.2f, 0f);

    [Header("Look")]
    public bool lookAtWorldTarget = true;
    public Transform worldTarget;

    [Header("Opcional si lo buscas por tag")]
    public string worldTargetTag = "OrderLookTarget";

    void Awake()
    {
        if (iconRenderer == null)
            iconRenderer = GetComponentInChildren<SpriteRenderer>(true);

        // Tomar al cliente como target
        if (target == null)
        {
            MoveClient mc = GetComponentInParent<MoveClient>();
            if (mc != null) target = mc.transform;
            else if (transform.parent != null) target = transform.parent;
        }

        // Buscar target del mundo si no está asignado
        if (worldTarget == null && !string.IsNullOrEmpty(worldTargetTag))
        {
            GameObject go = GameObject.FindWithTag(worldTargetTag);
            if (go != null) worldTarget = go.transform;
        }

        // Despegarse del cliente
        transform.SetParent(null, true);

        Hide();
    }

    void LateUpdate()
    {
        if (target == null)
        {
            Destroy(gameObject);
            return;
        }

        // mover TODO el indicador
        transform.position = target.position + offset;

        if (lookAtWorldTarget && worldTarget != null)
        {
            Vector3 dir = worldTarget.position - transform.position;

            if (dir.sqrMagnitude > 0.0001f)
                transform.forward = dir.normalized;
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