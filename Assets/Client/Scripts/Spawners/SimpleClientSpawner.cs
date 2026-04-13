using System.Collections;
using UnityEngine;

public class SimpleClientSpawner : MonoBehaviour
{
    [Header("Prefabs de clientes")]
    [SerializeField] private GameObject normalClientPrefab;
    [SerializeField] private GameObject rareClientPrefab;

    [Range(0f, 1f)]
    [SerializeField] private float rareSpawnChance = 0.1f; // 10%

    [Header("Spawn")]
    [SerializeField] private Transform[] spawnPoints;
    [SerializeField] private float spawnInterval = 10f;
    [SerializeField] private int maxClients = 5;

    [Header("Asignación manual para MoveClient")]
    [SerializeField] private Transform puntoSalida;
    [SerializeField] private Transform[] puntosAleatorios;
    [SerializeField] private Transform[] mesas;

    private WaitForSeconds wait;

    private void Awake()
    {
        wait = new WaitForSeconds(spawnInterval);
    }

    private void Start()
    {
        if (normalClientPrefab == null)
        {
            Debug.LogError("[SimpleClientSpawner] Asigna el normalClientPrefab con MoveClient.");
            enabled = false;
            return;
        }

        StartCoroutine(SpawnLoop());
    }

    private IEnumerator SpawnLoop()
    {
        while (true)
        {
            if (CountClients() < maxClients)
            {
                SpawnOne();
            }

            yield return wait;
        }
    }

    private int CountClients()
    {
        var arr = Object.FindObjectsByType<MoveClient>(
            FindObjectsInactive.Exclude,
            FindObjectsSortMode.None
        );
        return arr.Length;
    }

    private void SpawnOne()
    {
        Transform p = transform;

        if (spawnPoints != null && spawnPoints.Length > 0)
        {
            p = spawnPoints[Random.Range(0, spawnPoints.Length)];
        }

        GameObject prefabToSpawn = GetRandomClientPrefab();

        if (prefabToSpawn == null)
        {
            Debug.LogWarning("[SimpleClientSpawner] No hay prefab válido para spawnear.");
            return;
        }

        GameObject go = Instantiate(prefabToSpawn, p.position, p.rotation);

        MoveClient mc = go.GetComponent<MoveClient>();
        if (mc == null)
        {
            Debug.LogWarning("[SimpleClientSpawner] El prefab instanciado no tiene MoveClient.");
            return;
        }

        mc.puntoSalida = puntoSalida;
        mc.puntosAleatorios = puntosAleatorios;
        mc.mesas = mesas;
    }

    private GameObject GetRandomClientPrefab()
    {
        bool rareAlreadyExists = RareClientExists();

       
        if (rareClientPrefab != null && !rareAlreadyExists && Random.value < rareSpawnChance)
        {
            return rareClientPrefab;
        }

        return normalClientPrefab;
    }

    private bool RareClientExists()
    {
        GameObject[] allClients = GameObject.FindGameObjectsWithTag("Client");

        foreach (GameObject client in allClients)
        {
            if (client.name.Contains(rareClientPrefab.name))
            {
                return true;
            }
        }

        return false;
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, 0.25f);
    }
}