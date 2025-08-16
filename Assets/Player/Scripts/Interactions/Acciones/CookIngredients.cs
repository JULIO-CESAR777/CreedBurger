using System;
using System.Collections.Generic;
using UnityEngine;

public class CookIngredients : MonoBehaviour, IInteractable
{
    
    [Header("Base de datos de combinaciones")]
    public IngredientPrefabDB prefabDB; // Asigna en Inspector

    [Header("IDs actuales de ingredientes")]
    public List<int> ingredientIDs = new List<int>();

    [Header("ID combinado actual")]
    public int currentComboID;

    private void Start()
    {
        if (ingredientIDs != null && ingredientIDs.Count > 0) return;
        
        // Si este objeto nace con un solo ingrediente, lo agrega
        Ingredient ing = GetComponent<Ingredient>();
        
        if (ing != null)
        {
            TryAddIngredient(ing.id);
        }
        
    }

    public bool TryAddIngredient(int id)
    {
        // Evita duplicados
        if (ingredientIDs.Contains(id))
            return false;

        ingredientIDs.Add(id);

        // Recalcular la suma total (comboID)
        currentComboID = 0;
        foreach (int val in ingredientIDs)
        {
            currentComboID += val;
        }

        // Intentar actualizar visual si existe en la base de datos
        UpdateVisualFromDB();

        return true;
    }

    private void UpdateVisualFromDB()
    {
        if (prefabDB == null) return;

        GameObject newPrefab = prefabDB.GetPrefab(currentComboID);
        if (newPrefab != null)
        {
            Vector3 pos = transform.position;
            Quaternion rot = transform.rotation;

            // Instanciar primero
            GameObject result = Instantiate(newPrefab, pos, rot);

            // Copiar estado al nuevo
            var resultCook = result.GetComponent<CookIngredients>();
            if (resultCook != null)
            {
                resultCook.prefabDB = this.prefabDB; // MUY IMPORTANTE
                resultCook.ingredientIDs = new List<int>(this.ingredientIDs);
                resultCook.currentComboID = this.currentComboID;
            }

            // Destruir 'este' al final
            Destroy(gameObject);
        }
    }

    public void Interact(GameObject interactor) {}
    
    public InteractType GetInteractType() => InteractType.Cook;
}
