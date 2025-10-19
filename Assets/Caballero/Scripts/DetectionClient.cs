using UnityEngine;
using UnityEngine.AI;

public class DetectionClient : MonoBehaviour
{
    [Header("Spawn")]
    public GameObject knightPrefab;          // Prefab del caballero a instanciar
    public Transform spawnPointCaballero;    // D�nde aparecer�
    public int maxPuntosAntesDeSalir = 5;

    [Header("Ruta del caballero")]
    public Transform[] puntosAleatorios;     // Asigna en el Inspector
    public Transform puntoSalida;

    // Asigna en el Inspector
    public int maxKnightsEnMapa = 1;
    public bool InMap;

    private void Start()
    {
        InMap = false;
    }
    private void OnTriggerEnter(Collider other)
    {
        // Si entra un knight, s�lo actualiza el flag y sal
        if (other.CompareTag("Knight"))
        {
            InMap = true;  // hay al menos un knight en el trigger
            return;
        }

        // S�lo nos interesa cuando entra un cliente
        var cliente = other.GetComponent<MoveClient>();
        if (cliente == null) return;

        // Condici�n de estado
        if (cliente.estadoActual != MoveClient.Estado.Asustado) return;

        // L�MITE: si ya hay suficientes knights, no instancias
        if (GetKnightCount() >= maxKnightsEnMapa) return;

        // 1) Instanciar
        Vector3 spawnPos = spawnPointCaballero != null ? spawnPointCaballero.position : transform.position;
        Quaternion spawnRot = spawnPointCaballero != null ? spawnPointCaballero.rotation : Quaternion.identity;

        var go = Instantiate(knightPrefab, spawnPos, spawnRot);

        // 2) Ajustar al NavMesh
        if (NavMesh.SamplePosition(spawnPos, out var hit, 2f, NavMesh.AllAreas))
        {
            var agent = go.GetComponent<NavMeshAgent>();
            if (agent != null) agent.Warp(hit.position);
        }
        else
        {
            Debug.LogWarning($"{name}: No se encontr� NavMesh cerca del punto de spawn.");
        }

        // 3) Inicializar waypoints
        var mk = go.GetComponent<MoveKnight>();
        if (mk != null)
        {
            mk.Initialize(puntosAleatorios, puntoSalida, maxPuntosAntesDeSalir);
        }
        else
        {
            Debug.LogError("El prefab del caballero no tiene MoveKnight.");
        }
        AudioManager.I.PlayMusic("music_alarm");
        InMap = true; // ahora seguro hay uno en el mapa
    }

    private int GetKnightCount()
    {
        // Aseg�rate que el prefab tenga Tag "Knight"
        return GameObject.FindGameObjectsWithTag("Knight").Length;
    }
}
