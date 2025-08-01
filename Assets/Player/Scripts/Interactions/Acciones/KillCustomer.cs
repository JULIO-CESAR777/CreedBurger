using UnityEngine;

public class KillCustomer : MonoBehaviour, IInteractable
{
    
    public void Interact(GameObject interactor)
    {
        
        print("dentro del interact");
        MoveClient moveClient = gameObject.GetComponent<MoveClient>();
        
        if(moveClient == null) return;
        
        moveClient.Alto();
        moveClient.Morir();
        /*
         * TODO: todo eso deberia de ir en una funcion dentro del cliente
         * Agregar la muerte del cliente
         * Spawn de la carne
         * desaparecer al cliente
         */



    }
    
    public InteractType GetInteractType() => InteractType.Kill;
    
}
