using UnityEngine;

public class PlayerInteractionHandler : MonoBehaviour
{
    [SerializeField] public PlayerController controller;
    [SerializeField] public GameObject Hands;
    
    public bool isGrabingSomething;
    
    // Objeto agarrado
    public GameObject GrabbedObject;
    private IInteractable grabbedInteractableComponent;
    
    // Posible objeto a interactuar
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
        
        /*
         * Suceso cuando se quiere soltar una cosa
         * ----> Es un breakpoint por que no se permiten
         * otras acciones mientras se este sosteniendo algo
         */
        if (isGrabingSomething && 
            GrabbedObject != null && 
            grabbedInteractableComponent != null)
        {
            grabbedInteractableComponent.Interact(gameObject); // <--- así llamas al Drop
            isGrabingSomething = false;
            GrabbedObject = null;
            grabbedInteractableComponent = null;
            controller.animationHandler?.PlayIdle();
            return;
        }
        
        // Obtiene el tipo de accion
        InteractType type = interactableComponent.GetInteractType();
       
        // Interactua dependiendo del tipo de accion
        switch (type)
        {
            case InteractType.Grab:
            {
                controller.playerMovement.canMove = false;
                isGrabingSomething = true;
                GrabbedObject = interactableObject;
                grabbedInteractableComponent = interactableComponent;
                controller.animationHandler?.PlayTake();
                break;
            }
            case InteractType.Kill:
            {
                controller.playerMovement.canMove = false;
                controller.suspect = true;
                controller.animationHandler?.PlayKill();
                break;
            }
            case InteractType.Clean:
            {
                controller.suspect = true;
                controller.playerMovement.canMove = false;
                controller.animationHandler?.PlayClean();
                break;
            }
            case InteractType.SetTraps:
            {
                break;
            }
                
        }
        
    }
    
    // Funciones para interactuar desde las animaciones
    public void OnGrabAnimationEvent()
    {
        if (interactableComponent != null && interactableComponent.GetInteractType() == InteractType.Grab)
        {
            interactableComponent.Interact(gameObject);
        }
    }

    public void OnKillAnimationEvent()
    {
        if (interactableComponent != null && interactableComponent.GetInteractType() == InteractType.Kill)
        {
            interactableComponent.Interact(gameObject);
        }
    }

    public void OnCleanAnimationEvent()
    {
        if (interactableComponent != null && interactableComponent.GetInteractType() == InteractType.Clean)
        {
            interactableComponent.Interact(gameObject);
        }
    }
    
    
    // Se obtienen y se limpian referencias de los objetos interactuables
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
