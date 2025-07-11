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

    [Header("Tiempos de espera")]
    public float esperaComer = 2f;
    public float esperaAleatorio = 2f;

    private enum Estado { IrOrdenar, IrComer, EsperaComer, IrAleatorio, EsperaAleatorio, IrSalida, Terminado }
    private Estado estadoActual = Estado.IrOrdenar;

    private bool triggerOrdenar = false;
    private Transform aleatorioSeleccionado;

    void Start()
    {
        if (agent == null)
            agent = GetComponent<NavMeshAgent>();

        IrAPunto(puntoOrdenar);
    }

    void Update()
    {
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
                    Destroy(this.gameObject);
                }
                break;
        }
    }

    void IrAPunto(Transform punto)
    {
        if (punto != null && agent != null)
            agent.SetDestination(punto.position);
    }

    bool HaLlegadoDestino()
    {
        return !agent.pathPending && agent.remainingDistance <= agent.stoppingDistance && (!agent.hasPath || agent.velocity.sqrMagnitude == 0f);
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

    IEnumerator EsperaEnPunto(float segundos, Estado siguienteEstado)
    {
        // Solo selecciona punto aleatorio al pasar a ese estado
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
}
