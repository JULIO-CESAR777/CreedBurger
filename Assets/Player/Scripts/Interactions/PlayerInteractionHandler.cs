using System.Collections;
using UnityEngine;
using UnityEngine.UI;

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

    [SerializeField] public Image cooldownFillImage;
    public float trapCooldown;
    public bool canPutTraps;
    
    private void Start()
    {
        isGrabingSomething = false;
        canPutTraps = true;
        if (controller != null && controller.inputReader != null)
        {
            controller.inputReader.OnInteract += Interact;
            controller.inputReader.OnTraps += SetTraps;
        }
        
        cooldownFillImage = GameObject.FindWithTag("TrapCoolDown").GetComponent<Image>();
        cooldownFillImage.fillAmount = 1f;
        
        interactableObject = null;
        interactableComponent = null;
    }

    private void OnDestroy()
    {
        if (controller != null && controller.inputReader != null)
        {
            controller.inputReader.OnInteract -= Interact;
            controller.inputReader.OnTraps   -= SetTraps;
        }
    }

    public void SetTraps()
    {
        if (!canPutTraps) return;
        
        Instantiate(controller.trapPrefab, controller.transform.position, Quaternion.Euler(0, 90, 90));
        canPutTraps = false;
        StartCoroutine(ChangeTrapCooldown());
    }

    IEnumerator ChangeTrapCooldown(float duration = 1f)
    {
        float time = 0f;
        cooldownFillImage.fillAmount = 0f;

        while (time < duration)
        {
            time += Time.deltaTime;
            cooldownFillImage.fillAmount = 1f - (time / duration);
            yield return null;
        }

        cooldownFillImage.fillAmount = 1f;
        canPutTraps = true;
    }
    
    public void Interact()
    {
        // 1) Si estoy agarrando algo, prioriza soltar/usar sin tocar interactableComponent
        if (isGrabingSomething && GrabbedObject != null && grabbedInteractableComponent != null)
        {
            if (interactableObject != null) // hay algo enfrente
            {
                var cookMeat = interactableObject.GetComponent<CookMeat>();
                if (cookMeat != null && GrabbedObject != null
                    && GrabbedObject.name == "Carne")
                {
                    // Colocar la carne en el pinPoint
                    GrabbedObject.transform.SetParent(null, true);

                    var rb = GrabbedObject.GetComponent<Rigidbody>();
                    if (rb != null) rb.isKinematic = false;

                    controller.suspect = false;
                    var grab = GrabbedObject.GetComponent<GrabObject>();
                    if (grab != null) grab.isGrabbed = false;

                    GrabbedObject.transform.position = cookMeat.pinPoint.transform.position;

                    ResetGrabState();
                    controller.animationHandler?.PlayIdle();
                    return;
                }
            }

            // Drop normal del objeto en mano
            grabbedInteractableComponent.Interact(gameObject);
            ResetGrabState();
            controller.animationHandler?.PlayIdle();
            return;
        }

        // 2) Si no estoy agarrando nada y no hay target válido, no hay nada que hacer
        if (interactableComponent == null) return;

        // 3) Ya es seguro pedir el tipo
        InteractType type = interactableComponent.GetInteractType();

        // 4) Acciones sin objeto en mano
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
                // ...
                break;
            }
        }
    }
    
    private void ResetGrabState()
    {
        isGrabingSomething = false;
        GrabbedObject = null;
        grabbedInteractableComponent = null;
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
