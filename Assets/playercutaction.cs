using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Interacción de "cortar" que se ejecuta MIENTRAS se mantiene presionado el
/// botón, se cancela de inmediato al soltarlo antes de tiempo, y una vez
/// COMPLETADO un corte, no se puede repetir hasta que el jugador suelte el
/// botón y lo vuelva a presionar (aunque lo mantenga apretado, no se dispara
/// en cadena solo).
///
/// Consulta el estado del botón directamente cada frame con
/// cutAction.IsPressed() en vez de depender de mensajes (ver explicación en
/// la versión anterior de este script).
///
/// Requiere un PlayerInput en el mismo GameObject con el Input Actions Asset
/// que contiene la acción "Cut" (en el Action Map "Player").
/// </summary>
[RequireComponent(typeof(PlayerInput))]
public class PlayerCutAction : MonoBehaviour
{
    private enum State
    {
        Idle,             // No está cortando, listo para empezar si se presiona.
        Holding,          // Botón presionado, corte en progreso.
        WaitingForRelease // Corte ya completado; bloqueado hasta que se suelte el botón.
    }

    [Tooltip("Segundos que hay que mantener presionado para completar el corte.")]
    [SerializeField] private float requiredHoldDuration = 2f;

    [Tooltip("Nombre de la acción tal como aparece en el Input Actions Asset.")]
    [SerializeField] private string cutActionName = "Cut";

    private PlayerInput playerInput;
    private InputAction cutAction;

    private State state = State.Idle;
    private float holdTimer;

    /// <summary>Progreso del corte actual, de 0 a 1. Útil para una barra de carga en la UI.</summary>
    public float Progress01 => Mathf.Clamp01(holdTimer / requiredHoldDuration);

    private void Awake()
    {
        playerInput = GetComponent<PlayerInput>();
        cutAction = playerInput.actions[cutActionName];
    }

    private void Update()
    {
        bool isPressed = cutAction.IsPressed();

        switch (state)
        {
            case State.Idle:
                if (isPressed)
                {
                    state = State.Holding;
                    holdTimer = 0f;
                }
                break;

            case State.Holding:
                if (!isPressed)
                {
                    // Soltó antes de completar -> se cancela de inmediato.
                    holdTimer = 0f;
                    state = State.Idle;
                    HandleCutCanceled();
                    break;
                }

                holdTimer += Time.deltaTime;

                // Esto se ejecuta CADA FRAME mientras el botón sigue presionado:
                // animación de cortar, partículas, sonido en loop, daño incremental, etc.
                HandleCutting(Time.deltaTime);

                if (holdTimer >= requiredHoldDuration)
                {
                    holdTimer = 0f;
                    state = State.WaitingForRelease; // Bloqueado hasta que suelte el botón.
                    CompleteCut();
                }
                break;

            case State.WaitingForRelease:
                if (!isPressed)
                {
                    state = State.Idle; // Ya puede volver a presionarlo para cortar de nuevo.
                }
                break;
        }
    }

    private void HandleCutting(float deltaTime)
    {
        Debug.Log($"{name}: cortando... {Progress01:P0}");
        // TODO: tu lógica real aquí (ej. reproducir animación de cortar,
        // aplicar daño al objeto objetivo, mover una barra de progreso UI
        // usando Progress01, etc.)
    }

    private void HandleCutCanceled()
    {
        Debug.Log($"{name}: corte cancelado (soltó antes de tiempo)");
        // TODO: resetear animación, ocultar barra de progreso, etc.
    }

    private void CompleteCut()
    {
        Debug.Log($"{name}: ¡corte completado! (suelta el botón para poder cortar otra vez)");
        // TODO: tu lógica de corte completado (destruir el objeto cortado,
        // dar recompensa, spawnear recursos, etc.)
    }
}