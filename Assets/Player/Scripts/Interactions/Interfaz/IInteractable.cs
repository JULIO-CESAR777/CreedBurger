using UnityEngine;

public enum InteractType
{
    Grab,
    Kill,
    SetTraps,
    Cook,
    Clean
    
}
public interface IInteractable
{
    void Interact(GameObject interactor);
    InteractType GetInteractType();
}
