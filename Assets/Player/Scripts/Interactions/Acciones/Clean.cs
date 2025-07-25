using UnityEngine;

public class Clean : MonoBehaviour, IInteractable
{
    public void Interact(GameObject interactor)
    {
    }
    
    public InteractType GetInteractType() => InteractType.Cook;
}
