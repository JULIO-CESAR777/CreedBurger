using UnityEngine;
using System;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "IngredientPrefabDB", menuName = "Cooking /Ingredient Prefab DB")]
public class IngredientPrefabDB : ScriptableObject
{
    [Serializable]
    public struct Entry
    {
        public int id;              // ID único o suma de IDs
        public String title;
        public GameObject prefab;   // Prefab asociado
        public Sprite image;
    }

    public List<Entry> entries = new List<Entry>();

    private Dictionary<int, GameObject> _map;

    public void BuildIndex()
    {
        _map = new Dictionary<int, GameObject>();
        foreach (var e in entries)
        {
            if (!_map.ContainsKey(e.id))
                _map.Add(e.id, e.prefab);
        }
    }

    public GameObject GetPrefab(int id)
    {
        if (_map == null) BuildIndex();
        _map.TryGetValue(id, out var prefab);
        return prefab;
    }    
    
    // En IngredientPrefabDB
    public void OnValidate()
    {
        var seen = new HashSet<int>();
        foreach (var e in entries)
        {
            if (e.id == 0)
                //Debug.LogWarning($"[DB] Entrada con ID 0 (no recomendado).", this);
            if (e.prefab == null)
                Debug.LogWarning($"[DB] Entrada {e.id} sin prefab.", this);
            if (!seen.Add(e.id))
                Debug.LogError($"[DB] ID duplicado: {e.id}", this);
        }
    }
    
}
