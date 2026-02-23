using UnityEngine;
using UnityEngine.AI;
using System.Collections;

public class MoveClient : MonoBehaviour
{
    public NavMeshAgent agent;
    
    private ClientOrderIndicator indicator;

    [Header("Puntos")]
    public Transform puntoSalida;
    public Transform[] puntosAleatorios;

    [Header("Mesas del restaurante")]
    public Transform[] mesas;

    // --- Control de Mesa ---
    private MesaSeat mesaSeat;                         
    [SerializeField] private float retryMesaSeconds = 1.0f; 
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
    
    public bool isPaused;

    // ==== Pausa: cacheo de estado del agente/anim ====
    private Vector3 _prevDestination;
    private bool _prevHadPath;
    private bool _prevStopped;
    private bool _pauseCached;      // <- solo restauramos si esto es true
    private float _baseSpeed;       // <- speed de arranque (por si acaso)
    private Animator _anim;

    void Awake()
    {
        if (agent == null) agent = GetComponent<NavMeshAgent>();
        _anim = GetComponentInChildren<Animator>();
        _baseSpeed = (agent != null) ? agent.speed : 3.5f; // valor típico por defecto
        indicator = GetComponent<ClientOrderIndicator>();
    }

    void Start()
    {
        GameManager.GetInstance().onChangeGameState += OnChangeGameStateCallback;

        // Estado inicial según tu manager:
        isPaused = (GameManager.GetInstance().gameState == GameState.Pause);

        // No alteramos speed si no hay pausa cacheada aún:
        ApplyPauseState();

        // Elegir plan al aparecer
        if (randomizePlanOnSpawn)
        {
            plan = (Random.value < probPaseoComerPaseoSalir)
                ? Plan.PaseoComerPaseoSalir
                : Plan.ComerPaseoSalir;
        }

        BuildPlan();

        if (pedirAlEntrar)
            HacerPedidoInicial();

        BeginCurrentStep();
    }

    public void OnChangeGameStateCallback(GameState newState)
    {
        isPaused = (newState == GameState.Pause);
        ApplyPauseState();
    }

    private void ApplyPauseState()
    {
        if (agent == null) return;

        if (isPaused)
        {
            // Cachear estado SOLO cuando entramos a pausa
            _prevStopped = agent.isStopped;
            _prevHadPath = agent.hasPath;
            _prevDestination = _prevHadPath ? agent.destination : transform.position;
            _pauseCached = true;

            // Congelar
            agent.isStopped = true;
            agent.velocity = Vector3.zero;

            if (_anim) _anim.speed = 0f;
        }
        else
        {
            // Reanudar
            if (_anim) _anim.speed = 1f;

            if (_pauseCached)
            {
                agent.isStopped = _prevStopped;

                // Si teníamos destino y no estábamos parados, retomar
                if (!_prevStopped && _prevHadPath)
                    agent.SetDestination(_prevDestination);
            }
            else
            {
                // No había estado cacheado (por ejemplo, arranque en Play): solo asegurar que no esté parado
                agent.isStopped = false;
            }

            // Si por algún motivo el speed quedó en 0 (ediciones en runtime, etc.), recuperarlo
            if (agent.speed <= 0.001f)
                agent.speed = _baseSpeed;
        }
    }

    void OnDestroy()
    {
        if (GameManager.GetInstance() != null)
            GameManager.GetInstance().onChangeGameState -= OnChangeGameStateCallback;

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
        if (isPaused) return;

        DetectarSangreVisual();

        switch (estadoActual)
        {
            case Estado.IrMesa:
                if (HaLlegadoDestino())
                    SentarseYOrdenar();
                break;

            case Estado.EsperaPedido:
                break;

            case Estado.Comer:
                break;

            case Estado.IrAleatorio:
                if (HaLlegadoDestino())
                {
                    estadoActual = Estado.EsperaAleatorio;
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
                PasarAPaseo();
                break;

            case StepType.Comer:
            {
                if (mesas == null || mesas.Length == 0)
                {
                    Debug.LogWarning("[MoveClient] No hay mesas; se salta 'Comer'.");
                    StepComplete();
                    return;
                }

                mesaSeat = GetReservableMesa();
                if (mesaSeat == null)
                {
                    if (esperarMesaCR == null)
                        esperarMesaCR = StartCoroutine(EsperarMesaDisponible());
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
            SalirDelLugar();
            return;
        }
        BeginCurrentStep();
    }

    // --- LÓGICA ---
    private void SentarseYOrdenar()
    {
        if (agent != null) agent.isStopped = true;

        if (mesaSeat != null)
        {
            bool ok = mesaSeat.Sit(GetInstanceID());
            if (!ok)
            {
                Debug.Log("[MoveClient] La mesa reservada fue ocupada por otro. Reintentando...");
                mesaSeat = null;
                mesaAsignada = null;
                if (esperarMesaCR == null)
                    esperarMesaCR = StartCoroutine(EsperarMesaDisponible());
                estadoActual = Estado.Quieto;
                return;
            }
        }

        if (!pedirAlEntrar || !pedidoEnviado)
            HacerPedido();

        estadoActual = Estado.EsperaPedido;
    }

    private void EmpezarAComer()
    {
        if (OrderUIController.Instance != null)
            OrderUIController.Instance.RemoveOrder(GetInstanceID());

        estadoActual = Estado.Comer;

        if (agent != null)
        {
            agent.isStopped = true;
            agent.ResetPath();
        }
        if (indicator != null)
            indicator.Hide();
        AudioManager.I.Play("vfx_comiendo");

        StartCoroutine(EsperaEnPunto(esperaComer, OnComerFinished));
    }

    private void OnComerFinished()
    {
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

        // ✅ mostrar el icono porque está esperando
        if (indicator != null && pedido.image != null)
            indicator.Show(pedido.image);
    }

    public void RecibirPedido(int idDelChef)
    {
        if (!pedidoEnviado || pedidoListo) return;

        if (pedido.id == idDelChef)
        {
            pedidoListo = true;

            // ✅ ya no está esperando => ocultar
            if (indicator != null)
                indicator.Hide();

            if (estadoActual == Estado.EsperaPedido)
                EmpezarAComer();
        }
        else
        {
            Debug.Log($"[MoveClient] Pedido incorrecto. Esperado={pedido.id}, entregado={idDelChef} (cliente {name})");
        }
    }

    // --- Corrutinas: pausable por variable ---
    private IEnumerator PausableWait(float seconds)
    {
        float t = 0f;
        while (t < seconds)
        {
            if (!isPaused) t += Time.deltaTime;
            yield return null;
        }
    }

    IEnumerator EsperaEnPunto(float segundos, System.Action onDone)
    {
        yield return PausableWait(segundos);
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
        yield return PausableWait(segundos);
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
            yield return PausableWait(retryMesaSeconds);
        }
    }
}
