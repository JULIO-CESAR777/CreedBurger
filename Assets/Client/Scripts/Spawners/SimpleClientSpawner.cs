using System.Collections;
using UnityEngine;

public class SimpleClientSpawner : MonoBehaviour
{
    [Header("Prefab del cliente (debe tener MoveClient)")]
    [SerializeField] private GameObject clientPrefab;

    [Header("Spawn")]
    [SerializeField] private Transform[] spawnPoints;   // opcional; si está vacío usa la pos del spawner
    [SerializeField] private float spawnInterval = 10f; // segundos entre spawns
    [SerializeField] private int maxClients = 5;        // tope total en escena

    private WaitForSeconds wait;

    private void Awake()
    {
        wait = new WaitForSeconds(spawnInterval);
    }

    private void Start()
    {
        if (clientPrefab == null)
        {
            Debug.LogError("[SimpleClientSpawner] Asigna un clientPrefab con MoveClient.");
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
        // Unity 6 / 2023+: cuenta sólo activos
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

        var go = Instantiate(clientPrefab, p.position, p.rotation);

        // Aviso útil si el prefab no tiene MoveClient
        if (go.GetComponent<MoveClient>() == null)
        {
            Debug.LogWarning("[SimpleClientSpawner] El prefab instanciado no tiene MoveClient.");
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, 0.25f);
    }
}
