using System;
using System.Collections;
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

    // Para evitar múltiples llamadas con el mismo objeto al permanecer dentro del trigger
    private readonly HashSet<Ingredient> attempted = new();


    private void Start()
    {
        client = GetComponentInParent<MoveClient>();    
    }

    void Reset()
    {
        // Auto-asignaciones útiles
        var col = GetComponent<BoxCollider>();
        col.isTrigger = true;

        if (!client) client = GetComponentInParent<MoveClient>();
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

    private void TryDeliver(Collider other)
    {
        if (!client) return;
        if (client.estadoActual != MoveClient.Estado.EsperaPedido) return;

        // El plato puede estar en el collider o en un padre (si lo trae el jugador/mesero en la mano)
        Ingredient ing = other.GetComponent<Ingredient>();
        if (!ing) return;

        // Evita reintentos mientras el mismo objeto permanece en el área
        if (!attempted.Add(ing)) return;

        // Guardamos estado previo para detectar si aceptó
        var prevState = client.estadoActual;

        // ¡Aquí se hace la magia! -> usa tu comparación existente
        client.RecibirPedido(ing.id);

        // Si cambió a "Comer", el ID fue correcto.
        if (destroyIngredientOnSuccess && client.estadoActual == MoveClient.Estado.Comer)
        {
            Destroy(ing.gameObject);
        }

        // Si quieres permitir reintentos con el mismo objeto (por ejemplo, si fue incorrecto y sales/entras):
        // attempted.Remove(ing);  // descomenta esta línea
    }
}
