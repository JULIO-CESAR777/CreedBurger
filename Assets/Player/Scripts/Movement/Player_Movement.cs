using System;
using UnityEngine;
using UnityEngine.InputSystem;
public class Player_Movement : MonoBehaviour
{
    // Atributos privados
    private CharacterController characterController;
    private PlayerAnimationHandler animationHandler;
    private PlayerInputReader inputReader;
    private Vector2 input;
    
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
    
    
    private void Awake()
    {
        characterController = GetComponent<CharacterController>();
        animationHandler = GetComponent<PlayerAnimationHandler>();
        inputReader = GetComponent<PlayerInputReader>();

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

        
        Vector3 move = new Vector3(input.x, 0, input.y).normalized;
        
        if (move.magnitude > 0.1f)
        {
            // Rotar hacia la dirección de movimiento
            Quaternion targetRotation = Quaternion.LookRotation(move);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, 10f * Time.deltaTime);
        }
        
        // Revisiones de dash
        if (isDashing)
        {
            
            dashTimer += Time.fixedDeltaTime;
            if (dashTimer >= dashDuration)
            {
                currentSpeed = speed;
            }

            if (dashTimer >= dashCooldown)
            {
                canDash = true;
                isDashing = false;
                dashTimer = 0f;
            }
        }
        
        // Movemos el personaje
        characterController.Move(new Vector3(input.x, 0, input.y) * (currentSpeed * Time.deltaTime));
        animationHandler?.SetMovementSpeed(move.magnitude);
        
    }
    
    public void Dash()
    {
        if (canDash && !isDashing)
        {
            Debug.Log("Dash");
            isDashing = true;
            canDash = false;
            currentSpeed = dashSpeed;
            dashTimer = 0f;
            animationHandler?.PlayDash();
        }
    }


}
