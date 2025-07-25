using UnityEngine;

public class KillCustomer : MonoBehaviour, IInteractable
{
    
    public void Interact(GameObject interactor)
    {
        // Comportamiento de la carne que suelta el cliente o algo por el estilo
    }
    
    public InteractType GetInteractType() => InteractType.Kill;
    
}
