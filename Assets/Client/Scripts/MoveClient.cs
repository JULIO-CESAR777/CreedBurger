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

    // --- Control de Mesa ---
    private MesaSeat mesaSeat;                         // componente de la mesa elegida
    [SerializeField] private float retryMesaSeconds = 1.0f; // cada cuánto reintentar si no hay mesas
    private Coroutine esperarMesaCR;
    private Transform mesaAsignada;

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

    // ====== Plan de comportamiento ======
    public enum Plan { ComerPaseoSalir, PaseoComerPaseoSalir }
    [Header("Plan de comportamiento")]
    public Plan plan = Plan.ComerPaseoSalir;

    [Header("Plan aleatorio al aparecer")]
    public bool randomizePlanOnSpawn = true;
    [Range(0f, 1f)] public float probPaseoComerPaseoSalir = 0.5f;

    private enum StepType { Aleatorio, Comer, Salir }
    private StepType[] steps;
    private int stepIndex = 0;
    private StepType CurrentStep => (steps != null && stepIndex >= 0 && stepIndex < steps.Length) ? steps[stepIndex] : StepType.Salir;

    // ====== Estados ======
    public enum Estado { IrMesa, EsperaPedido, Comer, IrAleatorio, EsperaAleatorio, IrSalida, Aturdido, Asustado, Sospechando, Terminado, Quieto }
    public Estado estadoActual = Estado.IrMesa;
    private Estado estadoPrevio;
    private Vector3 destinoPrevio;

    private Transform aleatorioSeleccionado;

    private float bloodTimer = 0f;
    private bool isFocusingBlood = false;

    void Start()
    {
        if (agent == null) agent = GetComponent<NavMeshAgent>();

        // Elegir plan al aparecer
        if (randomizePlanOnSpawn)
        {
            plan = (Random.value < probPaseoComerPaseoSalir)
                ? Plan.PaseoComerPaseoSalir
                : Plan.ComerPaseoSalir;
        }

        // Construir el itinerario con el plan ya decidido
        BuildPlan();

        if (pedirAlEntrar)
            HacerPedidoInicial();

        BeginCurrentStep();
    }

    void OnDestroy()
    {
        if (OrderUIController.Instance != null)
            OrderUIController.Instance.RemoveOrder(GetInstanceID());

        CancelarEsperaMesa();
        if (mesaSeat != null)
        {
            mesaSeat.Release(GetInstanceID());
            mesaSeat = null;
            mesaAsignada = null;
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
                // Espera a RecibirPedido(...)
                break;

            case Estado.Comer:
                // Manejado por corrutina que termina el paso Comer
                break;

            case Estado.IrAleatorio:
                if (HaLlegadoDestino())
                {
                    estadoActual = Estado.EsperaAleatorio;
                    // Espera en el punto y AVANZA al siguiente paso del plan
                    StartCoroutine(EsperaEnPunto(esperaAleatorio, StepComplete));
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

    // ====== Planner ======
    private void BuildPlan()
    {
        switch (plan)
        {
            case Plan.PaseoComerPaseoSalir:
                steps = new StepType[] { StepType.Aleatorio, StepType.Comer, StepType.Aleatorio, StepType.Salir };
                break;
            case Plan.ComerPaseoSalir:
            default:
                steps = new StepType[] { StepType.Comer, StepType.Aleatorio, StepType.Salir };
                break;
        }
        stepIndex = 0;
    }

    private void BeginCurrentStep()
    {
        if (steps == null || steps.Length == 0) { SalirDelLugar(); return; }

        switch (CurrentStep)
        {
            case StepType.Aleatorio:
                PasarAPaseo(); // setea estadoActual = IrAleatorio y camina a un punto
                break;

            case StepType.Comer:
            {
                if (mesas == null || mesas.Length == 0)
                {
                    Debug.LogWarning("[MoveClient] No hay mesas; se salta 'Comer'.");
                    StepComplete();
                    return;
                }

                // Buscar una mesa reservable
                mesaSeat = GetReservableMesa();
                if (mesaSeat == null)
                {
                    // No hay mesas disponibles: esperar y reintentar (sin avanzar de paso)
                    if (esperarMesaCR == null)
                        esperarMesaCR = StartCoroutine(EsperarMesaDisponible());
                    // Quédate en un estado neutro (quieto) o haz una animación de "espera"
                    estadoActual = Estado.Quieto;
                    return;
                }

                mesaAsignada = mesaSeat.transform;
                if (agent != null) agent.isStopped = false;
                IrAPunto(mesaAsignada);
                estadoActual = Estado.IrMesa;
                break;
            }

            case StepType.Salir:
                SalirDelLugar();
                break;
        }
    }

    private void StepComplete()
    {
        stepIndex++;
        if (steps == null || stepIndex >= steps.Length)
        {
            // Plan terminado => salir si no se fue aún
            SalirDelLugar();
            return;
        }
        BeginCurrentStep();
    }

    // --- LÓGICA ---
    private void SentarseYOrdenar()
    {
        if (agent != null) agent.isStopped = true;

        // Ocupar la mesa al llegar
        if (mesaSeat != null)
        {
            bool ok = mesaSeat.Sit(GetInstanceID());
            if (!ok)
            {
                // La perdimos (alguien se adelantó). Volver a intentar.
                Debug.Log("[MoveClient] La mesa reservada fue ocupada por otro. Reintentando...");
                mesaSeat = null;
                mesaAsignada = null;
                if (esperarMesaCR == null)
                    esperarMesaCR = StartCoroutine(EsperarMesaDisponible());
                estadoActual = Estado.Quieto;
                return;
            }
        }

        // Si no pidió al entrar, pedir ahora
        if (!pedirAlEntrar || !pedidoEnviado)
            HacerPedido();

        estadoActual = Estado.EsperaPedido;
    }

    private void EmpezarAComer()
    {
        // Quitar icono en UI
        if (OrderUIController.Instance != null)
            OrderUIController.Instance.RemoveOrder(GetInstanceID());

        estadoActual = Estado.Comer;

        if (agent != null)
        {
            agent.isStopped = true;
            agent.ResetPath();
        }

        AudioManager.I.Play("vfx_comiendo");

        // Comer es un PASO del plan; al terminar, avanzar
        StartCoroutine(EsperaEnPunto(esperaComer, OnComerFinished));
    }

    private void OnComerFinished()
    {
        // Liberar mesa antes de continuar con el plan
        if (mesaSeat != null)
        {
            mesaSeat.Release(GetInstanceID());
            mesaSeat = null;
            mesaAsignada = null;
        }
        StepComplete();
    }

    private void PasarAPaseo()
    {
        if (puntosAleatorios == null || puntosAleatorios.Length == 0)
        {
            // Sin puntos aleatorios => saltar a salir
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
        CancelarEsperaMesa();

        // Liberar mesa si la tenemos
        if (mesaSeat != null)
        {
            mesaSeat.Release(GetInstanceID());
            mesaSeat = null;
            mesaAsignada = null;
        }

        estadoActual = Estado.IrSalida;
        if (agent != null) agent.isStopped = false;
        IrAPunto(puntoSalida);
    }

    public int GetPedidoId() => pedido.id;

    // --- PEDIDOS ---
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
    IEnumerator EsperaEnPunto(float segundos, System.Action onDone)
    {
        yield return new WaitForSeconds(segundos);
        onDone?.Invoke();
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

    private void CancelarEsperaMesa()
    {
        if (esperarMesaCR != null)
        {
            StopCoroutine(esperarMesaCR);
            esperarMesaCR = null;
        }
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
        CancelarEsperaMesa();

        // Liberar mesa si aplica
        if (mesaSeat != null)
        {
            mesaSeat.Release(GetInstanceID());
            mesaSeat = null;
            mesaAsignada = null;
        }

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

    // ====== MESAS: reserva, espera y selección ======
    private MesaSeat GetReservableMesa()
    {
        if (mesas == null || mesas.Length == 0) return null;

        // Empezar en índice aleatorio para distribuir mejor
        int start = UnityEngine.Random.Range(0, mesas.Length);
        for (int i = 0; i < mesas.Length; i++)
        {
            var t = mesas[(start + i) % mesas.Length];
            if (t == null) continue;
            var seat = t.GetComponent<MesaSeat>();
            if (seat == null) continue;

            if (seat.TryReserve(GetInstanceID()))
                return seat;
        }
        return null;
    }

    private IEnumerator EsperarMesaDisponible()
    {
        while (true)
        {
            var seat = GetReservableMesa();
            if (seat != null)
            {
                mesaSeat = seat;
                mesaAsignada = seat.transform;
                if (agent != null) agent.isStopped = false;
                IrAPunto(mesaAsignada);
                estadoActual = Estado.IrMesa;
                esperarMesaCR = null;
                yield break;
            }
            yield return new WaitForSeconds(retryMesaSeconds);
        }
    }
}
