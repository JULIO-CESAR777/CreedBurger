using UnityEngine;
using UnityEngine.AI;
using System.Collections;

public class MoveClient : MonoBehaviour
{
    public NavMeshAgent agent;

    [Header("Puntos")]
    public Transform puntoSalida;
    public Transform[] puntosAleatorios;

    // NUEVO: mesas del restaurante (asigna en el Inspector)
    public Transform[] mesas;

    [Header("Velocidades")]
    public int miedoaaa = 4;

    [Header("Tiempos de espera")]
    public float esperaComer = 4f;      // tiempo "comiendo"
    public float esperaAleatorio = 2f;  // pausa en punto aleatorio
    public float esperaSangreVer = 5f;

    [Header("Detecci�n de Sangre Visual")]
    public float detectionRadius = 6f;
    [Range(0f, 360f)] public float fieldOfView = 120f;

    public GameObject prefabSangre;
    public GameObject prefabCarne;

    [Header("Cosas de la UI")]
    public IngredientPrefabDB db;
    public OrderUIController orderUI;

    private IngredientPrefabDB.Entry pedido;
    private bool pedidoEnviado = false;
    private bool pedidoListo = false;

    // --- ESTADOS ---
    public enum Estado
    {
        IrMesa,         // NUEVO: ir a mesa aleatoria
        EsperaPedido,   // NUEVO: sentado esperando que lleven el plato correcto
        Comer,          // NUEVO: anim/comer por X segundos
        IrAleatorio,
        EsperaAleatorio,
        IrSalida,
        Aturdido,
        Asustado,
        Sospechando,
        Terminado,
        Quieto
    }
    public Estado estadoActual = Estado.IrMesa;
    private Estado estadoPrevio;
    private Vector3 destinoPrevio;

    // Trabajo interno
    private Transform mesaAsignada;
    private Transform aleatorioSeleccionado;

    // Detecci�n de sangre
    private float bloodTimer = 0f;
    private bool isFocusingBlood = false;

    void Start()
    {
        if (agent == null) agent = GetComponent<NavMeshAgent>();

        // Asignar mesa inicial
        if (mesas != null && mesas.Length > 0)
        {
            mesaAsignada = mesas[Random.Range(0, mesas.Length)];
            IrAPunto(mesaAsignada);
            estadoActual = Estado.IrMesa;
        }
        else
        {
            Debug.LogWarning("[MoveClient] No hay mesas asignadas. Se ir� directo a pasear y luego salir.");
            PasarAPaseo();
        }
    }

    void Update()
    {
        DetectarSangreVisual();

        switch (estadoActual)
        {
            case Estado.IrMesa:
                if (HaLlegadoDestino())
                    SentarseYOrdenar();
                break;

            case Estado.EsperaPedido:
                // quieto esperando RecibirPedido(id)
                break;

            case Estado.Comer:
                // el tiempo corri� por corrutina; aqu� no hacemos nada
                break;

            case Estado.IrAleatorio:
                if (HaLlegadoDestino())
                {
                    estadoActual = Estado.EsperaAleatorio;
                    StartCoroutine(EsperaEnPunto(esperaAleatorio, Estado.IrSalida));
                }
                break;

            case Estado.IrSalida:
                if (HaLlegadoDestino())
                {
                    estadoActual = Estado.Terminado;
                    Destroy(gameObject);
                }
                break;

            case Estado.Aturdido:
                break;

            case Estado.Asustado:
                if (HaLlegadoDestino())
                {
                    estadoActual = Estado.Terminado;
                    Destroy(gameObject);
                }
                break;

            case Estado.Quieto:
                break;
        }
    }

    // === L�GICA PRINCIPAL ===

    private void SentarseYOrdenar()
    {
        // Detenerse en la mesa
        if (agent != null) agent.isStopped = true;

        // Elegir receta aleatoria y mostrar UI
        HacerPedido();

        // Esperar a que se entregue con RecibirPedido(...)
        estadoActual = Estado.EsperaPedido;
        Debug.Log("[MoveClient] Sentado en mesa y esperando el pedido...");
    }

    private void EmpezarAComer()
    {
        // Ya lleg� el plato correcto
        if (orderUI != null) orderUI.Hide();

        estadoActual = Estado.Comer;

        // Por si ven�amos parados
        if (agent != null)
        {
            agent.isStopped = true;
            agent.ResetPath();
        }

        // Simular comer X segundos y luego pasear
        StartCoroutine(EsperaEnPunto(esperaComer, Estado.IrAleatorio));
        Debug.Log("[MoveClient] Comiendo...");
    }

    private void PasarAPaseo()
    {
        if (puntosAleatorios == null || puntosAleatorios.Length == 0)
        {
            // Si no hay puntos para pasear, ir directo a salida
            estadoActual = Estado.IrSalida;
            IrAPunto(puntoSalida);
            return;
        }

        aleatorioSeleccionado = puntosAleatorios[Random.Range(0, puntosAleatorios.Length)];
        estadoActual = Estado.IrAleatorio;

        if (agent != null) agent.isStopped = false;
        IrAPunto(aleatorioSeleccionado);
        Debug.Log("[MoveClient] Dando una vuelta (IrAleatorio)...");
    }

    private void SalirDelLugar()
    {
        estadoActual = Estado.IrSalida;
        if (agent != null) agent.isStopped = false;
        IrAPunto(puntoSalida);
        Debug.Log("[MoveClient] Saliendo...");
    }

    // === PEDIDOS ===

    private string GetPedidoDisplayName()
    {
        if (pedido.prefab != null) return pedido.prefab.name;
        return $"Receta {pedido.id}";
    }

    void HacerPedido()
    {
        if (db == null || db.entries.Count == 0)
        {
            Debug.LogWarning("[MoveClient] DB vac�a; no se puede pedir. Simulando pedido gen�rico.");
            pedido = default;
        }
        else
        {
            //int index = Random.Range(0, db.entries.Count);
            pedido = db.entries[7];
        }

        pedidoEnviado = true;
        pedidoListo = false;

        if (orderUI != null) orderUI.ShowOrder(pedido);
        Debug.Log($"[MoveClient] Cliente pidi�: {GetPedidoDisplayName()} (id={pedido.id})");
    }

    // Llama tu "mesero/chef" cuando deje el plato en la mesa del cliente
    public void RecibirPedido(int idDelChef)
    {
        if (!pedidoEnviado)
        {
            Debug.LogWarning("[MoveClient] A�n no env�a pedido; no puede recibir.");
            return;
        }

        if (pedido.id == idDelChef)
        {
            pedidoListo = true;
            Debug.Log($"[MoveClient] Recibi� el pedido correcto: {GetPedidoDisplayName()}");

            if (estadoActual == Estado.EsperaPedido)
                EmpezarAComer();
        }
        else
        {
            Debug.LogWarning($"[MoveClient] Pedido equivocado (entregado={idDelChef}, esperado={pedido.id}).");
            // aqu� puedes: ignorar, esperar otro intento, enojarse, etc.
        }
    }

    // === UTILIDADES DE MOVIMIENTO/ESTADO ===

    IEnumerator EsperaEnPunto(float segundos, Estado siguiente)
    {
        yield return new WaitForSeconds(segundos);

        if (siguiente == Estado.IrAleatorio)
        {
            PasarAPaseo();
        }
        else if (siguiente == Estado.IrSalida)
        {
            SalirDelLugar();
        }
    }

    bool HaLlegadoDestino()
    {
        return !agent.pathPending &&
               agent.remainingDistance <= agent.stoppingDistance &&
               (!agent.hasPath || agent.velocity.sqrMagnitude == 0f);
    }

    void IrAPunto(Transform punto)
    {
        if (punto != null && agent != null)
            agent.SetDestination(punto.position);
    }

    // === REACCIONES / EVENTOS ===

    private void DetectarSangreVisual()
    {
        if (estadoActual == Estado.Asustado || estadoActual == Estado.Terminado)
            return;

        Collider[] hits = Physics.OverlapSphere(transform.position, detectionRadius);
        Transform targetBlood = null;
        bool sangreVisible = false;

        foreach (Collider hit in hits)
        {
            if (hit.CompareTag("Blood"))
            {
                Vector3 dir = (hit.transform.position - transform.position).normalized;
                float angulo = Vector3.Angle(transform.forward, dir);
                if (angulo <= fieldOfView * 0.5f)
                {
                    sangreVisible = true;
                    targetBlood = hit.transform;
                    break;
                }
            }
        }

        if (sangreVisible)
        {
            if (!isFocusingBlood)
            {
                isFocusingBlood = true;
                if (agent != null) agent.isStopped = true;
            }
            estadoActual = Estado.Sospechando;

            Vector3 lookDir = (targetBlood.position - transform.position).normalized;
            Quaternion lookRot = Quaternion.LookRotation(lookDir);
            transform.rotation = Quaternion.Slerp(transform.rotation, lookRot, Time.deltaTime * 2f);

            bloodTimer += Time.deltaTime;
            if (bloodTimer >= esperaSangreVer)
                EntrarEnMiedo();
        }
        else if (isFocusingBlood)
        {
            isFocusingBlood = false;
            bloodTimer = 0f;
            if (agent != null) agent.isStopped = false;
        }
        else
        {
            bloodTimer = 0f;
        }
    }

    private void EntrarEnMiedo()
    {
        estadoActual = Estado.Asustado;
        agent.ResetPath();
        agent.speed = miedoaaa;
        agent.SetDestination(puntoSalida.position);
    }

    public void Aturdir(float duracion)
    {
        if (estadoActual == Estado.Aturdido) return;

        estadoPrevio = estadoActual;
        destinoPrevio = agent.destination;
        estadoActual = Estado.Aturdido;

        agent.ResetPath();
        StartCoroutine(RecuperarDeAturdimiento(duracion));
    }

    private IEnumerator RecuperarDeAturdimiento(float segundos)
    {
        yield return new WaitForSeconds(segundos);
        estadoActual = estadoPrevio;

        // Si estaba sentado esperando/comiendo, seguir sentado (no retomar path)
        if (estadoActual == Estado.EsperaPedido || estadoActual == Estado.Comer)
        {
            agent.isStopped = true;
        }
        else
        {
            agent.SetDestination(destinoPrevio);
        }
    }

    public void Alto()
    {
        estadoActual = Estado.Quieto;
        agent.ResetPath();
        agent.velocity = Vector3.zero;
    }

    public void Morir()
    {
        Vector3 spawnCosas = transform.position + Vector3.up * 2;
        Instantiate(prefabCarne, spawnCosas, transform.rotation);
        Instantiate(prefabSangre, transform.position, Quaternion.Euler(90, 0, 0));
        Destroy(gameObject);
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);
        Vector3 dirA = Quaternion.Euler(0, fieldOfView * 0.5f, 0) * transform.forward;
        Vector3 dirB = Quaternion.Euler(0, -fieldOfView * 0.5f, 0) * transform.forward;
        Gizmos.DrawLine(transform.position, transform.position + dirA * detectionRadius);
        Gizmos.DrawLine(transform.position, transform.position + dirB * detectionRadius);
    }
}
