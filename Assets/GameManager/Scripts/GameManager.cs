using UnityEngine;
using UnityEngine.InputSystem;

public class GameManager : MonoBehaviour
{
    
    // --- Singleton ---
    public static GameManager Instance { get; private set; }
    
    void Awake()
    {
        // Singleton básico
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        // Si quieres que sobreviva entre escenas, descomenta:
        // DontDestroyOnLoad(gameObject);
    }
    
    
    [Header("Configuración del juego")]
    [Range(1, 2)] public int cantidadJugadores = 1;

    [Header("Prefabs y spawn")]
    public GameObject jugadorPrefab;
    public GameObject cameraPrefab;
    public Transform[] puntosDeSpawn;
    
    [Header("Ingredient DB")]
    public IngredientPrefabDB ingredientePrefabDB;

    void Start()
    {
        int cantidad = GameSettings.Instance != null ? GameSettings.Instance.cantidadJugadores : cantidadJugadores;

        if (cantidad >= 1)
            CrearJugador("Player", puntosDeSpawn[0].position, 0);

        if (cantidad == 2)
            CrearJugador("Player2", puntosDeSpawn[1].position, 1);
        
        
        
    }


    void CrearJugador(string actionMap, Vector3 posicion, int index)
    {
        //Spawn del jugador
        GameObject obj = Instantiate(jugadorPrefab, posicion, Quaternion.identity);
        obj.name = actionMap;
        var input = obj.GetComponent<PlayerInput>();
        input.SwitchCurrentActionMap(actionMap);
        
        // Spawnear la camara
        GameObject cameraInstance = Instantiate(cameraPrefab, posicion, Quaternion.Euler(48, 0, 0));
        
        // Cambiarle el nombre por temas de debug
        cameraInstance.name = actionMap + "_Camera";
        
        // Mandar a llamar al script y darle un target
        var followScript = cameraInstance.GetComponent<SmoothCameraFollow>();
        if (followScript != null)
        {
            followScript.target = obj.transform;
        }
        
        // Asignar viewport para pantalla dividida
        Camera cam = cameraInstance.GetComponentInChildren<Camera>();
        
        if (cam != null)
        {
            if (cantidadJugadores == 1)
            {
                cam.rect = new Rect(0f, 0f, 1f, 1f); // pantalla completa
            }
            else
            {
                // Pantalla dividida horizontal (uno arriba, otro abajo)
                //if (index == 0)
                    //cam.rect = new Rect(0f, 0.5f, 1f, 0.5f); // Player 1 arriba
                //else
                    //cam.rect = new Rect(0f, 0f, 1f, 0.5f);   // Player 2 abajo

                // Si prefieres pantalla dividida vertical (lado a lado):
                 if (index == 0)
                     cam.rect = new Rect(0f, 0f, 0.5f, 1f); // izquierda
                 else
                     cam.rect = new Rect(0.5f, 0f, 0.5f, 1f); // derecha
            }
        }
        else
        {
            Debug.LogWarning("No se encontró una cámara en el prefab del jugador.");
        }
    }
}
