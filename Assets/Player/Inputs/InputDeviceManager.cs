using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Rastrea los dispositivos de input conectados (teclado + gamepads) y decide
/// qué dispositivo le corresponde a cada jugador.
///
/// Soporta dos modos (ver <see cref="GameMode"/>):
///
/// MODO DosJugadores (comportamiento original):
///  - 0 gamepads activos -> Player1 = Teclado,          Player2 = (ninguno)
///  - 1 gamepad activo   -> Player1 = Teclado,          Player2 = ese gamepad
///  - 2 gamepads activos -> Player2 = último conectado, Player1 = el otro
///                          (el teclado queda libre / sin jugador asociado)
///
/// MODO UnJugador (para jugar solo sin necesitar 2 controles):
///  - 0 gamepads activos -> Player1 = Teclado
///  - 1+ gamepads activos -> Player1 = el último gamepad conectado
///  - Player2 SIEMPRE es null (no se crea un segundo jugador).
///
/// IMPORTANTE: solo se "activan" como máximo 2 gamepads (los primeros 2 que
/// se conecten). Si llega un 3er, 4to, etc. control mientras ya hay 2 activos,
/// se IGNORA por completo: no entra a la lista, no afecta la asignación de
/// nadie, ni siquiera si más adelante se desconecta alguno de los 2 activos
/// (para que vuelva a tomarse en cuenta, tendría que desconectarse y
/// reconectarse cuando haya un cupo libre).
///
/// Es un singleton persistente: ponlo en un GameObject vacío en tu escena de
/// arranque (o cárgalo con DontDestroyOnLoad, que ya lo hace este script).
/// </summary>
public class InputDeviceManager : MonoBehaviour
{
    public enum GameMode
    {
        SinglePlayer,
        TwoPlayers
    }

    public static InputDeviceManager Instance { get; private set; }

    private const int MaxActiveGamepads = 2;

    [Tooltip("Modo con el que arranca el manager. Normalmente lo cambiarás desde tu menú " +
             "llamando a SetGameMode(...) antes de cargar la partida, así que este valor " +
             "es solo el de respaldo si nadie lo cambia.")]
    [SerializeField] private GameMode startingMode = GameMode.TwoPlayers;

    /// <summary>Gamepads ACTIVOS (máx. 2), en orden de conexión (el último del listado = el más reciente).</summary>
    private readonly List<Gamepad> activeGamepads = new List<Gamepad>();

    public GameMode CurrentMode { get; private set; }
    public InputDevice Player1Device { get; private set; }
    public InputDevice Player2Device { get; private set; }

    /// <summary>Se dispara cada vez que cambia la asignación de dispositivos (conexión/desconexión/modo).</summary>
    public event Action OnAssignmentsChanged;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
        CurrentMode = startingMode;
    }

    private void OnEnable()
    {
        // Registramos los gamepads que ya estaban conectados antes de que este script se activara,
        // respetando el límite (cualquier extra desde el arranque también se ignora).
        activeGamepads.Clear();
        foreach (var device in Gamepad.all)
        {
            if (activeGamepads.Count >= MaxActiveGamepads)
            {
                break;
            }
            activeGamepads.Add(device);
        }

        InputSystem.onDeviceChange += HandleDeviceChange;
        RecalculateAssignments();
    }

    private void OnDisable()
    {
        InputSystem.onDeviceChange -= HandleDeviceChange;
    }

    /// <summary>
    /// Llama esto desde tu menú (ej. botones "1 Jugador" / "2 Jugadores") ANTES de
    /// cargar la escena de juego, o incluso durante el juego si quieres permitir
    /// cambiarlo en caliente.
    /// </summary>
    public void SetGameMode(GameMode mode)
    {
        if (CurrentMode == mode)
        {
            return;
        }

        CurrentMode = mode;
        RecalculateAssignments();
    }

    private void HandleDeviceChange(InputDevice device, InputDeviceChange change)
    {
        if (device is not Gamepad gamepad)
        {
            return; // Solo nos interesan los gamepads; el teclado siempre está "disponible".
        }

        switch (change)
        {
            case InputDeviceChange.Added:
            case InputDeviceChange.Reconnected:
                if (!activeGamepads.Contains(gamepad))
                {
                    if (activeGamepads.Count < MaxActiveGamepads)
                    {
                        activeGamepads.Add(gamepad); // Se agrega al final = el más reciente.
                    }
                    // else: ya hay 2 controles activos -> este se ignora por completo.
                }
                RecalculateAssignments();
                break;

            case InputDeviceChange.Removed:
            case InputDeviceChange.Disconnected:
                // Si no estaba en la lista (era un control "extra" ignorado), esto no hace nada.
                activeGamepads.Remove(gamepad);
                RecalculateAssignments();
                break;
        }
    }

    private void RecalculateAssignments()
    {
        InputDevice newPlayer1;
        InputDevice newPlayer2;

        int count = activeGamepads.Count; // Siempre 0, 1 o 2 gracias al límite.

        if (CurrentMode == GameMode.SinglePlayer)
        {
            // Un solo jugador: prioriza el último gamepad conectado si hay alguno;
            // si no hay ninguno, usa el teclado. Nunca hay Player2.
            newPlayer1 = count > 0 ? activeGamepads[count - 1] : Keyboard.current;
            newPlayer2 = null;
        }
        else // TwoPlayers
        {
            if (count == 0)
            {
                newPlayer1 = Keyboard.current;
                newPlayer2 = null;
            }
            else if (count == 1)
            {
                newPlayer1 = Keyboard.current;
                newPlayer2 = activeGamepads[0];
            }
            else // count == 2
            {
                newPlayer2 = activeGamepads[1]; // El último conectado.
                newPlayer1 = activeGamepads[0]; // El otro control.
            }
        }

        bool changed = newPlayer1 != Player1Device || newPlayer2 != Player2Device;

        Player1Device = newPlayer1;
        Player2Device = newPlayer2;

        if (changed)
        {
            OnAssignmentsChanged?.Invoke();
        }
    }
}