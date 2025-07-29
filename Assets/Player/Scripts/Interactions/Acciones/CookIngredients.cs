using System;
using System.Collections.Generic;
using UnityEngine;

public class CookIngredients : MonoBehaviour, IInteractable
{
    
    public List<IngredientType> currentIngredients = new List<IngredientType>();

    private void Start()
    {
        TryAddIngredient(GetComponent<Ingredient>().type);
    }

    public bool TryAddIngredient(IngredientType type)
    {
        // Si ya está, no se agrega
        if (currentIngredients.Contains(type))
            return false;
        currentIngredients.Add(type);
        return true;
    }

    public bool CanCookSandwich()
    {
        return currentIngredients.Contains(IngredientType.Bread) &&
               currentIngredients.Contains(IngredientType.Lettuce) &&
               currentIngredients.Contains(IngredientType.Meat);
    }

    public void Cook()
    {
        Debug.Log("¡Sandwich cocinado!");
        // Aquí tu lógica para crear el sandwich y/o destruir ingredientes
        currentIngredients.Clear();
    }

    public void Interact(GameObject interactor)
    {
    }
    
    public InteractType GetInteractType() => InteractType.Clean;
}
