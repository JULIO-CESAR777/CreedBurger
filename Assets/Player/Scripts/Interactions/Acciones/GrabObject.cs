using UnityEngine;

public class GrabObject : MonoBehaviour, IInteractable
{
    
    public bool isGrabbed = false;
    
    public void Interact(GameObject interactor)
    {

        PlayerInteractionHandler interactionHandler = interactor.GetComponent<PlayerInteractionHandler>();
        
        if (isGrabbed == false)
        {
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
            }
            else
            {
                Debug.LogWarning($"{interactor.name} no tiene un hijo llamado 'Manos'");
            }    
        }
        else
        {

            // Solo interactuar con el objeto para cocinar
            if (interactionHandler.interactableObject != null)
            {
                CookIngredients targetCook = interactionHandler.interactableObject.GetComponent<CookIngredients>();
                CookIngredients thisCook = interactionHandler.GrabbedObject.GetComponent<CookIngredients>();
                if (targetCook != null && thisCook != null)
                {
                    // Fusionar los ingredientes de ambos
                    bool newIngredient = false;
                    foreach (var ingredient in thisCook.currentIngredients)
                    {
                        // Si alguno fue nuevo, hubo fusión
                        if (targetCook.TryAddIngredient(ingredient))
                            newIngredient = true;
                    }

                    if (newIngredient)
                    {
                        // Destruye este objeto (el dropeado)
                        Destroy(gameObject);

                        // Si se cumple la receta, cocina
                        if (targetCook.CanCookSandwich())
                            targetCook.Cook();
                        return;
                    }
                    else
                    {
                        Debug.Log("Todos los ingredientes ya estaban, solo se suelta.");
                    }
                }
            }
            // Quitar el objeto de la mano
            transform.SetParent(null, true);

            // Activar la física si tiene Rigidbody
            var rb = GetComponent<Rigidbody>();
            if (rb != null)
                rb.isKinematic = false;

            // Opcional: darle un pequeño empuje al soltar
            rb.AddForce(interactor.transform.forward * 2f, ForceMode.Impulse);
            isGrabbed = false;
        }


    }

    public InteractType GetInteractType() => InteractType.Grab;
}
