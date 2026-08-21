using CreedBurger.Ingredients;
using UnityEngine;

namespace CreedBurger.Recipes
{
    [CreateAssetMenu(
        fileName = "Recipe_",
        menuName = "Creed Burger/Recipes/Recipe")]
    public sealed class RecipeDefinition : ScriptableObject
    {
        [SerializeField] private IngredientData ingredientA;
        [SerializeField] private IngredientData ingredientB;

        [Tooltip("Vacío para una receta general. Ejemplo: cutting_board.")]
        [SerializeField] private string requiredStationId;

        [SerializeField, Min(0.1f)]
        private float processingDuration = 1f;

        public float ProcessingDuration => processingDuration;
        
        [SerializeField] private IngredientData result;

        public IngredientData Result => result;

        public bool Matches(
            IngredientData first,
            IngredientData second,
            string stationId)
        {
            if (first == null || result == null)
            {
                return false;
            }

            bool stationMatches = string.IsNullOrWhiteSpace(requiredStationId)
                ? string.IsNullOrWhiteSpace(stationId)
                : requiredStationId == stationId;

            if (!stationMatches)
            {
                return false;
            }

            return (ingredientA == first && ingredientB == second) ||
                   (ingredientA == second && ingredientB == first);
        }
    }
}