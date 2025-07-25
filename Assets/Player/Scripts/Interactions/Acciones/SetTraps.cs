using UnityEngine;

public class SetTraps : MonoBehaviour, IInteractable
{
    public void Interact(GameObject interactor)
    {
    }
    
    public InteractType GetInteractType() => InteractType.SetTraps;
}
