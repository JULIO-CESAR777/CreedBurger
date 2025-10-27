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
        if (other.CompareTag("Knight"))
        {
            InMap = true;
            return;
        }

        var cliente = other.GetComponent<MoveClient>();
        if (cliente == null) return;
        if (cliente.estadoActual != MoveClient.Estado.Asustado) return;

        // Límite de caballeros
        if (GetKnightCount() >= maxKnightsEnMapa) return;

        // --- clave: contar ANTES de instanciar ---
        int preCount = GetKnightCount();

        // 1) Instanciar
        Vector3 spawnPos = spawnPointCaballero ? spawnPointCaballero.position : transform.position;
        Quaternion spawnRot = spawnPointCaballero ? spawnPointCaballero.rotation : Quaternion.identity;

        var go = Instantiate(knightPrefab, spawnPos, spawnRot);

        // Música: si antes no había ninguno, acaba de entrar el primero
        if (preCount == 0)
        {
            AudioManager.I.PlayMusic("music_alarm");
        }

        // 2) Ajustar al NavMesh
        if (NavMesh.SamplePosition(spawnPos, out var hit, 2f, NavMesh.AllAreas))
        {
            var agent = go.GetComponent<NavMeshAgent>();
            if (agent) agent.Warp(hit.position);
        }
        else
        {
            Debug.LogWarning($"{name}: No se encontró NavMesh cerca del punto de spawn.");
        }

        // 3) Inicializar waypoints
        var mk = go.GetComponent<MoveKnight>();
        if (mk)
            mk.Initialize(puntosAleatorios, puntoSalida, maxPuntosAntesDeSalir);
        else
            Debug.LogError("El prefab del caballero no tiene MoveKnight.");

        InMap = true;
    }


    private int GetKnightCount()
    {
        // Aseg�rate que el prefab tenga Tag "Knight"
        return GameObject.FindGameObjectsWithTag("Knight").Length;
    }
}
