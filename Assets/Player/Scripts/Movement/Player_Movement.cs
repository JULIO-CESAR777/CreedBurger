using System;
using UnityEngine;
using UnityEngine.InputSystem;
public class Player_Movement : MonoBehaviour
{
    // Atributos privados
    public CharacterController characterController;
    private PlayerAnimationHandler animationHandler;
    private PlayerInputReader inputReader;
    private Vector2 input;
    private PlayerController controller;
    
    // Flags de control de movimiento (por si necesitas bloquear movimiento al interactuar)
    public bool canMove = true;
    
    // Atributos publicos
    [Header("Velocidad")]
    public float speed = 5.0f;
    private float currentSpeed;
    [Header("Dash")]
    public float dashSpeed = 15f;
    public float dashDuration = 0.2f;
    public float dashCooldown = 0.23f;
    private float dashTimer = 0f;
    private bool isDashing;
    private bool canDash;
    [Header("Gravedad")]
    public float gravity = -9.81f;
    public float groundedGravity = -2f;
    private float verticalVelocity;
    
    
    private void Awake()
    {
        characterController = GetComponent<CharacterController>();
        animationHandler = GetComponent<PlayerAnimationHandler>();
        inputReader = GetComponent<PlayerInputReader>();
        controller = GetComponent<PlayerController>();

    }

    private void OnEnable()
    {
        inputReader.OnMove += HandleMove;
        inputReader.OnDash += Dash;
    }

    private void OnDisable()
    {
        inputReader.OnMove -= HandleMove;
        inputReader.OnDash -= Dash;
    }
    
    private void Start()
    {
        currentSpeed = speed;
        isDashing = false;
        canDash = true;
    }
    
    public void HandleMove(Vector2 newInput)
    {
        input = newInput;
    }

    private void FixedUpdate()
    {

        if (controller.isPaused) return;
        
        if (!canMove)
        {
            // Si no se puede mover (por animación de interacción, etc.)
            animationHandler?.SetMovementSpeed(0f);
            return;
        }
        
        Vector3 move = new Vector3(input.x, 0, input.y).normalized;
        
        if (move.magnitude > 0.1f)
        {
            // Rotar hacia la dirección de movimiento
            Quaternion targetRotation = Quaternion.LookRotation(move);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, 10f * Time.deltaTime);
        }
        
        // Gravedad
        if (characterController.isGrounded)
        {
            if (verticalVelocity < 0)
                verticalVelocity = groundedGravity;
        }
        else
        {
            verticalVelocity += gravity * Time.fixedDeltaTime;
        }
        
        
        // Revisiones de dash
        if (isDashing)
        {
            
            dashTimer += Time.fixedDeltaTime;
            if (dashTimer >= dashDuration)
            {
                currentSpeed = speed;
                animationHandler?.PlayDash(false);
            }

            if (dashTimer >= dashCooldown)
            {
                canDash = true;
                isDashing = false;
                dashTimer = 0f;
            }
        }
        
        // Movimiento final incluyendo gravedad
        Vector3 velocity = new Vector3(input.x, 0, input.y).normalized * currentSpeed;
        velocity.y = verticalVelocity;

        characterController.Move(velocity * Time.fixedDeltaTime);
        animationHandler?.SetMovementSpeed(move.magnitude);
        
    }
    
    public void Dash()
    {
        if (canDash && !isDashing)
        {
            isDashing = true;
            canDash = false;
            currentSpeed = dashSpeed;
            dashTimer = 0f;
            animationHandler?.PlayDash(true);
        }
    }


}
