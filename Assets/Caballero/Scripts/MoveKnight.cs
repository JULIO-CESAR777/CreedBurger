using UnityEngine;
using UnityEngine.AI;
using System.Collections;

public class MoveKnight : MonoBehaviour
{
    public NavMeshAgent agent;

    public Transform[] puntosAleatorios;
    public Transform puntoSalida;

    [Header("Velocidades")]
    public int velocityKnight = 5;
    public int chaseSpeed = 7;

    [Header("Tiempos de espera")]
    public float esperaAleatorio = 2;
    public float esperaSangreVer = 2;

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

    private Estado estadoPrevio;
    private Vector3 destinoPrevio;
    private Transform aleatorioSeleccionado;

    private Transform player;
    private float lostTimer = 0f;
    private Vector3 lastSeenPos;

    void Start()
    {
        if (agent == null) agent = GetComponent<NavMeshAgent>();
        agent.speed = velocityKnight;

        GameManager.GetInstance().onChangeGameState += OnChangeGameStateCallback;
        if (GameManager.GetInstance().gameState == GameState.Pause) isPaused = true;

        puntoaleatorio();
        IrAPunto(aleatorioSeleccionado);

        AplicarPausa(isPaused);
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
        }
        else
        {
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
    }

    void Update()
    {
        if (isPaused) return;

        DetectarPlayer();

        switch (estadoActual)
        {
            case Estado.IrAleatorio:
                if (HaLlegadoDestino())
                {
                    estadoActual = Estado.EsperaAleatorio;
                    StartCoroutine(EsperaEnPunto(esperaAleatorio, Estado.IrAleatorio));
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
        }
    }

    void DetectarPlayer()
    {
        if (estadoActual == Estado.Perseguir) return;

        Collider[] hits = Physics.OverlapSphere(transform.position, playerDetectRadius);
        foreach (var h in hits)
        {
            if (!h.CompareTag(playerTag)) continue;

            Vector3 dirToTarget = (h.transform.position - transform.position).normalized;
            float ang = Vector3.Angle(transform.forward, dirToTarget);

            if (ang <= playerFOV * 0.5f)
            {
                RaycastHit hitInfo;
                if (Physics.Raycast(transform.position + Vector3.up * 1.5f, dirToTarget, out hitInfo, playerDetectRadius))
                {
                    if (hitInfo.collider.CompareTag(playerTag))
                    {
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
        }
    }

    public void Initialize(Transform[] waypoints, Transform salida, int maxPuntosPatrulla)
    {
        puntosAleatorios = waypoints;
        puntoSalida = salida;
        maxPuntos = maxPuntosPatrulla;

        puntoaleatorio();
        IrAPunto(aleatorioSeleccionado);
    }

    void TickPerseguir()
    {
        if (player == null)
        {
            AbortChase();
            return;
        }

        agent.isStopped = false;
        agent.SetDestination(player.position);

        float dist = Vector3.Distance(transform.position, player.position);
        if (dist <= catchDistance)
        {
            OnCatchPlayer(player);
            return;
        }

        Vector3 toTarget = (player.position - transform.position);
        float ang = Vector3.Angle(transform.forward, toTarget);

        bool inRadius = toTarget.sqrMagnitude <= playerDetectRadius * playerDetectRadius;
        bool inFov = ang <= playerFOV * 0.5f;

        if (inRadius && inFov)
        {
            lastSeenPos = player.position;
            lostTimer = 0f;
        }
        else
        {
            lostTimer += Time.deltaTime;

            if (!agent.pathPending)
                agent.SetDestination(lastSeenPos);

            if (lostTimer >= loseSightTime)
            {
                AbortChase();
            }
        }
    }

    void AbortChase()
    {
        estadoActual = (puntosVisitados < maxPuntos) ? Estado.IrAleatorio : Estado.IrSalida;
        agent.speed = velocityKnight;

        if (estadoActual == Estado.IrAleatorio)
        {
            puntoaleatorio();
            IrAPunto(aleatorioSeleccionado);
        }
        else
        {
            IrAPunto(puntoSalida);
        }
    }

    void OnCatchPlayer(Transform target)
    {
        Debug.Log($"{name} atrapó a {target.name}");
        AbortChase();
    }

    IEnumerator EsperaEnPunto(float segundos, Estado siguiente)
    {
        float timer = 0f;

        while (timer < segundos)
        {
            if (!isPaused)
                timer += Time.deltaTime;

            yield return null;
        }

        puntosVisitados++;

        if (puntosVisitados < maxPuntos)
        {
            puntoaleatorio();
            estadoActual = Estado.IrAleatorio;
            IrAPunto(aleatorioSeleccionado);
        }
        else
        {
            estadoActual = Estado.IrSalida;
            IrAPunto(puntoSalida);
        }
    }

    bool HaLlegadoDestino()
    {
        return !agent.pathPending &&
               agent.remainingDistance <= agent.stoppingDistance &&
               (!agent.hasPath || agent.velocity.sqrMagnitude == 0f);
    }

    public void puntoaleatorio()
    {
        if (puntosAleatorios == null || puntosAleatorios.Length == 0) return;
        aleatorioSeleccionado = puntosAleatorios[Random.Range(0, puntosAleatorios.Length)];
    }

    void IrAPunto(Transform punto)
    {
        if (punto != null && agent != null)
        {
            agent.SetDestination(punto.position);
            agent.isStopped = isPaused;
        }
    }

    void OnDestroy()
    {
        int remaining = GameObject.FindGameObjectsWithTag("Knight").Length;
        if (remaining <= 1)
        {
            AudioManager.I.PlayMusic("music_background");
        }
    }
}