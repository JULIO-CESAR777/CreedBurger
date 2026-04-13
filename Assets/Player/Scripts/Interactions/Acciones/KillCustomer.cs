using UnityEngine;

public class KillCustomer : MonoBehaviour, IInteractable
{
    public void Interact(GameObject interactor)
    {
        var handler = interactor.GetComponent<PlayerInteractionHandler>();
        if (handler == null) return;

        MoveClient moveClient = GetComponent<MoveClient>();
        if (moveClient == null) return;

        // =========================
        // SI TRAE ALGO EN LA MANO -> INTENTAR ENTREGAR
        // =========================
        if (handler.isGrabingSomething && handler.GrabbedObject != null)
        {
            if (moveClient.estadoActual != MoveClient.Estado.EsperaPedido)
                return;

            GameObject heldObject = handler.GrabbedObject;

            Ingredient ing = heldObject.GetComponent<Ingredient>();
            CookIngredients cook = null;

            if (ing == null)
                cook = heldObject.GetComponent<CookIngredients>();

            if (ing == null && cook == null) return;

            int deliveredId = ing != null ? ing.id : cook.currentComboID;

            moveClient.RecibirPedido(deliveredId);

            // Si aceptó el pedido y pasó a Comer
            if (moveClient.estadoActual == MoveClient.Estado.Comer)
            {
                ScoreSystem.Instance?.AwardDeliverySuccess();

                var grab = heldObject.GetComponent<GrabObject>();
                if (grab != null)
                {
                    handler.ClearHeldObjectAfterDelivery(heldObject);
                    handler.BlockKillForSeconds(0.25f);
                }

                Object.Destroy(heldObject);
            }

            return;
        }

        // =========================
        // SI NO TRAE NADA -> MATAR
        // =========================
        if (handler.IsKillBlocked()) return;

        moveClient.Alto();
        moveClient.Morir();

        ScoreSystem.Instance?.AwardKillCustomer();
    }

    public InteractType GetInteractType() => InteractType.Kill;
}