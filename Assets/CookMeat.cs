using UnityEngine;

public class CookMeat : MonoBehaviour, IInteractable
{
    [SerializeField] public GameObject pinPoint;
    
    public void Interact(GameObject interactor)
    {
        
    }
    
    public InteractType GetInteractType() => InteractType.CookMeat;
}
