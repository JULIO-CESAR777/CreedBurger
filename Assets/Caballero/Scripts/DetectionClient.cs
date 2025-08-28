using UnityEngine;
using UnityEngine.AI;

public class DetectionClient : MonoBehaviour
{
    [Header("Spawn")]
    public GameObject knightPrefab;          // Prefab del caballero a instanciar
    public Transform spawnPointCaballero;    // Dónde aparecerá
    public int maxPuntosAntesDeSalir = 5;

    [Header("Ruta del caballero")]
    public Transform[] puntosAleatorios;     // Asigna en el Inspector
    public Transform puntoSalida;            // Asigna en el Inspector

    private void OnTriggerEnter(Collider other)
    {
        var cliente = other.GetComponent<MoveClient>();
        if (cliente == null) return;

        // Solo spawnea si el cliente está "asustado"
        if (cliente.estadoActual != MoveClient.Estado.Asustado) return;

        // 1) Instanciar
        Vector3 spawnPos = spawnPointCaballero != null ? spawnPointCaballero.position : transform.position;
        Quaternion spawnRot = spawnPointCaballero != null ? spawnPointCaballero.rotation : Quaternion.identity;

        var go = Instantiate(knightPrefab, spawnPos, spawnRot);

        // 2) Asegurar que cae dentro del NavMesh
        if (NavMesh.SamplePosition(spawnPos, out var hit, 2f, NavMesh.AllAreas))
        {
            var agent = go.GetComponent<NavMeshAgent>();
            if (agent != null) agent.Warp(hit.position);
        }
        else
        {
            Debug.LogWarning($"{name}: No se encontró NavMesh cerca del punto de spawn.");
        }

        // 3) Pasar waypoints y salida al caballero
        var mk = go.GetComponent<MoveKnight>();
        if (mk != null)
        {
            mk.Initialize(puntosAleatorios, puntoSalida, maxPuntosAntesDeSalir);
        }
        else
        {
            Debug.LogError("El prefab del caballero no tiene MoveKnight.");
        }
    }
}
