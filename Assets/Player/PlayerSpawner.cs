using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Crea/reasigna a los jugadores en base a lo que decide el InputDeviceManager.
///
/// - No usa PlayerInputManager: la asignación es manual y determinista
///   (no es "presiona un botón para unirte"), así que ese componente no hace falta.
/// - Si el dispositivo de un jugador cambia en caliente (ej. se desconecta un
///   control y el sistema reordena), el jugador NO se destruye sin razón:
///   se intenta preservar su posición/rotación al re-emparejarlo.
/// </summary>
public class PlayerSpawner : MonoBehaviour
{
    [SerializeField] private GameObject playerPrefab; // Debe tener un componente PlayerInput
    [SerializeField] private Transform spawnPointPlayer1;
    [SerializeField] private Transform spawnPointPlayer2;

    [Tooltip("Debe coincidir EXACTAMENTE con el nombre del Control Scheme en tu Input Actions Asset")]
    [SerializeField] private string keyboardScheme = "Keyboard";

    [Tooltip("Debe coincidir EXACTAMENTE con el nombre del Control Scheme en tu Input Actions Asset")]
    [SerializeField] private string gamepadScheme = "Gamepad";

    private PlayerInput player1Input;
    private PlayerInput player2Input;

    private void OnEnable()
    {
        // Unity no garantiza que InputDeviceManager.Awake() ya se haya ejecutado
        // cuando este OnEnable corre (depende del orden de los GameObjects en la
        // escena). Si ya existe, nos suscribimos de inmediato; si no, esperamos
        // a que aparezca en vez de asumir que ya está listo.
        if (InputDeviceManager.Instance != null)
        {
            Subscribe();
        }
        else
        {
            StartCoroutine(WaitForManagerThenSubscribe());
        }
    }

    private IEnumerator WaitForManagerThenSubscribe()
    {
        while (InputDeviceManager.Instance == null)
        {
            yield return null;
        }
        Subscribe();
    }

    private void Subscribe()
    {
        InputDeviceManager.Instance.OnAssignmentsChanged += HandleAssignmentsChanged;
        HandleAssignmentsChanged(); // Asignación inicial al arrancar la escena.
    }

    private void OnDisable()
    {
        StopAllCoroutines();

        if (InputDeviceManager.Instance != null)
        {
            InputDeviceManager.Instance.OnAssignmentsChanged -= HandleAssignmentsChanged;
        }
    }

    private void HandleAssignmentsChanged()
    {
        var mgr = InputDeviceManager.Instance;
        InputDevice newP1 = mgr.Player1Device;
        InputDevice newP2 = mgr.Player2Device;

        InputDevice oldP2Device = GetCurrentDevice(player2Input);

        // Caso especial: si el dispositivo que le toca ahora al Jugador 1 es el
        // mismo que TIENE actualmente el Jugador 2, hay que liberarlo primero
        // (actualizando a Player 2) antes de que Player 1 intente tomarlo,
        // o habrá un conflicto de emparejamiento.
        if (newP1 != null && newP1 == oldP2Device)
        {
            UpdatePlayer(ref player2Input, newP2, spawnPointPlayer2);
            UpdatePlayer(ref player1Input, newP1, spawnPointPlayer1);
        }
        else
        {
            UpdatePlayer(ref player1Input, newP1, spawnPointPlayer1);
            UpdatePlayer(ref player2Input, newP2, spawnPointPlayer2);
        }
    }

    private static InputDevice GetCurrentDevice(PlayerInput playerInput)
    {
        if (playerInput != null && playerInput.devices.Count > 0)
        {
            return playerInput.devices[0];
        }
        return null;
    }

    private void UpdatePlayer(ref PlayerInput playerInput, InputDevice device, Transform spawnPoint)
    {
        // No hay dispositivo para este jugador (ej. Player2 sin control conectado).
        if (device == null)
        {
            if (playerInput != null)
            {
                playerInput.user.UnpairDevices();
                Destroy(playerInput.gameObject);
                playerInput = null;
            }
            return;
        }

        // Ya tiene exactamente este dispositivo: no hacemos nada.
        if (GetCurrentDevice(playerInput) == device)
        {
            return;
        }

        // Si ya existía con OTRO dispositivo, lo recreamos pero conservando su posición.
        Vector3 lastPosition = spawnPoint != null ? spawnPoint.position : Vector3.zero;
        Quaternion lastRotation = spawnPoint != null ? spawnPoint.rotation : Quaternion.identity;

        if (playerInput != null)
        {
            lastPosition = playerInput.transform.position;
            lastRotation = playerInput.transform.rotation;

            // Liberamos el dispositivo INMEDIATAMENTE (Destroy es diferido al final del frame,
            // y si no liberamos ya, el otro jugador podría chocar al intentar tomarlo).
            playerInput.user.UnpairDevices();
            Destroy(playerInput.gameObject);
            playerInput = null;
        }

        string scheme = device is Keyboard ? keyboardScheme : gamepadScheme;

        playerInput = PlayerInput.Instantiate(
            playerPrefab,
            controlScheme: scheme,
            pairWithDevice: device);

        playerInput.transform.SetPositionAndRotation(lastPosition, lastRotation);
    }
}