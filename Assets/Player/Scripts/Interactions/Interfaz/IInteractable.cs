using UnityEngine;

public enum InteractType
{
    Grab,
    Kill,
    Use
    
}
public interface IInteractable
{
    void Interact(GameObject interactor);
    InteractType GetInteractType();
}
