using UnityEngine;

namespace CreedBurger.Ingredients
{
    [CreateAssetMenu(
        fileName = "Ingredient_",
        menuName = "Creed Burger/Ingredients/Ingredient Data")]
    public sealed class IngredientData : ScriptableObject
    {
        [SerializeField] private string ingredientId;
        [SerializeField] private string displayName;
        [SerializeField] private Sprite icon;
        [SerializeField] private Ingredient prefab;

        public string IngredientId => ingredientId;
        public string DisplayName => displayName;
        public Sprite Icon => icon;
        public Ingredient Prefab => prefab;
    }
}