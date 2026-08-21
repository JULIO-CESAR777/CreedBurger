using CreedBurger.Ingredients;
using CreedBurger.Interaction;
using UnityEngine;

namespace CreedBurger.Counters
{
    public class ProcessingCounter : BaseCounter
    {
        [Tooltip("Vacío para usar el ID predeterminado de la estación.")]
        [SerializeField] private string stationIdOverride;

        protected virtual bool ProcessOnInteract => true;
        protected virtual string DefaultStationId => "processing";

        protected string StationId =>
            string.IsNullOrWhiteSpace(stationIdOverride)
                ? DefaultStationId
                : stationIdOverride;

        public override void Interact(PlayerInteractor player)
        {
            if (ProcessOnInteract &&
                player != null &&
                !player.HasHeldIngredient &&
                StoredIngredient != null &&
                TryProcessStoredIngredient())
            {
                return;
            }

            base.Interact(player);
        }

        protected override bool TryResolveRecipe(
            IngredientData first,
            IngredientData second,
            out IngredientData result)
        {
            result = null;

            if (RecipeBook != null &&
                RecipeBook.TryGetResult(first, second, StationId, out result))
            {
                return true;
            }

            return base.TryResolveRecipe(first, second, out result);
        }

        protected bool CanProcessStoredIngredient(out IngredientData result)
        {
            result = null;

            return StoredIngredient != null &&
                   RecipeBook != null &&
                   RecipeBook.TryGetResult(
                       StoredIngredient.Data,
                       null,
                       StationId,
                       out result);
        }

        protected virtual bool TryProcessStoredIngredient()
        {
            return CanProcessStoredIngredient(out IngredientData result) &&
                   TryReplaceStoredIngredient(result);
        }
    }
}