using UnityEngine;

public class PlayerInteractionHandler : MonoBehaviour
{
    [SerializeField] public PlayerController controller;
    [SerializeField] public GameObject Hands;
    
    public bool isGrabingSomething;
    
    public GameObject GrabbedObject;
    private IInteractable grabbedInteractableComponent;
    
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

        if (interactableComponent == null && !isGrabingSomething)
            return;

        print("interact");
        
        if (isGrabingSomething && GrabbedObject != null && grabbedInteractableComponent != null)
        {
            print("drop");
            grabbedInteractableComponent.Interact(gameObject); // <--- así llamas al Drop
            isGrabingSomething = false;
            GrabbedObject = null;
            grabbedInteractableComponent = null;
            controller.animationHandler?.PlayIdle();
            return;
        }

        InteractType type = interactableComponent.GetInteractType();

        // Aquí puedes hacer cosas específicas según el tipo:
        if (type == InteractType.Grab)
        {
            isGrabingSomething = true;
            GrabbedObject = interactableObject;
            grabbedInteractableComponent = interactableComponent;
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
