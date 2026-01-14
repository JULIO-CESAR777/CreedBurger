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
            ingredientIDs.Add(ing.id);
            currentComboID += ing.id;
        }
        
    }

    public bool checkForRepeatedIngredients(int id)
    {
        return ingredientIDs.Contains(id)? false : true;
    }
    
    
    public void TryAddIngredient(CookIngredients cook)
    {
        // TODO: Revisar si tiene ingredientes repetidos, solo aceptando los IDs: 1 2 4 8 16
        // No es necesario revisar los IDs especificos dado que siempre van a comenzar con esos IDs
        foreach (var IDs in cook.ingredientIDs)
        {
            foreach (var OwnIDs in ingredientIDs)
            {
                if (IDs == OwnIDs)
                {
                    //print("Hay un ID repetido");
                    return;
                }
            }
        }
        
        // TODO: Agregar numeros a la lista de IDs
        foreach (var IDs in cook.ingredientIDs)
        {
            //print("Se agregan los IDs: " + IDs);
            ingredientIDs.Add(IDs);
        }
        
        // TODO: Suma el combo de IDs
        currentComboID = 0;
        foreach (var val in ingredientIDs)
        {
            currentComboID += val;
        }

        // Intentar actualizar visual si existe en la base de datos
        UpdateVisualFromDB();

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
