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
            //Debug.Log($"[EntregaTrigger] Cliente aún no está esperando pedido (estado={client.estadoActual}).");
            return;
        }

        Ingredient ing = other.GetComponentInParent<Ingredient>();
        if (!ing) ing = other.GetComponentInChildren<Ingredient>(true);

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
        //Debug.Log($"[EntregaTrigger] Intentando entregar ID={deliveredId} al cliente {client.name}");

        client.RecibirPedido(deliveredId);

        if (destroyIngredientOnSuccess && client.estadoActual == MoveClient.Estado.Comer)
        {
            ScoreSystem.Instance?.AwardDeliverySuccess();

            GameObject deliveredObject = null;

            if (ing) deliveredObject = ing.gameObject;
            else if (cook) deliveredObject = cook.gameObject;

            if (deliveredObject != null)
            {
                var grab = deliveredObject.GetComponent<GrabObject>();
                if (grab != null && grab.CurrentHolder != null)
                {
                    grab.CurrentHolder.ClearHeldObjectAfterDelivery(deliveredObject);
                    grab.CurrentHolder.BlockKillForSeconds(0.25f);
                }

                Destroy(deliveredObject);
            }
        }
        else
        {
            attempted.Remove(key);
            //Debug.Log($"[EntregaTrigger] Pedido incorrecto. Esperado != {deliveredId}");
        }
    }
}