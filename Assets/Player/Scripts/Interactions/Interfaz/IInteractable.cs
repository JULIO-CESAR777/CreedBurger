using UnityEngine;

public enum InteractType
{
    Grab,
    Kill,
    SetTraps,
    Cook,
    Clean,
    Cut,
    CookMeat
    
}
public interface IInteractable
{
    void Interact(GameObject interactor);
    InteractType GetInteractType();
}
