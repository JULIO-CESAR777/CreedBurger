using System;
using UnityEngine;
using UnityEngine.InputSystem;
public class Player_Movement : MonoBehaviour
{
    // Atributos privados
    private CharacterController characterController;
    private PlayerInputActions playerInputActions;
    private Animator animator;
    
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
        animator = GetComponent<Animator>();
        playerInputActions = new PlayerInputActions();
        playerInputActions.Player.Enable();
        playerInputActions.Player.Dash.performed += Dash;
    }

    private void Start()
    {
        currentSpeed = speed;
        isDashing = false;
        canDash = true;
    }

    private void FixedUpdate()
    {
        // Guardamos los inputs recibidos de movimiento
        Vector2 input = playerInputActions.Player.Movement.ReadValue<Vector2>();
        
        // Animacion entre idle y caminar
        animator.SetFloat("movement", (input.x == 0 && input.y == 0) ? 0 : 1);
        
        Vector3 move = new Vector3(input.x, 0, input.y);
        
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
            if(dashTimer >= dashDuration)
                currentSpeed = speed;

            if (dashTimer >= dashCooldown)
            {
                canDash = true;
                isDashing = false;
                dashTimer = 0f;
            }
        }
        
        // Movemos el personaje
        characterController.Move(new Vector3(input.x, 0, input.y) * (currentSpeed * Time.deltaTime));
        
    }
    
    public void Dash(InputAction.CallbackContext context)
    {
        if (canDash && !isDashing)
        {
            Debug.Log("Dash");
            isDashing = true;
            canDash = false;
            currentSpeed = dashSpeed;
            dashTimer = 0f;
        }
    }


}
