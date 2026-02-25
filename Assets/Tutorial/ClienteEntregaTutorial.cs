using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(BoxCollider))]
public class ClienteEntregaTutorial : MonoBehaviour
{
    [Header("Cliente dueño del trigger")]
    public MoveClientTutorial client;

    [Header("Opciones")]
    public bool requireInteractKey = false;
    public KeyCode interactKey = KeyCode.E;
    public bool destroyIngredientOnSuccess = true;

    private readonly HashSet<Object> attempted = new HashSet<Object>();

    void Start()
    {
        if (!client)
            client = GetComponentInParent<MoveClientTutorial>();

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
        attempted.Remove(other);

        var ing = other.GetComponentInParent<Ingredient>();
        if (ing) attempted.Remove(ing);

        var cook = other.GetComponentInParent<CookIngredients>();
        if (cook) attempted.Remove(cook);
    }

    private void TryDeliver(Collider other)
    {
        if (!client) return;

        // Buscar Ingredient
        Ingredient ing = other.GetComponentInParent<Ingredient>();
        if (!ing) ing = other.GetComponentInChildren<Ingredient>(true);

        // O CookIngredients
        CookIngredients cook = null;
        if (!ing)
        {
            cook = other.GetComponentInParent<CookIngredients>();
            if (!cook) cook = other.GetComponentInChildren<CookIngredients>(true);
        }

        if (!ing && !cook) return;

        Object key = (Object)ing ?? (Object)cook;
        if (!attempted.Add(key)) return;

        int deliveredId = ing ? ing.id : cook.currentComboID;

        Debug.Log($"[TutorialEntrega] Intentando entregar ID={deliveredId} a {client.name}");

        // Guardamos el pedido correcto antes de llamar
        int expectedId = client.GetPedidoId();

        client.RecibirPedido(deliveredId);

        // ✅ Solo si fue correcto
        if (deliveredId == expectedId)
        {
            ScoreSystem.Instance?.AwardDeliverySuccess();

            if (destroyIngredientOnSuccess)
            {
                if (ing) Destroy(ing.gameObject);
                else if (cook) Destroy(cook.gameObject);
            }
        }
        else
        {
            Debug.Log("[TutorialEntrega] Pedido incorrecto.");
        }
    }
}