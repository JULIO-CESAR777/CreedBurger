using UnityEngine;
using UnityEngine.AI;
using System.Collections;

public class MoveClient : MonoBehaviour
{
    public NavMeshAgent agent;

    public Transform puntoOrdenar;
    public Transform puntoSecundario;
    public Transform[] puntosAleatorios;
    public Transform puntoSalida;

    [Header("Velocidades")]
    public int miedoaaa = 4;

    [Header("Tiempos de espera")]
    public float esperaComer = 2;
    public float esperaAleatorio = 2;
    public float esperaAturdimiento = 5;
    public float esperaSangreVer = 5;

    [Header("Detección de Sangre Visual")]
    public float detectionRadius = 6f;
    [Range(0f, 360f)] public float fieldOfView = 120f;

    public GameObject prefabSangre;

    public GameObject prefabCarne;

    public enum Estado
    {
        IrOrdenar,
        Quieto,
        IrComer,
        EsperaComer,
        IrAleatorio,
        EsperaAleatorio,
        IrSalida,
        Aturdido,
        Asustado,
        Sospechando,
        Terminado
    }
    public Estado estadoActual = Estado.IrOrdenar;
    private Estado estadoPrevio;
    private Vector3 destinoPrevio;

    private bool triggerOrdenar = false;
    private Transform aleatorioSeleccionado;

    // Temporizador para detección visual de sangre
    private float bloodTimer = 0f;
    private bool isFocusingBlood = false;

    void Start()
    {
        if (agent == null)
            agent = GetComponent<NavMeshAgent>();

        IrAPunto(puntoOrdenar);
    }

    void Update()
    {
        // Detección visual de sangre
        DetectarSangreVisual();

        // Comportamiento según estado
        switch (estadoActual)
        {
            case Estado.IrOrdenar:
                if (triggerOrdenar)
                {
                    estadoActual = Estado.IrComer;
                    IrAPunto(puntoSecundario);
                }
                break;

            case Estado.IrComer:
                if (HaLlegadoDestino())
                {
                    estadoActual = Estado.EsperaComer;
                    StartCoroutine(EsperaEnPunto(esperaComer, Estado.IrAleatorio));
                }
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
                // No hace nada mientras está aturdido
                break;

            case Estado.Asustado:
                // Simplemente sigue a puntoSalida
                if (HaLlegadoDestino())
                {
                    estadoActual = Estado.Terminado;
                    Destroy(gameObject);
                }
                break;

            case Estado.Quieto:
                // Detenido indefinidamente
                break;
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (estadoActual == Estado.IrOrdenar && other.CompareTag("Ordenar"))
        {
            triggerOrdenar = true;
            Debug.Log("Trigeree el Ordenar");
        }
        else
        {
            Debug.Log("Trigereo pero no con el ese");
        }
    }

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
                // Primer frame de detección: detener al agente
                isFocusingBlood = true;
                agent.isStopped = true;
            }
            estadoActual = Estado.Sospechando;
            // Mirar lentamente hacia la sangre
            Vector3 lookDir = (targetBlood.position - transform.position).normalized;
            Quaternion lookRot = Quaternion.LookRotation(lookDir);
            transform.rotation = Quaternion.Slerp(transform.rotation, lookRot, Time.deltaTime * 2f);

            bloodTimer += Time.deltaTime;
            if (bloodTimer >= esperaSangreVer)
                EntrarEnMiedo();
        }
        else if (isFocusingBlood)
        {
            // Se interrumpe la visión antes de completar el timer
            isFocusingBlood = false;
            bloodTimer = 0f;
            agent.isStopped = false;
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

    IEnumerator EsperaEnPunto(float segundos, Estado siguienteEstado)
    {
        if (siguienteEstado == Estado.IrAleatorio)
        {
            aleatorioSeleccionado = puntosAleatorios[Random.Range(0, puntosAleatorios.Length)];
            yield return new WaitForSeconds(segundos);
            estadoActual = Estado.IrAleatorio;
            IrAPunto(aleatorioSeleccionado);
        }
        else if (siguienteEstado == Estado.IrSalida)
        {
            yield return new WaitForSeconds(segundos);
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

    void IrAPunto(Transform punto)
    {
        if (punto != null && agent != null)
            agent.SetDestination(punto.position);
    }

    public void Aturdir(float duracion)
    {
        if (estadoActual == Estado.Aturdido)
            return;

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
        agent.SetDestination(destinoPrevio);
    }

    public void Alto()
    {
        estadoActual = Estado.Quieto;
        agent.ResetPath();
        agent.velocity = Vector3.zero;
    }

    public void Morir()
    {
        Vector3 SpawnCosas = transform.position + Vector3.up * 2;

        Instantiate(prefabCarne, SpawnCosas, transform.rotation);
        Instantiate(prefabSangre , transform.position, Quaternion.Euler(90,0,0));

        // (Opcional) destruir este objeto “muerto”
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
