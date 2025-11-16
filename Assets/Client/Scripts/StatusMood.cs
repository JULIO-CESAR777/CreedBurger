using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
[DefaultExecutionOrder(-100)]
public class StatusMood : MonoBehaviour
{
    private static readonly Dictionary<int, StatusMood> registry = new();

    public MoveClient moveClient;

    [Header("Prefabs")]
    public GameObject prefabFeliz;
    public GameObject prefabTriste;
    public GameObject prefabAlerta;
    public GameObject prefabSospechando;
    public GameObject prefabComiendo;

    [Header("Colocación")]
    public float yOffset = 2f;
    public Transform spawnParent; // si es null, usa el root del cliente

    private enum Mood { None, Feliz, Triste, Alerta, Sospechando, Comiendo }
    private Mood lastMood = Mood.None;

    private Transform anchor;
    private readonly Dictionary<Mood, GameObject> instances = new();

    private int ownerId;
    private bool built = false; // <— candado
    private const string AnchorName = "_MoodAnchor";

    void Awake()
    {
        if (moveClient == null) moveClient = GetComponentInParent<MoveClient>();
        if (moveClient == null) { enabled = false; return; }

        ownerId = moveClient.GetInstanceID();

        // Solo 1 StatusMood por cliente
        if (registry.TryGetValue(ownerId, out var ex) && ex != null && ex != this)
        {
            Debug.LogWarning($"[StatusMood] '{moveClient.name}' ya tiene HUD. Deshabilitando duplicado en '{name}'.");
            enabled = false;
            return;
        }
        registry[ownerId] = this;

        if (!built) BuildOnce();
    }

   

    private void BuildOnce()
    {
        if (built) return; // <— candado
        built = true;

        if (spawnParent == null) spawnParent = moveClient.transform;

        // Asegurar un solo anchor (si hubiera más, destruye extra)
        anchor = EnsureSingleAnchor(spawnParent);

        // Reusar si ya hay caras dentro del anchor (por alguna razón ya estaban)
        ReuseIfPresent(Mood.Feliz,        prefabFeliz);
        ReuseIfPresent(Mood.Triste,       prefabTriste);
        ReuseIfPresent(Mood.Alerta,       prefabAlerta);
        ReuseIfPresent(Mood.Sospechando,  prefabSospechando);
        ReuseIfPresent(Mood.Comiendo,     prefabComiendo);

        // Crear las que falten (una sola vez)
        CreateIfMissing(Mood.Feliz,        prefabFeliz);
        CreateIfMissing(Mood.Triste,       prefabTriste);
        CreateIfMissing(Mood.Alerta,       prefabAlerta);
        CreateIfMissing(Mood.Sospechando,  prefabSospechando);
        CreateIfMissing(Mood.Comiendo,     prefabComiendo);

        // Apagar todas al inicio
        SetMood(Mood.None);

        // Barrer duplicados por nombre dentro del anchor (por si otro script instanció)
        KillDuplicatesByName(anchor);
    }

    void Update()
    {
        if (moveClient == null) return;
        var mood = MapState(moveClient.estadoActual);
        if (mood != lastMood) SetMood(mood);
    }

    void OnDestroy()
    {
        if (registry.TryGetValue(ownerId, out var self) && self == this)
            registry.Remove(ownerId);
    }

    // ---------- helpers ----------
    private void SetMood(Mood mood)
    {
        foreach (var kv in instances)
            if (kv.Value != null) kv.Value.SetActive(false);

        if (instances.TryGetValue(mood, out var go) && go != null)
            go.SetActive(true);

        lastMood = mood;
    }

    private static Mood MapState(MoveClient.Estado s)
    {
        switch (s)
        {
            case MoveClient.Estado.IrMesa:
            case MoveClient.Estado.EsperaPedido:
            case MoveClient.Estado.IrAleatorio:
            case MoveClient.Estado.EsperaAleatorio:
            case MoveClient.Estado.IrSalida:   return Mood.Feliz;
            case MoveClient.Estado.Quieto:     return Mood.Triste;
            case MoveClient.Estado.Aturdido:
            case MoveClient.Estado.Asustado:   return Mood.Alerta;
            case MoveClient.Estado.Sospechando:return Mood.Sospechando;
            case MoveClient.Estado.Comer:      return Mood.Comiendo;
            default:                            return Mood.None;
        }
    }

    private Transform EnsureSingleAnchor(Transform parent)
    {
        Transform first = null;
        var toDestroy = new List<GameObject>();
        foreach (Transform c in parent)
        {
            if (c.name == AnchorName)
            {
                if (first == null) first = c;
                else toDestroy.Add(c.gameObject);
            }
        }
        foreach (var go in toDestroy) Destroy(go);

        if (first == null)
        {
            first = new GameObject(AnchorName).transform;
            first.SetParent(parent, worldPositionStays: false);
        }

        first.localPosition = Vector3.up * yOffset;
        first.localRotation = Quaternion.identity;
        first.localScale    = Vector3.one;
        return first;
    }

    private void ReuseIfPresent(Mood mood, GameObject prefab)
    {
        if (prefab == null) return;
        // Busca hijos cuyo nombre contenga el nombre de prefab
        string key = prefab.name;
        GameObject found = null;
        foreach (Transform child in anchor)
        {
            if (child.name.Contains(key))
            {
                if (found == null) found = child.gameObject;
                else Destroy(child.gameObject); // si hay más de uno, elimina extra
            }
        }
        if (found != null)
        {
            found.SetActive(false);
            instances[mood] = found;
        }
    }

    private void CreateIfMissing(Mood mood, GameObject prefab)
    {
        if (prefab == null) return;
        if (instances.ContainsKey(mood) && instances[mood] != null) return;

        var go = Instantiate(prefab, anchor.position, Quaternion.identity, anchor);
        go.name = prefab.name + "(Clone)"; // nombre limpio y consistente
        go.SetActive(false);
        instances[mood] = go;
    }

    private void KillDuplicatesByName(Transform parent)
    {
        // Si por cualquier motivo quedaron 2 con el mismo nombre, deja solo 1
        var seen = new HashSet<string>();
        var toKill = new List<GameObject>();
        foreach (Transform c in parent)
        {
            string n = c.name;
            if (!seen.Add(n)) toKill.Add(c.gameObject);
        }
        foreach (var go in toKill) Destroy(go);
    }
}
