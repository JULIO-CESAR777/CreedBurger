using UnityEngine;
using UnityEngine.InputSystem;

public class GameManager : MonoBehaviour
{
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
        
        Debug.Log("Cantidad de jugadores: " + cantidad);
    }


    void CrearJugador(string actionMap, Vector3 posicion)
    {
        GameObject obj = Instantiate(jugadorPrefab, posicion, Quaternion.identity);
        var input = obj.GetComponent<PlayerInput>();
        input.SwitchCurrentActionMap(actionMap); // Asigna su mapa específico
    }
}
