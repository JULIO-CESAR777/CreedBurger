using System.Collections;
using UnityEngine;

public class GrabObject : MonoBehaviour, IInteractable
{
    public bool isGrabbed = false;

    private IngredientSpawner ingredientSpawner;
    public int price;
    private bool isSpawner = false;
    
    public bool canBeGrabbed = false;

    public PlayerInteractionHandler CurrentHolder { get; private set; }

    private void Start()
    {
        if (CompareTag("Spawner"))
        {
            ingredientSpawner = GetComponent<IngredientSpawner>();
            isSpawner = true;
        }
    }

    IEnumerator ReenableCollisionAfterDelay(Collider a, Collider b, float delay)
    {
        yield return new WaitForSeconds(delay);

        if (a == null || b == null) yield break;
        if (a.gameObject == null || b.gameObject == null) yield break;

        Physics.IgnoreCollision(a, b, false);
    }

    public bool CanBeGrabbed()
    {
        if (!canBeGrabbed) return false;
        if (!isSpawner) return true;
        if (ingredientSpawner == null) return false;
        if (ScoreSystem.Instance == null) return false;

        return ScoreSystem.Instance.Coins >= price;
    }

    public void SetHeldState(bool held)
    {
        var rb = GetComponent<Rigidbody>();
        if (rb != null)
            rb.isKinematic = held;

        var col = GetComponent<Collider>();
        if (col != null)
            col.isTrigger = held;

        isGrabbed = held;

        if (!held)
            CurrentHolder = null;
    }

    public void Interact(GameObject interactor)
    {
        var interactionHandler = interactor.GetComponent<PlayerInteractionHandler>();
        if (interactionHandler == null) return;
        
        if (!isGrabbed)
        {
            if (isSpawner)
            {
                if (ScoreSystem.Instance == null) return;
                if (!ScoreSystem.Instance.HasEnoughCoins(price)) return;

                var spawnedObject = Instantiate(
                    ingredientSpawner.ingredientSpawner,
                    transform.position,
                    transform.rotation
                );

                var spawnerCollider = GetComponent<Collider>();
                var spawnedCollider = spawnedObject.GetComponent<Collider>();
                if (spawnerCollider != null && spawnedCollider != null)
                {
                    Physics.IgnoreCollision(spawnerCollider, spawnedCollider, true);
                    StartCoroutine(ReenableCollisionAfterDelay(spawnerCollider, spawnedCollider, 1f));
                }

                var spawnedGrab = spawnedObject.GetComponent<GrabObject>();
                if (spawnedGrab != null)
                {
                    spawnedGrab.SetHeldState(true);
                    spawnedGrab.CurrentHolder = interactionHandler;
                    interactionHandler.SetGrabbedInteractable(spawnedGrab);
                }

                interactionHandler.isGrabingSomething = true;
                interactionHandler.GrabbedObject = spawnedObject;
                interactionHandler.GrabTrap();
                return;
            }
            else
            {
                SetHeldState(true);
                CurrentHolder = interactionHandler;

                interactionHandler.isGrabingSomething = true;
                interactionHandler.GrabbedObject = gameObject;
                interactionHandler.SetGrabbedInteractable(this);
                interactionHandler.GrabTrap();
                return;
            }
        }

        DropNormally(interactor, interactionHandler);
    }

    private void DropNormally(GameObject interactor, PlayerInteractionHandler handler)
    {
        handler.isGrabbingDardTrap = false;

        SetHeldState(false);

        var rb = GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.AddForce(interactor.transform.forward * 2f, ForceMode.Impulse);
        }

        handler.controller.suspect = false;
    }

    public InteractType GetInteractType() => InteractType.Grab;
}