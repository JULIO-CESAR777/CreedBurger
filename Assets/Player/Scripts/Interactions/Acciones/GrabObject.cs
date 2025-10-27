using System;
using System.Collections;
using UnityEngine;

public class GrabObject : MonoBehaviour, IInteractable
{
    
    public bool isGrabbed = false;
    IngredientSpawner ingredientSpawner;
    bool isSpawner = false;
    

    private void Start()
    {
        if (gameObject.CompareTag("Spawner"))
        {
            ingredientSpawner = gameObject.GetComponent<IngredientSpawner>();
            isSpawner = true;
        }
    }
    
    IEnumerator ReenableCollisionAfterDelay(Collider a, Collider b, float delay)
    {
        yield return new WaitForSeconds(delay);
        Physics.IgnoreCollision(a, b, false); // Ahora sí vuelven a colisionar
    }

    public void Interact(GameObject interactor)
    {

        PlayerInteractionHandler interactionHandler = interactor.GetComponent<PlayerInteractionHandler>();
        
        if (!isGrabbed)
        {

            if (gameObject.name == "Meat Machine") return;
            
            if (gameObject.name == "Meat")
            {
                interactionHandler.controller.suspect = true;
            }

            
            Transform handTransform = interactor.GetComponent<PlayerInteractionHandler>().Hands.transform;

            if (handTransform == null) return;
            
            // Si se intenta agarrar un item desde un spawner
            if (isSpawner)
            {
                print("Interactua con el grabObject");
                // Spawn del item
                GameObject ingredient = Instantiate(ingredientSpawner.ingredientSpawner, gameObject.transform.position, gameObject.transform.rotation);
                
                // Obtener los colliders del ingrediente y del spawn
                Collider spawnerCollider = gameObject.GetComponent<Collider>();
                Collider ingredientCollider = ingredient.GetComponent<Collider>();

                if (spawnerCollider != null && ingredientCollider != null)
                {
                    // Ignorar la colision
                    Physics.IgnoreCollision(spawnerCollider, ingredientCollider);
                    StartCoroutine(ReenableCollisionAfterDelay(spawnerCollider, ingredientCollider, 1f));
                }
                
                // Seteo hacia las manos del jugador
                ingredient.transform.SetParent(handTransform, true);
                
                // Apagamos el rigidbody del ingrediente
                var rb = ingredient.GetComponent<Rigidbody>();
                if (rb != null)
                    rb.isKinematic = true;
                
                // Opcional: poner posición relativa a la mano
                ingredient.transform.localPosition = Vector3.zero;
                isGrabbed = true;
                
                // Parte del player
                interactionHandler.isGrabingSomething = true;
                interactionHandler.GrabbedObject = ingredient;
                
                return;
            }
            // SI se agarra un item en el piso
            else
            {
                transform.SetParent(handTransform, true);

                var rb = GetComponent<Rigidbody>();
                if (rb != null)
                    rb.isKinematic = true;

                // Opcional: poner posición relativa a la mano
                transform.localPosition = Vector3.zero;
                isGrabbed = true;
                
                // Parte del player
                interactionHandler.isGrabingSomething = true;
                interactionHandler.GrabbedObject = gameObject;
                
                return;
            }
            
        }
        
        GameObject target = interactionHandler.interactableObject;
        
        // Solo interactuar con el objeto para cocinar
        if (target != null && target != interactionHandler.controller.gameObject)
        {
            // Transformacion de carne
            if (target.name == "Meat Machine" && gameObject.tag == "Carne")
            {
                if (target.GetComponent<SpawningMeat>().SpawnMeat())
                {
                    Destroy(gameObject);                    
                }
            }


            // Cocina
            CookIngredients targetCook = target.GetComponent<CookIngredients>();
            CookIngredients thisCook = GetComponent<CookIngredients>();
            if (targetCook != null && thisCook != null)
            {
                print("Se esta intentando cocinar algo");
                // Fusionar los ingredientes de ambos
                bool fused = false;
                
                foreach (var ingredient in thisCook.ingredientIDs)
                {
                    if (targetCook.checkForRepeatedIngredients(ingredient))
                    {
                        print("Se fusiono");
                        fused = true;
                    }
                }
                
                if (fused)
                {
                    print("la ptm");
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

        foreach (Transform child in handler.Hands.transform)
        {
            child.SetParent(null, true);
            var rb_ = child.GetComponent<Rigidbody>();
            if (rb_ != null)
            {
                rb_.isKinematic = false;
                //rb_.AddForce(interactor.transform.forward * 2f, ForceMode.Impulse);
            }
        }
        
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
