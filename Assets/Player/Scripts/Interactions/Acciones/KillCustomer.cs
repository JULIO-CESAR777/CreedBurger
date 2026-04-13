using UnityEngine;

public class KillCustomer : MonoBehaviour, IInteractable
{
    public void Interact(GameObject interactor)
    {
        var handler = interactor.GetComponent<PlayerInteractionHandler>();
        if (handler == null) return;
        if (handler.isGrabingSomething || handler.GrabbedObject != null || handler.IsKillBlocked()) return;

        MoveClient moveClient = GetComponent<MoveClient>();
        if (moveClient == null) return;

        // No matar si ya recibió el pedido y pasó a otro estado
        //if (moveClient.estadoActual == MoveClient.Estado.EsperaPedido)
            //return;

        moveClient.Alto();
        moveClient.Morir();

        ScoreSystem.Instance?.AwardKillCustomer();
    }

    public InteractType GetInteractType() => InteractType.Kill;
}