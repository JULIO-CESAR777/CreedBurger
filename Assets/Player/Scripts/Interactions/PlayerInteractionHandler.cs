using System;
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

    //[SerializeField] public Image cooldownFillImage;
    
    
    [Header("Traps")]
    public float trapCooldown;
    public bool canPutTraps;
    [SerializeField] private Transform ShootingPoint;
    
    private void Start()
    {
        isGrabingSomething = false;
        canPutTraps = true;
        if (controller != null && controller.inputReader != null)
        {
            controller.inputReader.OnInteract += Interact;
            controller.inputReader.OnTraps += SetTraps;
        }
        
        //cooldownFillImage = GameObject.FindWithTag("TrapCoolDown").GetComponent<Image>();
        //cooldownFillImage.fillAmount = 1f;
        
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
        if (controller.isPaused) return;

        if (isGrabingSomething == false)
        {
            Instantiate(controller.trapPrefab, controller.transform.position, Quaternion.Euler(0, 90, 90));
            canPutTraps = false;
            StartCoroutine(ChangeTrapCooldown());
        }
        else if (GrabbedObject.GetComponent<ITrap>() != null)
        {
            /* TODO: Checar que cuando un objeto se agarre sea una trampa o no (para las animaciones)
                
            */
            
            /*
             Usar esta funcion en la animacion de disparar dardos... sirve para hacer la trampa, no importa si es de 
             dardos o no pero es para llamarla desde una animacion
             TODO: Mandar a llamar a la funcion UseTrap desde el fin de la animacion
            */
            UseTrap();
        }
    }

    public void UseTrap()
    {
        GrabbedObject.GetComponent<ITrap>().Use(ShootingPoint);
        
        //TODO: Esto se debe hacer dps de toda la animacion de uso de la trampa
        //controller.animationHandler?.PlayDard();
        
        controller.animationHandler?.PlayIdle();
        Destroy(GrabbedObject);
        ResetGrabState();
    }


    IEnumerator ChangeTrapCooldown(float duration = 1f)
    {
        float time = 0f;
        //cooldownFillImage.fillAmount = 0f;

        while (time < duration)
        {
            time += Time.deltaTime;
            //cooldownFillImage.fillAmount = 1f - (time / duration);
            yield return null;
        }

        //cooldownFillImage.fillAmount = 1f;
        canPutTraps = true;
    }
    
   public void Interact()
   {
        if (controller.isPaused) return;

        // Si estoy agarrando algo
        if (isGrabingSomething && GrabbedObject != null && grabbedInteractableComponent != null)
        {
            if (interactableObject != null)
            {
                // USAR MEAT MACHINE
                if (interactableObject.name.Contains("Meat Machine") && GrabbedObject.CompareTag("Carne"))
                {
                    var spawner = interactableObject.GetComponent<SpawningMeat>();
                    if (spawner != null)
                    {
                        spawner.SpawnMeat();
                        Destroy(GrabbedObject);
                        ResetGrabState();
                        controller.animationHandler?.PlayIdle();
                        return;
                    }
                }

                // COCINAR EN PINPOINT
                var cookMeat = interactableObject.GetComponent<CookMeat>();
                if (cookMeat != null && GrabbedObject.name == "Carne")
                {
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

                // COCINA DE COMBINACIÓN
                var targetCook = interactableObject.GetComponent<CookIngredients>();
                var thisCook = GrabbedObject.GetComponent<CookIngredients>();
                if (targetCook != null && thisCook != null)
                {
                    bool fused = false;
                    foreach (var ingredient in thisCook.ingredientIDs)
                    {
                        if (targetCook.checkForRepeatedIngredients(ingredient))
                        {
                            fused = true;
                            break;
                        }
                    }

                    if (fused)
                    {
                        targetCook.TryAddIngredient(thisCook);
                        controller.suspect = false;
                        Destroy(GrabbedObject);
                        ResetGrabState();
                        controller.animationHandler?.PlayIdle();
                        return;
                    }
                }
            }

            // Drop normal si no hay nada especial que hacer
            grabbedInteractableComponent.Interact(gameObject);
            ResetGrabState();
            controller.animationHandler?.PlayIdle();
            return;
        }

        // Si no tengo nada en la mano, y hay un objeto válido
        if (interactableComponent == null) return;

        InteractType type = interactableComponent.GetInteractType();

        switch (type)
        {
            case InteractType.Grab:
                if (interactableObject.name == "Meat Machine" && isGrabingSomething) return;
                // Revisar si agarra una trampa
                if (GrabbedObject.GetComponent<ITrap>() != null)
                {
                    // Que tipo de trampa agarra
                    // 1. Trampa de dardo
                    if (GrabbedObject.GetComponent<DardTrap>() != null)
                    {
                        controller.animationHandler.PlayTakeDard();
                    }
                }
                // Si no es una trampa
                else
                {
                    controller.animationHandler?.PlayTake();  
                }
                controller.playerMovement.canMove = false;
                grabbedInteractableComponent = interactableComponent;
                break;
            
            case InteractType.Kill:
                controller.playerMovement.canMove = false;
                controller.suspect = true;
                controller.animationHandler?.PlayKill();
                break;
            case InteractType.Clean:
                // "Esta haciendo algo" -> Para que no pueda interactuar hasta que acabe
                controller.inputReader.isDoingSomething = true;
                // "Sospechoso" -> Detectable por los clientes
                controller.suspect = true;
                controller.playerMovement.canMove = false;
                controller.animationHandler?.PlayClean();
                break;
            case InteractType.SetTraps:
                break;
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
        if (controller.isPaused) return;
        if (interactableComponent != null && interactableComponent.GetInteractType() == InteractType.Grab)
        {
            interactableComponent.Interact(gameObject);
        }
    }

    public void OnKillAnimationEvent()
    {
        if (controller.isPaused) return;
        if (GrabbedObject == null && interactableComponent != null && interactableComponent.GetInteractType() == InteractType.Kill)
        {
            interactableComponent.Interact(gameObject);
        }
    }

    public void OnCleanAnimationEvent()
    {
        if (controller.isPaused) return;
        if (GrabbedObject == null && interactableComponent != null && interactableComponent.GetInteractType() == InteractType.Clean)
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
