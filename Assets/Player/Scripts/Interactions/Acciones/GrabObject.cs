using UnityEngine;

public class GrabObject : MonoBehaviour, IInteractable
{
    
    public bool isGrabbed = false;
    
    public void Interact(GameObject interactor)
    {

        PlayerInteractionHandler interactionHandler = interactor.GetComponent<PlayerInteractionHandler>();
        
        if (!isGrabbed)
        {
            if (gameObject.name == "Meat")
            {
                interactionHandler.controller.suspect = true;
            }

            Transform handTransform = interactor.GetComponent<PlayerInteractionHandler>().Hands.transform;
            if (handTransform != null)
            {
            
                transform.SetParent(handTransform, true);

                var rb = GetComponent<Rigidbody>();
                if (rb != null)
                    rb.isKinematic = true;

                // Opcional: poner posición relativa a la mano
                transform.localPosition = Vector3.zero;
                isGrabbed = true;
                return;
            }
        }
        
        
        GameObject target = interactionHandler.interactableObject;
        
        // Solo interactuar con el objeto para cocinar
        if (target != null && target != interactionHandler.controller.gameObject)
        {
            CookIngredients targetCook = target.GetComponent<CookIngredients>();
            CookIngredients thisCook = GetComponent<CookIngredients>();
            if (targetCook != null && thisCook != null)
            {
                // Fusionar los ingredientes de ambos
                bool fused = false;
                
                foreach (var ingredient in thisCook.ingredientIDs)
                {
                    print("ingrediente: " + ingredient);

                    if (targetCook.checkForRepeatedIngredients(ingredient))
                    {
                        fused = true;
                    }
                    
                }
                

                if (fused)
                {
                    print("se juntaron");
                    targetCook.TryAddIngredient(thisCook);
                    // Destruye este objeto (el dropeado)
                    interactionHandler.controller.suspect = false;
                    Destroy(gameObject);
                    return;
                }
                
            }
        }
            
        DropNormally(interactor, interactionHandler);
        
    }
    
    private void DropNormally(GameObject interactor, PlayerInteractionHandler handler)
    {
        // Quitar de la mano
        transform.SetParent(null, true);

        // Física on + empujón opcional
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
