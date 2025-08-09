using UnityEngine;

public class Clean : MonoBehaviour, IInteractable
{
    public void Interact(GameObject interactor)
    {
        interactor.GetComponent<PlayerInteractionHandler>().controller.suspect = false;
        Destroy(gameObject);
    }
    
    public InteractType GetInteractType() => InteractType.Clean;
}
