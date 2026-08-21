using System;
using UnityEngine;
using UnityEngine.InputSystem;
 
/// <summary>
/// Movimiento simple en 3D (plano X/Z) para probar que cada jugador responde
/// a su propio dispositivo (teclado o gamepad), sin importar cuál le haya
/// tocado según el InputDeviceManager / PlayerSpawner.
///
/// Requiere un componente PlayerInput en el mismo GameObject, con:
///   - Actions Asset: PlayerControls.inputactions
///   - Default Map: Player
///   - Behavior: Send Messages
///
/// El método OnMove(InputValue) lo llama automáticamente el PlayerInput
/// cuando la acción "Move" cambia de valor.
/// "Dash" se consulta directamente con IsPressed() cada frame, detectando el
/// flanco de "recién presionado" nosotros mismos, porque es una acción de
/// botón simple
/// </summary>
[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(PlayerInput))]
public class PlayerController : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float rotationSpeed = 10f;
 
    private Rigidbody rb;
    private Vector2 moveInput;
    
    // Inputs
    private PlayerInput playerInput;
    
    [Header("Dash")]
    [SerializeField] private string dashActionName = "Dash";
    [SerializeField] private float dashDistance = 5f;   // Distancia total que recorre el dash.
    [SerializeField] private float dashDuration = 0.2f; // Cuánto tarda en recorrer esa distancia.
    [SerializeField] private float dashCooldown = 1f;   // Tiempo de espera antes de poder volver a dashear.
    private InputAction dashAction;
    private bool isDashing;
    private bool wasDashPressedLastFrame;
    private float dashTimer;
    private float dashCooldownTimer;
    private Vector3 dashDirection;
 
    /// <summary>True mientras el dash está en curso. Útil si quieres bloquear otras acciones (ej. Cut) durante el dash.</summary>
    public bool IsDashing => isDashing;
    
    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true; // Evita que el Rigidbody se vuelque al moverse.
        
        playerInput = GetComponent<PlayerInput>();
        dashAction = playerInput.actions[dashActionName];
    }
 
    // Llamado por PlayerInput (Behavior = Send Messages) al cambiar la acción "Move".
    public void OnMove(InputValue value)
    {
        moveInput = value.Get<Vector2>();
    }

    private void Update()
    {
        // El cooldown corre siempre, independientemente de si se está moviendo o dasheando.
        if (dashCooldownTimer > 0f)
        {
            dashCooldownTimer -= Time.deltaTime;
        }
 
        bool isDashPressed = dashAction.IsPressed();
        bool justPressed = isDashPressed && !wasDashPressedLastFrame;
        wasDashPressedLastFrame = isDashPressed;
 
        if (justPressed && !isDashing && dashCooldownTimer <= 0f)
        {
            StartDash(); // Manda a iniciar el bool del dash
        }
        
    }
    
    private void FixedUpdate()
    {
        if (isDashing)
        {
            PerformDash();
            return; // Mientras dashea, ignora el movimiento normal.
        }
        
        
        Vector3 direction = new Vector3(moveInput.x, 0f, moveInput.y);
 
        if (direction.sqrMagnitude < 0.0001f)
        {
            return;
        }
 
        // Movimiento
        Vector3 targetPosition = rb.position + direction.normalized * moveSpeed * Time.fixedDeltaTime;
        rb.MovePosition(targetPosition);
 
        // Rotar el personaje para que mire hacia donde se mueve.
        Quaternion targetRotation = Quaternion.LookRotation(direction);
        rb.MoveRotation(Quaternion.Slerp(rb.rotation, targetRotation, rotationSpeed * Time.fixedDeltaTime));
    }
    
    #region Dashing
    private void StartDash() // Aqui se busca obtener la direccion del dash y empezar el dash en el Fixed Update
    {
        Vector3 inputDirection = new Vector3(moveInput.x, 0f, moveInput.y);
 
        // Dashea hacia donde se está moviendo; si no hay input de movimiento, dashea hacia donde mira.
        dashDirection = inputDirection.sqrMagnitude > 0.0001f
            ? inputDirection.normalized
            : transform.forward;
 
        isDashing = true;
        dashTimer = 0f;
        dashCooldownTimer = dashCooldown;
 
        rb.MoveRotation(Quaternion.LookRotation(dashDirection));
    }
    
    private void PerformDash() // Esto ya es el movimiento del dash
    {
        dashTimer += Time.fixedDeltaTime;
 
        float dashSpeed = dashDistance / dashDuration;
        Vector3 targetPosition = rb.position + dashDirection * dashSpeed * Time.fixedDeltaTime;
        rb.MovePosition(targetPosition);
 
        if (dashTimer >= dashDuration)
        {
            isDashing = false;
        }
    }
    #endregion
}
