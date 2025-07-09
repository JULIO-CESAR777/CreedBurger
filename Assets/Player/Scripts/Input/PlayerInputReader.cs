using UnityEngine;
using UnityEngine.InputSystem;
using System;

public class PlayerInputReader : MonoBehaviour
{
    private PlayerInputActions inputActions;

    // Eventos públicos que otros scripts pueden escuchar
    public event Action<Vector2> OnMove;
    public event Action OnDash;
    public event Action OnInteract;
    
    private void Awake()
    {
        inputActions = new PlayerInputActions();
        inputActions.Enable();

        // Movimiento continuo
        inputActions.Player.Movement.performed += ctx => OnMove?.Invoke(ctx.ReadValue<Vector2>());
        inputActions.Player.Movement.canceled += ctx => OnMove?.Invoke(Vector2.zero);

        // Acciones de un solo disparo
        inputActions.Player.Dash.performed += ctx => OnDash?.Invoke();
        inputActions.Player.Interact.performed += ctx => OnInteract?.Invoke();
    }

    private void OnDisable()
    {
        inputActions.Disable();
    }
    
    
}
