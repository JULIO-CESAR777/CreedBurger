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
        {
            controller.inputReader.OnInteract += Interact;
            controller.inputReader.OnTraps += SetTraps;
        }
        
        interactableObject = null;
        interactableComponent = null;
    }

    private void OnDestroy()
    {
        controller.inputReader.OnInteract -= Interact;
        controller.inputReader.OnTraps -= SetTraps;
    }

    public void SetTraps()
    {
        print("sexo publico");        
    }
    
    public void Interact()
    {
        
        if (interactableComponent == null && !isGrabingSomething)
            return;

        // Obtiene el tipo de accion
        InteractType type = interactableComponent.GetInteractType();
        
        
        if (isGrabingSomething && 
            GrabbedObject != null && 
            grabbedInteractableComponent != null)
        {

            CookMeat cookMeat = interactableObject.GetComponent<CookMeat>();
            
            if (interactableObject != null &&  cookMeat != null 
                && type == InteractType.CookMeat && GrabbedObject.name == "Carne")
            {
                GrabbedObject.transform.SetParent(null, true);
                // Física on + empujón opcional
                var rb = GrabbedObject.GetComponent<Rigidbody>();
                if (rb != null)
                {
                    rb.isKinematic = false;
                }
                controller.suspect = false;
                GrabbedObject.GetComponent<GrabObject>().isGrabbed = false;
                GrabbedObject.transform.position = cookMeat.pinPoint.transform.position;
                
                
            }
            else
            {
                grabbedInteractableComponent.Interact(gameObject); // <--- así llamas al Drop
            }
            isGrabingSomething = false; 
            GrabbedObject = null; 
            grabbedInteractableComponent = null;
            controller.animationHandler?.PlayIdle();
            
            return;
        }
        
        
        // Interactua dependiendo del tipo de accion 
        // Acciones sin objetos en mano
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
