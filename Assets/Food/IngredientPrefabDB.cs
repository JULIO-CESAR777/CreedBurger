using UnityEngine;
using System;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "IngredientPrefabDB", menuName = "Cooking/Ingredient Prefab DB")]
public class IngredientPrefabDB : ScriptableObject
{
    [Serializable]
    public struct Entry
    {
        public int id;              // ID único
        public string title;        // (no usaremos en UI, opcional)
        public GameObject prefab;   // opcional
        public Sprite image;        // Sprite a mostrar en UI
        public Sprite imageResultado;
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
    // --- helpers ---
    public bool TryGetById(int id, out Entry entry)
    {
        for (int i = 0; i < entries.Count; i++)
            if (entries[i].id == id) { entry = entries[i]; return true; }
        entry = default;
        return false;
    }

    public bool TryGetRandom(out Entry entry)
    {
        entry = default;
        if (entries == null || entries.Count == 0) return false;
        entry = entries[UnityEngine.Random.Range(0, entries.Count)];
        return true;
    }

    public void OnValidate()
    {
        var seen = new HashSet<int>();
        foreach (var e in entries)
        {
            if (e.image == null)
                //Debug.LogWarning($"[DB] Entrada {e.id} no tiene image (UI mostrará vacío).", this);
            if (!seen.Add(e.id))
                Debug.LogError($"[DB] ID duplicado: {e.id}", this);
        }
    }
    
    public GameObject GetPrefab(int id)
    {
        if (_map == null) BuildIndex();
        _map.TryGetValue(id, out var prefab);
        return prefab;
    }   
}