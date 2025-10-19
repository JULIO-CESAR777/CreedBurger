using UnityEngine;

public class StatusMood : MonoBehaviour
{
    public MoveClient moveClient;

    [Header("Prefabs")]
    public GameObject prefabFeliz;
    public GameObject prefabTriste;
    public GameObject prefabAlerta;
    public GameObject prefabSospechando;
     public GameObject prefabComiendo;

    [Header("Colocaci�n")]
    public float yOffset = 2f;          // altura sobre el cliente
    public Transform spawnParent;       // opcional (si lo dejas vac�o, usa este transform)

    private GameObject currentInstance;
    private MoveClient.Estado lastState = (MoveClient.Estado)(-1);

    void Awake()
    {
        if (moveClient == null)
            moveClient = GetComponent<MoveClient>();

        if (spawnParent == null)
            spawnParent = transform;    // por defecto, cu�lgalo de este objeto
    }

    void Update()
    {
        if (moveClient == null) return;

        var state = moveClient.estadoActual;
        if (state == lastState) return; // solo act�a cuando cambie el estado

        // Elegir prefab seg�n estado
        GameObject prefabToShow = null;
        switch (state)
        {
            case MoveClient.Estado.IrMesa:
            case MoveClient.Estado.EsperaPedido:
            case MoveClient.Estado.IrAleatorio:
            case MoveClient.Estado.EsperaAleatorio:
            case MoveClient.Estado.IrSalida:
                prefabToShow = prefabFeliz;
                break;

            case MoveClient.Estado.Quieto:
                prefabToShow = prefabTriste;
                break;

            case MoveClient.Estado.Aturdido:
            case MoveClient.Estado.Asustado:
                prefabToShow = prefabAlerta;
                break;

            case MoveClient.Estado.Sospechando:
                prefabToShow = prefabSospechando;
                break;
            
            case MoveClient.Estado.Comer:
                prefabToShow = prefabComiendo;
                break;

         

            case MoveClient.Estado.Terminado:
            default:
                prefabToShow = null;
                break;
        }

        // Apagar anterior
        if (currentInstance != null)
        {
            Destroy(currentInstance);
            currentInstance = null;
        }

        // Instanciar nuevo (si hay)
        if (prefabToShow != null)
        {
            Vector3 pos = spawnParent.position + Vector3.up * yOffset;
            currentInstance = Instantiate(prefabToShow, pos, Quaternion.identity, spawnParent);
        }

        lastState = state;
    }
}
