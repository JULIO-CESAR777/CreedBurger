using UnityEngine;

public class CookIngredients : MonoBehaviour, IInteractable
{
    public void Interact(GameObject interactor)
    {
    }
    
    public InteractType GetInteractType() => InteractType.Clean;
}
