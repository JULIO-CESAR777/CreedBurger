using System.Collections;
using UnityEngine;
using UnityEngine.UIElements;

public class GrabObject : MonoBehaviour, IInteractable
{
    public bool isGrabbed = false;

    private IngredientSpawner ingredientSpawner;
    private bool isSpawner = false;

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
        Physics.IgnoreCollision(a, b, false);
    }
    
    public void Interact(GameObject interactor)
    {
        var interactionHandler = interactor.GetComponent<PlayerInteractionHandler>();

        if (!isGrabbed)
        {
            // 🔄 AGARRAR OBJETO
            Transform handTransform = interactionHandler.Hands.transform;
            if (handTransform == null) return;

            if (isSpawner)
            {
                var ingredient = Instantiate(ingredientSpawner.ingredientSpawner, transform.position, transform.rotation);

                var spawnerCollider = GetComponent<Collider>();
                var ingredientCollider = ingredient.GetComponent<Collider>();
                if (spawnerCollider != null && ingredientCollider != null)
                {
                    Physics.IgnoreCollision(spawnerCollider, ingredientCollider);
                    StartCoroutine(ReenableCollisionAfterDelay(spawnerCollider, ingredientCollider, 1f));
                }

                ingredient.transform.SetParent(handTransform, true);
                var rb = ingredient.GetComponent<Rigidbody>();
                if (rb != null) rb.isKinematic = true;

                ingredient.transform.localPosition = Vector3.zero;
                isGrabbed = true;

                interactionHandler.isGrabingSomething = true;
                interactionHandler.GrabbedObject = ingredient;
                return;
            }
            else
            {
                if (gameObject.GetComponent<DardTrap>() == null)
                {
                    transform.SetParent(handTransform, true);
                }
                else
                {
                    gameObject.GetComponent<BoxCollider>().isTrigger = true;
                }

                var rb = GetComponent<Rigidbody>();
                if (rb != null) rb.isKinematic = true;
                transform.localPosition = Vector3.zero;
                isGrabbed = true;
                interactionHandler.isGrabingSomething = true;
                interactionHandler.GrabbedObject = gameObject;  
                interactionHandler.GrabTrap();
                
                return;
            }
        }

        // 🔄 DROP NORMAL
        DropNormally(interactor, interactionHandler);
    }

    private void DropNormally(GameObject interactor, PlayerInteractionHandler handler)
    {
        // Si era una cerbatana, dejar de seguirla manualmente
        handler.isGrabbingDardTrap = false;

        transform.SetParent(null, true);

        foreach (Transform child in handler.Hands.transform)
        {
            child.SetParent(null, true);

            var rbChild = child.GetComponent<Rigidbody>();
            if (rbChild != null)
            {
                rbChild.isKinematic = false;
            }
        }

        var rb = GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.isKinematic = false;
            rb.AddForce(interactor.transform.forward * 2f, ForceMode.Impulse);
        }

        var collider = GetComponent<Collider>();
        if (collider != null)
        {
            collider.isTrigger = false;
        }

        handler.controller.suspect = false;
        isGrabbed = false;
    }

    public InteractType GetInteractType() => InteractType.Grab;
}
