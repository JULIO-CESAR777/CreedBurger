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
    public float loseSightTime = 2f;
    public string playerTag = "Player";

    [Header("Vision")]
    [SerializeField] private float eyeHeight = 1.5f;
    [SerializeField] private float repathDistanceThreshold = 0.15f;

    [Header("Cooldown tras colisión con player")]
    [SerializeField] private float ignorePlayerAfterCollisionTime = 3f;

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
    private Vector3 lastChaseDestination;

    private Coroutine esperaRoutine;
    private Coroutine ignoreDetectionRoutine;
    private bool canDetectPlayer = true;

    void Start()
    {
        if (agent == null)
            agent = GetComponent<NavMeshAgent>();

        if (agent != null)
        {
            agent.speed = velocityKnight;
            agent.stoppingDistance = 0f;
        }

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

        if (estadoActual != Estado.Perseguir && canDetectPlayer)
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
        if (!canDetectPlayer) return;

        Transform visiblePlayer = BuscarPlayerVisible();
        if (visiblePlayer == null) return;

        if (esperaRoutine != null)
        {
            StopCoroutine(esperaRoutine);
            esperaRoutine = null;
        }

        player = visiblePlayer;
        lastSeenPos = player.position;
        lostTimer = 0f;

        estadoActual = Estado.Perseguir;
        agent.speed = chaseSpeed;
        agent.stoppingDistance = 0f;
        agent.isStopped = false;

        lastChaseDestination = player.position;
        agent.SetDestination(lastChaseDestination);
    }

    Transform BuscarPlayerVisible()
    {
        Collider[] hits = Physics.OverlapSphere(transform.position, playerDetectRadius);
        Transform bestTarget = null;
        float bestSqrDist = float.MaxValue;

        foreach (var h in hits)
        {
            Transform targetRoot = ObtenerPlayerRoot(h);
            if (targetRoot == null) continue;

            Vector3 origin = transform.position + Vector3.up * eyeHeight;
            Vector3 targetPoint = ObtenerPuntoVision(targetRoot);
            Vector3 toTarget = targetPoint - origin;

            float sqrDist = toTarget.sqrMagnitude;
            if (sqrDist > playerDetectRadius * playerDetectRadius) continue;

            float ang = Vector3.Angle(transform.forward, toTarget.normalized);
            if (ang > playerFOV * 0.5f) continue;

            if (!TieneLineaDeVision(targetRoot, origin, targetPoint)) continue;

            if (sqrDist < bestSqrDist)
            {
                bestSqrDist = sqrDist;
                bestTarget = targetRoot;
            }
        }

        return bestTarget;
    }

    void TickPerseguir()
    {
        if (!canDetectPlayer)
        {
            AbortChase();
            return;
        }

        if (player == null)
        {
            AbortChase();
            return;
        }

        Vector3 origin = transform.position + Vector3.up * eyeHeight;
        Vector3 targetPoint = ObtenerPuntoVision(player);
        Vector3 toTarget = targetPoint - origin;

        bool inRadius = toTarget.sqrMagnitude <= playerDetectRadius * playerDetectRadius;
        float ang = Vector3.Angle(transform.forward, toTarget.normalized);
        bool inFov = ang <= playerFOV * 0.5f;

        bool hasLineOfSight = false;

        if (inRadius && inFov)
            hasLineOfSight = TieneLineaDeVision(player, origin, targetPoint);

        if (hasLineOfSight)
        {
            lastSeenPos = player.position;
            lostTimer = 0f;
            agent.isStopped = false;

            if ((lastChaseDestination - player.position).sqrMagnitude > repathDistanceThreshold * repathDistanceThreshold)
            {
                lastChaseDestination = player.position;
                agent.SetDestination(lastChaseDestination);
            }
        }
        else
        {
            lostTimer += Time.deltaTime;

            if (!agent.pathPending &&
                (lastChaseDestination - lastSeenPos).sqrMagnitude > repathDistanceThreshold * repathDistanceThreshold)
            {
                lastChaseDestination = lastSeenPos;
                agent.isStopped = false;
                agent.SetDestination(lastChaseDestination);
            }

            if (lostTimer >= loseSightTime)
            {
                AbortChase();
                return;
            }
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (!EsPlayer(collision.gameObject)) return;

        EmpezarIgnorarDeteccionPorColision();
    }

    void EmpezarIgnorarDeteccionPorColision()
    {
        if (ignoreDetectionRoutine != null)
            StopCoroutine(ignoreDetectionRoutine);

        canDetectPlayer = false;
        AbortChase();

        ignoreDetectionRoutine = StartCoroutine(IgnorarDeteccionPorTiempo(ignorePlayerAfterCollisionTime));
    }

    IEnumerator IgnorarDeteccionPorTiempo(float segundos)
    {
        float timer = 0f;

        while (timer < segundos)
        {
            if (!isPaused)
                timer += Time.deltaTime;

            yield return null;
        }

        canDetectPlayer = true;
        ignoreDetectionRoutine = null;
    }

    bool EsPlayer(GameObject obj)
    {
        if (obj == null) return false;

        if (obj.CompareTag(playerTag))
            return true;

        Transform root = obj.transform.root;
        return root != null && root.CompareTag(playerTag);
    }

    bool TieneLineaDeVision(Transform targetRoot, Vector3 origin, Vector3 targetPoint)
    {
        Vector3 dir = targetPoint - origin;
        float dist = dir.magnitude;

        if (dist <= 0.001f) return true;

        if (Physics.Raycast(origin, dir.normalized, out RaycastHit hitInfo, dist))
        {
            Transform hitRoot = hitInfo.collider.transform.root;
            return hitRoot == targetRoot || hitInfo.collider.CompareTag(playerTag) || hitRoot.CompareTag(playerTag);
        }

        return false;
    }

    Vector3 ObtenerPuntoVision(Transform target)
    {
        Collider c = target.GetComponentInChildren<Collider>();
        if (c != null)
            return c.bounds.center;

        return target.position + Vector3.up;
    }

    Transform ObtenerPlayerRoot(Collider col)
    {
        if (col == null) return null;

        if (col.CompareTag(playerTag))
            return col.transform.root;

        Transform root = col.transform.root;
        if (root != null && root.CompareTag(playerTag))
            return root;

        return null;
    }

    void AbortChase()
    {
        player = null;
        lostTimer = 0f;

        if (agent == null) return;

        agent.speed = velocityKnight;
        agent.stoppingDistance = 0f;
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
               agent.remainingDistance <= agent.stoppingDistance + 0.05f &&
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
            agent.speed = velocityKnight;
            agent.stoppingDistance = 0f;
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
        Gizmos.DrawRay(transform.position + Vector3.up * eyeHeight, left * playerDetectRadius);
        Gizmos.DrawRay(transform.position + Vector3.up * eyeHeight, right * playerDetectRadius);
    }
}