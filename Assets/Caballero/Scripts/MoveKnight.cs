using UnityEngine;
using UnityEngine.AI;
using System.Collections;

public class MoveKnight : MonoBehaviour
{
    public NavMeshAgent agent;

    [Header("Puntos")]
    public Transform[] puntosAleatorios;
    public Transform puntoSalida;

    [Header("Velocidades")]
    public int velocityKnight = 5;
    public int chaseSpeed = 7;

    [Header("Tiempos de espera")]
    public float esperaAleatorio = 2f;
    public float esperaSangreVer = 2f;

    [Header("Detección de Player")]
    public float playerDetectRadius = 10f;
    [Range(0f, 360f)] public float playerFOV = 140f;
    public float catchDistance = 1.2f;
    public float loseSightTime = 2f;

    public string playerTag = "Player";

    public bool isPaused = false;

    private int puntosVisitados = 0;
    public int maxPuntos = 5;

    public enum Estado
    {
        Quieto,
        IrAleatorio,
        EsperaAleatorio,
        IrSalida,
        Aturdido,
        Sospechando,
        Perseguir,
        Terminado
    }

    public Estado estadoActual = Estado.IrAleatorio;

    private Transform aleatorioSeleccionado;
    private Transform player;

    private float lostTimer = 0f;
    private Vector3 lastSeenPos;

    private Coroutine esperaRoutine;

    void Start()
    {
        if (agent == null)
            agent = GetComponent<NavMeshAgent>();

        if (agent != null)
            agent.speed = velocityKnight;

        var gm = GameManager.GetInstance();
        if (gm != null)
        {
            gm.onChangeGameState += OnChangeGameStateCallback;

            if (gm.gameState == GameState.Pause)
                isPaused = true;
        }

        if (aleatorioSeleccionado == null && estadoActual != Estado.IrSalida)
            IrANuevoPuntoAleatorio();

        AplicarPausa(isPaused);
    }

    public void Initialize(Transform[] waypoints, Transform salida, int maxPuntosPatrulla)
    {
        puntosAleatorios = waypoints;
        puntoSalida = salida;
        maxPuntos = maxPuntosPatrulla;

        IrANuevoPuntoAleatorio();
    }

    public void OnChangeGameStateCallback(GameState newState)
    {
        isPaused = newState != GameState.Play;
        AplicarPausa(isPaused);
    }

    void AplicarPausa(bool pausado)
    {
        if (agent == null) return;

        if (pausado)
        {
            agent.isStopped = true;
            agent.velocity = Vector3.zero;
            return;
        }

        agent.isStopped = false;

        switch (estadoActual)
        {
            case Estado.IrAleatorio:
                if (aleatorioSeleccionado != null)
                    agent.SetDestination(aleatorioSeleccionado.position);
                break;

            case Estado.Perseguir:
                if (player != null)
                    agent.SetDestination(player.position);
                break;

            case Estado.IrSalida:
                if (puntoSalida != null)
                    agent.SetDestination(puntoSalida.position);
                break;
        }
    }

    void Update()
    {
        if (isPaused || agent == null) return;

        DetectarPlayer();

        switch (estadoActual)
        {
            case Estado.IrAleatorio:
                if (HaLlegadoDestino())
                {
                    estadoActual = Estado.EsperaAleatorio;

                    if (esperaRoutine != null)
                        StopCoroutine(esperaRoutine);

                    esperaRoutine = StartCoroutine(EsperaEnPunto(esperaAleatorio));
                }
                else if (!agent.pathPending && (!agent.hasPath || agent.pathStatus != NavMeshPathStatus.PathComplete))
                {
                    IrANuevoPuntoAleatorio();
                }
                break;

            case Estado.Perseguir:
                TickPerseguir();
                break;

            case Estado.IrSalida:
                if (HaLlegadoDestino())
                {
                    estadoActual = Estado.Terminado;
                    Destroy(gameObject);
                }
                break;

            case Estado.Quieto:
                if (!agent.pathPending && !agent.hasPath)
                {
                    if (puntosVisitados < maxPuntos)
                        IrANuevoPuntoAleatorio();
                    else if (puntoSalida != null)
                    {
                        estadoActual = Estado.IrSalida;
                        IrAPunto(puntoSalida);
                    }
                }
                break;
        }
    }

    void DetectarPlayer()
    {
        if (estadoActual == Estado.Perseguir) return;

        Collider[] hits = Physics.OverlapSphere(transform.position, playerDetectRadius);

        foreach (var h in hits)
        {
            if (!h.CompareTag(playerTag)) continue;

            Vector3 origin = transform.position + Vector3.up * 1.5f;
            Vector3 dirToTarget = (h.transform.position - origin).normalized;
            float ang = Vector3.Angle(transform.forward, dirToTarget);

            if (ang > playerFOV * 0.5f) continue;

            if (Physics.Raycast(origin, dirToTarget, out RaycastHit hitInfo, playerDetectRadius))
            {
                if (!hitInfo.collider.CompareTag(playerTag)) continue;

                if (esperaRoutine != null)
                {
                    StopCoroutine(esperaRoutine);
                    esperaRoutine = null;
                }

                player = h.transform;
                lastSeenPos = player.position;
                lostTimer = 0f;

                estadoActual = Estado.Perseguir;
                agent.speed = chaseSpeed;
                agent.isStopped = false;
                agent.SetDestination(player.position);
                return;
            }
        }
    }

    void TickPerseguir()
    {
        if (player == null)
        {
            AbortChase();
            return;
        }

        Vector3 origin = transform.position + Vector3.up * 1.5f;
        Vector3 toTarget = player.position - origin;
        float dist = Vector3.Distance(transform.position, player.position);
        float ang = Vector3.Angle(transform.forward, toTarget);

        bool inRadius = toTarget.sqrMagnitude <= playerDetectRadius * playerDetectRadius;
        bool inFov = ang <= playerFOV * 0.5f;

        bool hasLineOfSight = false;
        if (inRadius && inFov)
        {
            if (Physics.Raycast(origin, toTarget.normalized, out RaycastHit hitInfo, playerDetectRadius))
            {
                hasLineOfSight = hitInfo.collider.CompareTag(playerTag);
            }
        }

        if (hasLineOfSight)
        {
            lastSeenPos = player.position;
            lostTimer = 0f;
            agent.isStopped = false;
            agent.SetDestination(player.position);
        }
        else
        {
            lostTimer += Time.deltaTime;

            if (!agent.pathPending)
            {
                agent.isStopped = false;
                agent.SetDestination(lastSeenPos);
            }

            if (lostTimer >= loseSightTime)
            {
                AbortChase();
                return;
            }
        }

        if (dist <= catchDistance)
        {
            OnCatchPlayer(player);
        }
    }

    void AbortChase()
    {
        player = null;
        lostTimer = 0f;

        if (agent == null) return;

        agent.speed = velocityKnight;
        agent.isStopped = false;
        agent.ResetPath();

        if (puntosVisitados < maxPuntos)
        {
            IrANuevoPuntoAleatorio();
        }
        else
        {
            estadoActual = Estado.IrSalida;
            IrAPunto(puntoSalida);
        }
    }

    void OnCatchPlayer(Transform target)
    {
        Debug.Log($"{name} atrapó a {target.name}");
        AbortChase();
    }

    IEnumerator EsperaEnPunto(float segundos)
    {
        float timer = 0f;

        while (timer < segundos)
        {
            if (!isPaused)
                timer += Time.deltaTime;

            yield return null;
        }

        esperaRoutine = null;
        puntosVisitados++;

        if (puntosVisitados < maxPuntos)
        {
            IrANuevoPuntoAleatorio();
        }
        else
        {
            estadoActual = Estado.IrSalida;
            IrAPunto(puntoSalida);
        }
    }

    bool HaLlegadoDestino()
    {
        if (agent == null) return false;

        return !agent.pathPending &&
               agent.remainingDistance <= agent.stoppingDistance &&
               (!agent.hasPath || agent.velocity.sqrMagnitude <= 0.05f);
    }

    bool IrANuevoPuntoAleatorio()
    {
        if (puntosAleatorios == null || puntosAleatorios.Length == 0 || agent == null)
            return false;

        for (int i = 0; i < puntosAleatorios.Length * 2; i++)
        {
            Transform candidato = puntosAleatorios[Random.Range(0, puntosAleatorios.Length)];
            if (candidato == null) continue;

            NavMeshPath path = new NavMeshPath();
            bool pathValido = agent.CalculatePath(candidato.position, path) &&
                              path.status == NavMeshPathStatus.PathComplete;

            if (!pathValido) continue;

            aleatorioSeleccionado = candidato;
            estadoActual = Estado.IrAleatorio;
            IrAPunto(aleatorioSeleccionado);
            return true;
        }

        agent.ResetPath();
        estadoActual = Estado.Quieto;
        return false;
    }

    public void puntoaleatorio()
    {
        if (puntosAleatorios == null || puntosAleatorios.Length == 0) return;
        aleatorioSeleccionado = puntosAleatorios[Random.Range(0, puntosAleatorios.Length)];
    }

    void IrAPunto(Transform punto)
    {
        if (punto == null || agent == null) return;

        agent.isStopped = false;
        agent.SetDestination(punto.position);

        if (isPaused)
            agent.isStopped = true;
    }

    void OnDestroy()
    {
        var gm = GameManager.GetInstance();
        if (gm != null)
            gm.onChangeGameState -= OnChangeGameStateCallback;

        int remaining = GameObject.FindGameObjectsWithTag("Knight").Length;
        if (remaining <= 1)
        {
            AudioManager.I?.PlayMusic("music_background");
        }
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, playerDetectRadius);

        Vector3 left = Quaternion.Euler(0, -playerFOV * 0.5f, 0) * transform.forward;
        Vector3 right = Quaternion.Euler(0, playerFOV * 0.5f, 0) * transform.forward;

        Gizmos.color = Color.cyan;
        Gizmos.DrawRay(transform.position + Vector3.up, left * playerDetectRadius);
        Gizmos.DrawRay(transform.position + Vector3.up, right * playerDetectRadius);
    }
}