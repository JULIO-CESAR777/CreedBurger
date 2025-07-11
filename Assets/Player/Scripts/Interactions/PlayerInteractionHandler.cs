using UnityEngine;

public class PlayerInteractionHandler : MonoBehaviour
{
    [SerializeField] public PlayerController controller;
    [SerializeField] public GameObject Hands;
    
    public bool isGrabingSomething;
    public GameObject interactableObject;
    private IInteractable interactableComponent;

    private void Start()
    {
        isGrabingSomething = false;
        if (controller != null && controller.inputReader != null)
            controller.inputReader.OnInteract += Interact;
        interactableObject = null;
        interactableComponent = null;
    }

    private void OnDestroy()
    {
        controller.inputReader.OnInteract -= Interact;
    }
    

    public void Interact()
    {

        if (interactableComponent == null)
            return;

        InteractType type = interactableComponent.GetInteractType();

        // Aquí puedes hacer cosas específicas según el tipo:
        if (type == InteractType.Grab)
        {
            isGrabingSomething = true;
            controller.animationHandler?.PlayTake();
        }
        else if (type == InteractType.Kill)
        {
            // Código específico para puertas, etc.
        }
        
    }
    
    
    public void OnGrabAnimationEvent()
    {
        if (interactableComponent != null && interactableComponent.GetInteractType() == InteractType.Grab)
        {
            interactableComponent.Interact(gameObject);
        }
    }
    
    private void OnTriggerEnter(Collider other)
    {
        interactableComponent = other.GetComponent<IInteractable>();
        interactableObject = interactableComponent != null ? other.gameObject : null;
    }

    private void OnTriggerExit(Collider other)
    {
        // Si sales del trigger, limpias referencias
        if (other.gameObject == interactableObject)
        {
            interactableComponent = null;
            interactableObject = null;
        }
    }
}
