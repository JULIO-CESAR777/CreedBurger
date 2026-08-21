using CreedBurger.Ingredients;
using CreedBurger.Recipes;
using UnityEngine;

namespace CreedBurger.Counters
{
    public sealed class PanCounter : ProcessingCounter
    {
        private float cookingTimer;

        public float Progress01 { get; private set; }

        protected override string DefaultStationId => "pan";

        // La sartén procesa automáticamente en Update, no al pulsar Interact.
        protected override bool ProcessOnInteract => false;

        private void Update()
        {
            if (!TryGetCurrentProcessingRecipe(out RecipeDefinition recipe))
            {
                ResetProgress();
                return;
            }

            cookingTimer += Time.deltaTime;
            Progress01 = Mathf.Clamp01(
                cookingTimer / recipe.ProcessingDuration);

            if (cookingTimer < recipe.ProcessingDuration)
            {
                return;
            }

            if (TryProcessStoredIngredient())
            {
                ResetProgress();
            }
        }

        private bool TryGetCurrentProcessingRecipe(
            out RecipeDefinition recipe)
        {
            recipe = null;

            return StoredIngredient != null &&
                   RecipeBook != null &&
                   RecipeBook.TryGetRecipe(
                       StoredIngredient.Data,
                       null,
                       StationId,
                       out recipe);
        }

        private void ResetProgress()
        {
            cookingTimer = 0f;
            Progress01 = 0f;
        }
    }
}