using System.Collections;
using UnityEngine;

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
                transform.SetParent(handTransform, true);
                var rb = GetComponent<Rigidbody>();
                if (rb != null) rb.isKinematic = true;

                transform.localPosition = Vector3.zero;
                isGrabbed = true;

                interactionHandler.isGrabingSomething = true;
                interactionHandler.GrabbedObject = gameObject;
                return;
            }
        }

        // 🔄 DROP NORMAL
        DropNormally(interactor, interactionHandler);
    }

    private void DropNormally(GameObject interactor, PlayerInteractionHandler handler)
    {
        transform.SetParent(null, true);

        foreach (Transform child in handler.Hands.transform)
        {
            child.SetParent(null, true);
            var rb_ = child.GetComponent<Rigidbody>();
            if (rb_ != null)
            {
                rb_.isKinematic = false;
            }
        }

        var rb = GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.isKinematic = false;
            rb.AddForce(interactor.transform.forward * 2f, ForceMode.Impulse);
        }

        handler.controller.suspect = false;
        isGrabbed = false;
    }

    public InteractType GetInteractType() => InteractType.Grab;
}
