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

            // Solo interactuar con el objeto que está enfrente
            if (interactionHandler.interactableObject != null)
            {
                CookIngredients cookIngredients = interactionHandler.interactableObject.GetComponent<CookIngredients>();
                Ingredient ingredient = interactionHandler.GrabbedObject.GetComponent<Ingredient>();
                if (cookIngredients != null && ingredient != null)
                {
                    bool added = cookIngredients.TryAddIngredient(ingredient.type);
                    if (added)
                    {
                        Debug.Log("Ingrediente agregado a la combinación: " + ingredient.type);
                        // Opcional: destruye este objeto porque ya fue absorbido
                        Destroy(gameObject);
                        // Si se cumple la receta, cocina
                        if (cookIngredients.CanCookSandwich())
                            cookIngredients.Cook();
                        return;
                    }
                    else
                    {
                        Debug.Log("Ingrediente repetido, solo se suelta.");
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
