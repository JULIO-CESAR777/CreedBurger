using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class OrderUIController : MonoBehaviour
{
    public static OrderUIController Instance;

    [Header("Panel donde aparecerán los pedidos")]
    public Transform contentPanel; // Panel dentro del Canvas (Vertical o Grid Layout)
    public GameObject orderIconPrefab; // Prefab con un Image

    private Dictionary<int, GameObject> activeOrders = new(); // idCliente -> icono

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    // Crear un icono nuevo en el panel
    public void AddOrder(int clientId, Sprite sprite)
    {
        if (sprite == null || contentPanel == null || orderIconPrefab == null)
        {
            Debug.LogWarning("[OrderUIController] Faltan referencias.");
            return;
        }

        // Si ya existe, actualizar sprite
        if (activeOrders.ContainsKey(clientId))
        {
            var existing = activeOrders[clientId].GetComponentInChildren<Image>();
            if (existing != null) existing.sprite = sprite;
            return;
        }

        GameObject iconGO = Instantiate(orderIconPrefab, contentPanel);
        Image img = iconGO.GetComponentInChildren<Image>();
        if (img != null)
        {
            img.sprite = sprite;
            img.preserveAspect = true;
        }

        activeOrders[clientId] = iconGO;
    }

    // Quitar el icono cuando el cliente termina
    public void RemoveOrder(int clientId)
    {
        if (activeOrders.TryGetValue(clientId, out var icon))
        {
            Destroy(icon);
            activeOrders.Remove(clientId);
        }
    }
}