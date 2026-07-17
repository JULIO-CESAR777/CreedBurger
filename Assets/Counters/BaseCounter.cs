using CreedBurger.Ingredients;
using CreedBurger.Interaction;
using CreedBurger.Recipes;
using UnityEngine;

namespace CreedBurger.Counters
{
    [RequireComponent(typeof(Collider))]
    public class BaseCounter : MonoBehaviour, IInteractable
    {
        [SerializeField] private Transform ingredientAnchor;
        [SerializeField] private RecipeBook recipeBook;

        protected RecipeBook RecipeBook => recipeBook;
        
        protected Ingredient StoredIngredient { get; private set; }

        protected Transform IngredientAnchor =>
            ingredientAnchor != null ? ingredientAnchor : transform;

        public virtual void Interact(PlayerInteractor player)
        {
            if (player == null)
            {
                return;
            }

            if (!player.HasHeldIngredient)
            {
                TryGiveIngredientTo(player);
                return;
            }

            if (StoredIngredient == null)
            {
                TryStoreHeldIngredient(player);
                return;
            }

            TryCombineStoredAndHeldIngredient(player);
        }

        private bool TryGiveIngredientTo(PlayerInteractor player)
        {
            if (StoredIngredient == null ||
                !player.TryHoldIngredient(StoredIngredient))
            {
                return false;
            }

            StoredIngredient = null;
            return true;
        }

        private bool TryStoreHeldIngredient(PlayerInteractor player)
        {
            if (!player.TryTakeHeldIngredient(out Ingredient ingredient))
            {
                return false;
            }

            StoreIngredient(ingredient);
            return true;
        }

        private bool TryCombineStoredAndHeldIngredient(PlayerInteractor player)
        {
            Ingredient heldIngredient = player.HeldIngredient;

            if (StoredIngredient == null ||
                heldIngredient == null ||
                recipeBook == null ||
                !TryResolveRecipe(
                    StoredIngredient.Data,
                    heldIngredient.Data,
                    out IngredientData resultData) ||
                !IngredientFactory.TryCreate(
                    resultData,
                    IngredientAnchor.position,
                    IngredientAnchor.rotation,
                    out Ingredient resultIngredient))
            {
                return false;
            }

            player.TryTakeHeldIngredient(out Ingredient consumedHeldIngredient);

            Destroy(StoredIngredient.gameObject);
            Destroy(consumedHeldIngredient.gameObject);

            StoredIngredient = null;
            StoreIngredient(resultIngredient);
            return true;
        }

        protected void StoreIngredient(Ingredient ingredient)
        {
            StoredIngredient = ingredient;
            ingredient.PlaceOn(IngredientAnchor);
        }
        
        protected virtual bool TryResolveRecipe(
            IngredientData first,
            IngredientData second,
            out IngredientData result)
        {
            result = null;
            return RecipeBook != null &&
                   RecipeBook.TryGetResult(first, second, out result);
        }
        
        protected bool TryReplaceStoredIngredient(IngredientData resultData)
        {
            if (StoredIngredient == null ||
                !IngredientFactory.TryCreate(
                    resultData,
                    IngredientAnchor.position,
                    IngredientAnchor.rotation,
                    out Ingredient resultIngredient))
            {
                return false;
            }

            Destroy(StoredIngredient.gameObject);
            StoreIngredient(resultIngredient);
            return true;
        }
    }
}