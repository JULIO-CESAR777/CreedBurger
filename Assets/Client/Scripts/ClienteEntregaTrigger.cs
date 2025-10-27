using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(BoxCollider))]
public class ClienteEntregaTrigger : MonoBehaviour
{
    [Header("Cliente dueño del trigger")]
    public MoveClient client;

    [Header("Opciones")]
    public bool requireInteractKey = false;
    public KeyCode interactKey = KeyCode.E;
    public bool destroyIngredientOnSuccess = true;

    // Evitar múltiples llamadas con el mismo objeto al permanecer dentro
    private readonly HashSet<Object> attempted = new HashSet<Object>();

    void Start()
    {
        if (!client) client = GetComponentInParent<MoveClient>();
        var col = GetComponent<BoxCollider>();
        col.isTrigger = true;
    }

    void OnTriggerEnter(Collider other)
    {
        if (requireInteractKey) return;
        TryDeliver(other);
    }

    void OnTriggerStay(Collider other)
    {
        if (!requireInteractKey) return;
        if (Input.GetKeyDown(interactKey))
            TryDeliver(other);
    }

    void OnTriggerExit(Collider other)
    {
        // Limpia “attempted” al salir para permitir reintentos con el mismo objeto
        attempted.Remove(other);
        var ing = other.GetComponentInParent<Ingredient>();
        if (ing) attempted.Remove(ing);
        var cook = other.GetComponentInParent<CookIngredients>();
        if (cook) attempted.Remove(cook);
    }

    private void TryDeliver(Collider other)
    {
        if (!client) return;

        if (client.estadoActual != MoveClient.Estado.EsperaPedido)
        {
            // Útil para ver por qué no avanza
            Debug.Log($"[EntregaTrigger] Cliente aún no está esperando pedido (estado={client.estadoActual}).");
            return;
        }

        // 1) ¿Trae un Ingredient en este collider, en su padre o en sus hijos?
        Ingredient ing = other.GetComponentInParent<Ingredient>();
        if (!ing) ing = other.GetComponentInChildren<Ingredient>(true);

        // 2) ¿O es un CookIngredients (combo)?
        CookIngredients cook = null;
        if (!ing)
        {
            cook = other.GetComponentInParent<CookIngredients>();
            if (!cook) cook = other.GetComponentInChildren<CookIngredients>(true);
        }

        if (!ing && !cook) return; // no trae nada “comible”

        // Evitar spam con el mismo objeto mientras está dentro
        Object key = (Object)ing ?? (Object)cook;
        if (!attempted.Add(key)) return;

        int deliveredId = ing ? ing.id : cook.currentComboID;
        Debug.Log($"[EntregaTrigger] Intentando entregar ID={deliveredId} al cliente {client.name}");

        var prevState = client.estadoActual;

        client.RecibirPedido(deliveredId);

        // Si cambió a Comer, fue correcto
        if (destroyIngredientOnSuccess && client.estadoActual == MoveClient.Estado.Comer)
        {
            
            ScoreSystem.Instance?.AwardDeliverySuccess();
            
            if (ing) Destroy(ing.gameObject);
            else if (cook) Destroy(cook.gameObject);
        }
        else
        {
            // Permite reintentos con el mismo objeto si fue incorrecto
            attempted.Remove(key);
            Debug.Log($"[EntregaTrigger] Pedido incorrecto. Esperado != {deliveredId}");
        }
    }
}
