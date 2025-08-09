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
    public Transform[] puntosDeSpawn; // tamaño 2

    void Start()
    {
        int cantidad = GameSettings.Instance != null ? GameSettings.Instance.cantidadJugadores : 1;

        if (cantidad >= 1)
            CrearJugador("Player", puntosDeSpawn[0].position);

        if (cantidad == 2)
            CrearJugador("Player2", puntosDeSpawn[1].position);
        
    }


    void CrearJugador(string actionMap, Vector3 posicion)
    {
        GameObject obj = Instantiate(jugadorPrefab, posicion, Quaternion.identity);
        var input = obj.GetComponent<PlayerInput>();
        input.SwitchCurrentActionMap(actionMap); // Asigna su mapa específico
    }
}
