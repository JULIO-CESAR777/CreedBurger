using UnityEngine;

namespace CreedBurger.Ingredients
{
    public static class IngredientFactory
    {
        public static bool TryCreate(
            IngredientData data,
            Vector3 position,
            Quaternion rotation,
            out Ingredient ingredient)
        {
            ingredient = null;

            if (data == null || data.Prefab == null)
            {
                Debug.LogWarning("No se puede crear un ingrediente sin datos y prefab.");
                return false;
            }

            ingredient = Object.Instantiate(data.Prefab, position, rotation);
            ingredient.Initialize(data);
            return true;
        }
    }
}