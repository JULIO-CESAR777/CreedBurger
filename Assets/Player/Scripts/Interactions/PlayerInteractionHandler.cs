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
        
        if (isGrabingSomething && GrabbedObject != null && grabbedInteractableComponent != null)
        {
            grabbedInteractableComponent.Interact(gameObject); // <--- así llamas al Drop
            isGrabingSomething = false;
            GrabbedObject = null;
            grabbedInteractableComponent = null;
            controller.animationHandler?.PlayIdle();
            return;
        }

        InteractType type = interactableComponent.GetInteractType();

        switch (type)
        {
            case InteractType.Grab:
            {
                isGrabingSomething = true;
                GrabbedObject = interactableObject;
                grabbedInteractableComponent = interactableComponent;
                controller.animationHandler?.PlayTake();
                break;
            }
            case InteractType.Kill:
            {
                controller.playerMovement.canMove = false;
                controller.animationHandler?.PlayKill();
                // llamo a la funcion del cliente
                print("asesinado");
                interactableComponent.Interact(gameObject);
                controller.playerMovement.canMove = true;
                break;
            }
            case InteractType.Cook:
            {
                break;
            }
            case InteractType.Clean:
            {
                break;
            }
            case InteractType.SetTraps:
            {
                break;
            }
                
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
        if (other.gameObject == interactableObject)
        {
            interactableComponent = null;
            interactableObject = null;
        }
    }
}
