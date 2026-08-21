using CreedBurger.Interaction;
using UnityEngine;

namespace CreedBurger.Ingredients
{
    [RequireComponent(typeof(Collider))]
    public sealed class IngredientDispenser : MonoBehaviour, IInteractable
    {
        [SerializeField] private IngredientData ingredientToDispense;
        [SerializeField] private Transform spawnAnchor;

        public void Interact(PlayerInteractor player)
        {
            if (player == null || player.HasHeldIngredient)
            {
                return;
            }

            if (ingredientToDispense == null || ingredientToDispense.Prefab == null)
            {
                Debug.LogWarning($"{name}: falta asignar Ingredient Data o su prefab.");
                return;
            }

            Transform anchor = spawnAnchor != null ? spawnAnchor : transform;

            if (!IngredientFactory.TryCreate(
                    ingredientToDispense,
                    anchor.position,
                    anchor.rotation,
                    out Ingredient ingredient))
            {
                return;
            }

            ingredient.Initialize(ingredientToDispense);

            // Lo entrega directamente al jugador; no se acumulan ingredientes
            // ni se instancia nada si ya llevaba uno.
            if (!player.TryHoldIngredient(ingredient))
            {
                Destroy(ingredient.gameObject);
            }
        }
    }
}