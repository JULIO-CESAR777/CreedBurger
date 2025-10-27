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
    public float catchDistance = 1.2f;      // distancia para “atrapar”
    public float loseSightTime = 2f;        // tiempo sin ver al player para desistir
     
    public string playerTag = "Player";     // o usa LayerMask solamente

    // patrulla limitada
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
        Perseguir,      // 👈 nuevo
        Terminado
    }
    public Estado estadoActual = Estado.IrAleatorio;

    private Estado estadoPrevio;
    private Vector3 destinoPrevio;
    private Transform aleatorioSeleccionado;



    // persecución
    private Transform player;          // referencia viva al player detectado
    private float lostTimer = 0f;      // contador cuando se pierde de vista
    private Vector3 lastSeenPos;       // última posición vista

    void Start()
    {
        if (agent == null) agent = GetComponent<NavMeshAgent>();
        agent.speed = velocityKnight;

        // arranque patrulla
        puntoaleatorio();
        IrAPunto(aleatorioSeleccionado);
    }

    void Update()
    {
        // 1) intentar detectar player (si lo ves, saltas a Perseguir)
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
                // ⬇️ Aquí añadimos un Raycast para comprobar línea de visión
                RaycastHit hitInfo;
                if (Physics.Raycast(transform.position + Vector3.up * 1.5f, dirToTarget, out hitInfo, playerDetectRadius))
                {
                    if (hitInfo.collider.CompareTag(playerTag))
                    {
                        // Player visible
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

        // Arranca patrulla
        puntoaleatorio();
        IrAPunto(aleatorioSeleccionado);
    }
    void TickPerseguir()
    {
        if (player == null)
        {
            // perdió referencia del player por destrucción/cambio
            AbortChase();
            return;
        }

        // actualizar destino al player cada frame
        agent.isStopped = false;
        agent.SetDestination(player.position);

        // ¿lo alcanzó?
        float dist = Vector3.Distance(transform.position, player.position);
        if (dist <= catchDistance)
        {
            OnCatchPlayer(player);
            return;
        }

        // ¿sigue en cono de visión?
        Vector3 toTarget = (player.position - transform.position);
        float ang = Vector3.Angle(transform.forward, toTarget);

        bool inRadius = toTarget.sqrMagnitude <= playerDetectRadius * playerDetectRadius;
        bool inFov = ang <= playerFOV * 0.5f;

        if (inRadius && inFov /* && (opcional) HayLineaDeVision() */)
        {
            lastSeenPos = player.position;
            lostTimer = 0f;
        }
        else
        {
            // no lo vemos: contamos tiempo perdido
            lostTimer += Time.deltaTime;

            // mientras tanto, vamos a la última posición vista
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
        // volver a patrullar (o manda a salida, si prefieres)
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
        // Aquí define qué pasa al atrapar:
        // - Desactivar movimiento del player
        // - Reproducir animación/sonido
        // - Cargar escena, restar vida, etc.
        Debug.Log($"{name} atrapó a {target.name}");
        Destroy(this);
        // ejemplo: volver a patrullar o ir a salida
        AbortChase();
    }

    // ---------- PATRULLA ----------
    IEnumerator EsperaEnPunto(float segundos, Estado siguiente)
    {
        yield return new WaitForSeconds(segundos);

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
            agent.SetDestination(punto.position);
    }
    
    void OnDestroy()
    {
        // Cuenta incluye a este mismo, así que <=1 => soy el último que queda
        int remaining = GameObject.FindGameObjectsWithTag("Knight").Length;
        if (remaining <= 1)
        {
            AudioManager.I.PlayMusic("music_background");
        }
    }


    // (Aturdir y otros métodos pueden quedarse igual)
}
