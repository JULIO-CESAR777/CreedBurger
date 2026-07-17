using CreedBurger.Ingredients;
using UnityEngine;

namespace CreedBurger.Recipes
{
    [CreateAssetMenu(
        fileName = "RecipeBook",
        menuName = "Creed Burger/Recipes/Recipe Book")]
    public sealed class RecipeBook : ScriptableObject
    {
        [SerializeField] private RecipeDefinition[] recipes;

        public bool TryGetResult(
            IngredientData first,
            IngredientData second,
            out IngredientData result)
        {
            return TryGetResult(first, second, string.Empty, out result);
        }

        public bool TryGetResult(
            IngredientData first,
            IngredientData second,
            string stationId,
            out IngredientData result)
        {
            result = null;

            if (!TryGetRecipe(first, second, stationId, out RecipeDefinition recipe))
            {
                return false;
            }

            result = recipe.Result;
            return true;
        }

        public bool TryGetRecipe(
            IngredientData first,
            IngredientData second,
            string stationId,
            out RecipeDefinition recipe)
        {
            recipe = null;

            if (first == null || recipes == null)
            {
                return false;
            }

            foreach (RecipeDefinition candidate in recipes)
            {
                if (candidate == null ||
                    !candidate.Matches(first, second, stationId))
                {
                    continue;
                }

                recipe = candidate;
                return true;
            }

            return false;
        }
    }
}