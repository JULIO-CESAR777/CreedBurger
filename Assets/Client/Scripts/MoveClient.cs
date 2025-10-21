using UnityEngine;
using UnityEngine.AI;
using System.Collections;

public class MoveClient : MonoBehaviour
{
    public NavMeshAgent agent;

    [Header("Puntos")]
    public Transform puntoSalida;
    public Transform[] puntosAleatorios;

    [Header("Mesas del restaurante")]
    public Transform[] mesas;

    [Header("Velocidades")]
    public int miedoaaa = 4;

    [Header("Tiempos de espera")]
    public float esperaComer = 4f;
    public float esperaAleatorio = 2f;
    public float esperaSangreVer = 5f;

    [Header("Detección de Sangre Visual")]
    public float detectionRadius = 6f;
    [Range(0f, 360f)] public float fieldOfView = 120f;

    public GameObject prefabSangre;
    public GameObject prefabCarne;

    [Header("Receta / UI (Panel en Canvas)")]
    public IngredientPrefabDB db;
    [Tooltip("-1 = aleatorio; >=0 fuerza ese ID de la DB")]
    public int idPedidoForzado = -1;
    [Tooltip("Si true, pide al entrar; si false, pide hasta sentarse")]
    public bool pedirAlEntrar = true;

    // Pedido actual
    private IngredientPrefabDB.Entry pedido;
    private bool pedidoEnviado = false;
    private bool pedidoListo = false;

    // Estados
    public enum Estado { IrMesa, EsperaPedido, Comer, IrAleatorio, EsperaAleatorio, IrSalida, Aturdido, Asustado, Sospechando, Terminado, Quieto }
    public Estado estadoActual = Estado.IrMesa;
    private Estado estadoPrevio;
    private Vector3 destinoPrevio;

    private Transform mesaAsignada;
    private Transform aleatorioSeleccionado;

    private float bloodTimer = 0f;
    private bool isFocusingBlood = false;

    void Start()
    {
        if (agent == null) agent = GetComponent<NavMeshAgent>();

        // Asignar mesa inicial o paseo
        if (mesas != null && mesas.Length > 0)
        {
            mesaAsignada = mesas[Random.Range(0, mesas.Length)];
            IrAPunto(mesaAsignada);
            estadoActual = Estado.IrMesa;
        }
        else
        {
            PasarAPaseo();
        }

        // Pide receta al entrar (opcional)
        if (pedirAlEntrar)
            HacerPedidoInicial();
    }

    void OnDestroy()
    {
        // Quita su icono del panel si sigue ahí
        if (OrderUIController.Instance != null)
            OrderUIController.Instance.RemoveOrder(GetInstanceID());
    }

    void Update()
    {
        DetectarSangreVisual();

        switch (estadoActual)
        {
            case Estado.IrMesa:
                if (HaLlegadoDestino()) SentarseYOrdenar();
                break;

            case Estado.EsperaPedido:
                // esperando RecibirPedido(...)
                break;

            case Estado.Comer:
                // lo maneja la corrutina
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

            case Estado.Asustado:
                if (HaLlegadoDestino())
                {
                    estadoActual = Estado.Terminado;
                    Destroy(gameObject);
                }
                break;
        }
    }

    // --- LÓGICA ---
    private void SentarseYOrdenar()
    {
        if (agent != null) agent.isStopped = true;

        // Si no pidió al entrar, pedir ahora
        if (!pedirAlEntrar || !pedidoEnviado)
            HacerPedido();

        estadoActual = Estado.EsperaPedido;
    }
    
    private void EmpezarAComer()
    {
        // Ya lleg� el plato correcto
        if (OrderUIController.Instance != null)
            OrderUIController.Instance.RemoveOrder(GetInstanceID());

        estadoActual = Estado.Comer;

        // Por si ven�amos parados
        if (agent != null)
        {
            agent.isStopped = true;
            agent.ResetPath();
        }
        AudioManager.I.Play("vfx_comiendo");
        // Simular comer X segundos y luego pasear
        StartCoroutine(EsperaEnPunto(esperaComer, Estado.IrAleatorio));
        //Debug.Log("[MoveClient] Comiendo...");
    }

    private void PasarAPaseo()
    {
        if (puntosAleatorios == null || puntosAleatorios.Length == 0)
        {
            estadoActual = Estado.IrSalida;
            IrAPunto(puntoSalida);
            return;
        }

        aleatorioSeleccionado = puntosAleatorios[Random.Range(0, puntosAleatorios.Length)];
        estadoActual = Estado.IrAleatorio;

        if (agent != null) agent.isStopped = false;
        IrAPunto(aleatorioSeleccionado);
    }

    private void SalirDelLugar()
    {
        estadoActual = Estado.IrSalida;
        if (agent != null) agent.isStopped = false;
        IrAPunto(puntoSalida);
    }
    public int GetPedidoId() => pedido.id;

// --- PEDIDOS (usa la misma lógica en ambos) ---
    private void HacerPedidoInicial()
    {
        if (db == null || db.entries.Count == 0) return;

        if (idPedidoForzado >= 0 && db.TryGetById(idPedidoForzado, out var eById))
            pedido = eById;
        else
            db.TryGetRandom(out pedido);

        pedidoEnviado = true;
        pedidoListo = false;

        if (OrderUIController.Instance != null && pedido.image != null)
            OrderUIController.Instance.AddOrder(GetInstanceID(), pedido.image);

        Debug.Log($"[MoveClient] Pedido inicial ID={pedido.id} (cliente {name})");
    }

    private void HacerPedido()
    {
        if (db == null || db.entries.Count == 0) return;

        if (idPedidoForzado >= 0 && db.TryGetById(idPedidoForzado, out var eById))
            pedido = eById;
        else
            db.TryGetRandom(out pedido);

        pedidoEnviado = true;
        pedidoListo = false;

        if (OrderUIController.Instance != null && pedido.image != null)
            OrderUIController.Instance.AddOrder(GetInstanceID(), pedido.image);

        Debug.Log($"[MoveClient] Pedido en mesa ID={pedido.id} (cliente {name})");
    }

    // El chef/mesero llama esto cuando entrega el plato
    public void RecibirPedido(int idDelChef)
    {
        if (!pedidoEnviado) return;

        if (pedido.id == idDelChef)
        {
            pedidoListo = true;
            if (estadoActual == Estado.EsperaPedido)
                EmpezarAComer();
        }
        else
        {
            Debug.Log($"[MoveClient] Pedido incorrecto. Esperado={pedido.id}, entregado={idDelChef} (cliente {name})");
        }
    }


    // --- MOV/ESTADO ---
    IEnumerator EsperaEnPunto(float segundos, Estado siguiente)
    {
        yield return new WaitForSeconds(segundos);

        if (siguiente == Estado.IrAleatorio)      PasarAPaseo();
        else if (siguiente == Estado.IrSalida)    SalirDelLugar();
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

    // --- REACCIONES ---
    private void DetectarSangreVisual()
    {
        if (estadoActual == Estado.Asustado || estadoActual == Estado.Terminado) return;

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
        if (puntoSalida != null)
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

        if (estadoActual == Estado.EsperaPedido || estadoActual == Estado.Comer)
        {
            if (agent != null) agent.isStopped = true;
        }
        else
        {
            if (agent != null) agent.SetDestination(destinoPrevio);
        }
    }

    public void Alto()
    {
        estadoActual = Estado.Quieto;
        if (agent != null)
        {
            agent.ResetPath();
            agent.velocity = Vector3.zero;
        }
    }

    public void Morir()
    {
        Vector3 spawnCosas = transform.position + Vector3.up * 2;
        if (prefabCarne) Instantiate(prefabCarne, spawnCosas, transform.rotation);
        if (prefabSangre) Instantiate(prefabSangre, transform.position, Quaternion.Euler(90, 0, 0));
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
