using System.Collections;
using UnityEngine;

public class ClientSpawnerAssignRefs : MonoBehaviour
{
    [Header("Prefab del cliente (MoveClient)")]
    [SerializeField] private GameObject clientPrefab;

    [Header("Spawn")]
    [SerializeField] private Transform[] spawnPoints;
    [SerializeField] private float spawnInterval = 10f;
    [SerializeField] private int maxClients = 5;

    [Header("Referencias del restaurante (se asignan a cada cliente)")]
    [SerializeField] private Transform puntoSalida;
    [SerializeField] private Transform[] puntosAleatorios;
    [SerializeField] private Transform mesasRoot;     // opcional: padre con Seat_* (cada uno con MesaSeat)
    [SerializeField] private Transform[] mesas;       // o arrastra aquí los Seat_* directamente

    [Header("Utilidad")]
    [SerializeField] private bool assignToExistingOnStart = true; // asigna refs al cliente que ya tenías en la escena

    private WaitForSeconds wait;

    private void Awake()
    {
        wait = new WaitForSeconds(spawnInterval);
    }

    private void Start()
    {
        if (assignToExistingOnStart)
        {
            // Asigna referencias a cualquier MoveClient ya presente en la escena
            var existentes = Object.FindObjectsByType<MoveClient>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);
            foreach (var mc in existentes) AssignRefs(mc);
        }

        StartCoroutine(SpawnLoop());
    }

    private IEnumerator SpawnLoop()
    {
        while (true)
        {
            int count = CountClients();
            if (count < maxClients)
                SpawnOne();

            yield return wait;
        }
    }

    private int CountClients()
    {
        var arr = Object.FindObjectsByType<MoveClient>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);
        return arr.Length;
    }

    private void SpawnOne()
    {
        if (clientPrefab == null)
        {
            Debug.LogWarning("[ClientSpawnerAssignRefs] Falta clientPrefab.");
            return;
        }

        Transform p = transform;
        if (spawnPoints != null && spawnPoints.Length > 0)
            p = spawnPoints[Random.Range(0, spawnPoints.Length)];

        var go = Instantiate(clientPrefab, p.position, p.rotation);
        var mc = go.GetComponent<MoveClient>();
        if (mc == null)
        {
            Debug.LogError("[ClientSpawnerAssignRefs] El prefab no tiene MoveClient.");
            return;
        }

        AssignRefs(mc);
    }

    private void AssignRefs(MoveClient mc)
    {
        // punto de salida
        if (mc.puntoSalida == null && puntoSalida != null)
            mc.puntoSalida = puntoSalida;

        // puntos aleatorios
        if ((mc.puntosAleatorios == null || mc.puntosAleatorios.Length == 0))
            mc.puntosAleatorios = puntosAleatorios;

        // mesas (Seats con MesaSeat)
        if (mc.mesas == null || mc.mesas.Length == 0)
        {
            // prioridad: arreglo explícito
            if (mesas != null && mesas.Length > 0)
            {
                mc.mesas = mesas;
            }
            // alternativa: recolectar desde un root
            else if (mesasRoot != null)
            {
                var seats = mesasRoot.GetComponentsInChildren<MesaSeat>(true);
                Transform[] arr = new Transform[seats.Length];
                for (int i = 0; i < seats.Length; i++)
                    arr[i] = seats[i].transform;
                mc.mesas = arr;
            }
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, 0.25f);
    }
}
