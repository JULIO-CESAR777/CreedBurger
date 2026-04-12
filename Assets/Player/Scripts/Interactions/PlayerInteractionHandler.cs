using System.Collections;
using UnityEngine;

public class PlayerInteractionHandler : MonoBehaviour
{
    [SerializeField] public PlayerController controller;
    [SerializeField] public GameObject Hands;
    [SerializeField] private Transform HoldPoint;
    [SerializeField] private Transform DardPosition;

    public bool isGrabbingDardTrap = false;
    public bool isGrabingSomething;

    // Objeto agarrado
    public GameObject GrabbedObject;
    private IInteractable grabbedInteractableComponent;

    // Posible objeto a interactuar
    public GameObject interactableObject;
    private IInteractable interactableComponent;

    [Header("Traps")]
    public float trapCooldown;
    public bool canPutTraps;
    [SerializeField] private Transform ShootingPoint;

    private Vector3 rotationCorrection = new Vector3(0, -180f, 0);

    private void Start()
    {
        isGrabingSomething = false;
        isGrabbingDardTrap = false;
        canPutTraps = true;

        if (controller != null && controller.inputReader != null)
        {
            controller.inputReader.OnInteract += Interact;
            controller.inputReader.OnTraps += SetTraps;
        }

        interactableObject = null;
        interactableComponent = null;
    }

    private void Update()
    {
        if (controller == null || controller.isPaused) return;
        if (!isGrabingSomething) return;
        if (GrabbedObject == null) return;

        Transform targetPoint = HoldPoint;

        if (GrabbedObject.GetComponent<DardTrap>() != null && DardPosition != null)
        {
            targetPoint = DardPosition;
        }

        if (targetPoint == null) return;

        GrabbedObject.transform.position = targetPoint.position;

        if (GrabbedObject.GetComponent<DardTrap>() != null)
        {
            GrabbedObject.transform.rotation =
                controller.transform.rotation * Quaternion.Euler(rotationCorrection);
        }
        else
        {
            GrabbedObject.transform.rotation = targetPoint.rotation;
        }
    }

    private void OnDestroy()
    {
        if (controller != null && controller.inputReader != null)
        {
            controller.inputReader.OnInteract -= Interact;
            controller.inputReader.OnTraps -= SetTraps;
        }
    }

    public void SetTraps()
    {
        if (!canPutTraps) return;
        if (controller.isPaused) return;

        if (!isGrabingSomething)
        {
            Instantiate(controller.trapPrefab, controller.transform.position, Quaternion.Euler(0, 90, 90));
            canPutTraps = false;
            StartCoroutine(ChangeTrapCooldown());
            return;
        }

        if (GrabbedObject == null) return;

        var trap = GrabbedObject.GetComponent<ITrap>();
        if (trap == null) return;

        if (GrabbedObject.GetComponent<DardTrap>() != null)
        {
            controller.animationHandler?.PlayShootDard();
        }
    }

    // Se manda a llamar desde un evento en la animación
    public void UseTrap()
    {
        if (GrabbedObject == null) return;

        var trap = GrabbedObject.GetComponent<ITrap>();
        if (trap == null) return;

        trap.Use(ShootingPoint);

        controller.animationHandler?.PlayIdle();
        Destroy(GrabbedObject);
        ResetGrabState();
    }

    IEnumerator ChangeTrapCooldown(float duration = 1f)
    {
        float time = 0f;

        while (time < duration)
        {
            time += Time.deltaTime;
            yield return null;
        }

        canPutTraps = true;
    }

    public void Interact()
    {
        if (controller == null || controller.isPaused) return;

        // Si ya tengo algo en la mano
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
                        controller.animationHandler?.PlayDrop();
                        return;
                    }
                }

                // COCINAR EN PINPOINT
                var cookMeat = interactableObject.GetComponent<CookMeat>();
                if (cookMeat != null && GrabbedObject.name == "Carne")
                {
                    var grab = GrabbedObject.GetComponent<GrabObject>();
                    if (grab != null)
                    {
                        grab.SetHeldState(false);
                    }

                    controller.suspect = false;
                    GrabbedObject.transform.position = cookMeat.pinPoint.transform.position;

                    ResetGrabState();
                    controller.animationHandler?.PlayDrop();
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
                        controller.animationHandler?.PlayDrop();
                        return;
                    }
                }
            }

            // Drop normal
            grabbedInteractableComponent.Interact(gameObject);
            ResetGrabState();
            controller.animationHandler?.PlayDrop();
            return;
        }

        // Si no tengo nada en la mano
        if (interactableComponent == null) return;

        InteractType type = interactableComponent.GetInteractType();

        switch (type)
        {
            case InteractType.Grab:
            {
                if (interactableObject == null) return;
                if (interactableObject.name == "Meat Machine" && isGrabingSomething) return;

                var grabObject = interactableObject.GetComponent<GrabObject>();

                if (grabObject != null && !grabObject.CanBeGrabbed())
                {
                    controller.gameManager.hudScoreUI.ShowNoMoneyPanel();
                    return;
                }

                // Solo mostrar costo si realmente es un spawner con precio
                if (grabObject != null && grabObject.CompareTag("Spawner") && grabObject.price > 0)
                {
                    controller.gameManager.hudScoreUI.ShowSubstractMoneyPanel(grabObject.price);
                }

                controller.animationHandler?.PlayTake();
                grabbedInteractableComponent = interactableComponent;
                controller.playerMovement.canMove = false;
                break;
            }

            case InteractType.Kill:
                controller.playerMovement.canMove = false;
                controller.suspect = true;
                controller.animationHandler?.PlayKill();
                break;

            case InteractType.Clean:
                controller.inputReader.isDoingSomething = true;
                controller.suspect = true;
                controller.playerMovement.canMove = false;
                controller.animationHandler?.PlayClean();
                break;

            case InteractType.SetTraps:
                break;
        }
    }

    public void GrabTrap()
    {
        if (GrabbedObject == null) return;

        if (GrabbedObject.GetComponent<ITrap>() == null)
        {
            isGrabbingDardTrap = false;
            controller.animationHandler?.KeepTheObject();
        }
        else if (GrabbedObject.GetComponent<DardTrap>() != null)
        {
            isGrabbingDardTrap = true;
            controller.animationHandler?.PlayTakeDard();
        }
    }
    
    public void SetGrabbedInteractable(IInteractable interactable)
    {
        grabbedInteractableComponent = interactable;
    }

    private void ResetGrabState()
    {
        isGrabingSomething = false;
        isGrabbingDardTrap = false;
        GrabbedObject = null;
        grabbedInteractableComponent = null;
    }

    // Funciones para interactuar desde las animaciones
    public void OnGrabAnimationEvent()
    {
        if (controller == null || controller.isPaused) return;

        if (interactableComponent != null && interactableComponent.GetInteractType() == InteractType.Grab)
        {
            interactableComponent.Interact(gameObject);
        }
    }

    public void OnKillAnimationEvent()
    {
        if (controller == null || controller.isPaused) return;

        if (GrabbedObject == null &&
            interactableComponent != null &&
            interactableComponent.GetInteractType() == InteractType.Kill)
        {
            interactableComponent.Interact(gameObject);
        }
    }

    public void OnCleanAnimationEvent()
    {
        if (controller == null || controller.isPaused) return;

        if (GrabbedObject == null &&
            interactableComponent != null &&
            interactableComponent.GetInteractType() == InteractType.Clean)
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