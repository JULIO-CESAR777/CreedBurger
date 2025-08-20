using UnityEngine;

public class CutIngredients : MonoBehaviour, IInteractable
{
    public void Interact(GameObject interactor)
    {
        
    }
    
    public InteractType GetInteractType() => InteractType.Cut;
}
