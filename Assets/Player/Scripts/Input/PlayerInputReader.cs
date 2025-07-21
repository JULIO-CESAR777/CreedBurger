using UnityEngine;
using UnityEngine.InputSystem;
using System;

public class PlayerInputReader : MonoBehaviour
{
    private PlayerInput playerInput;
    private PlayerInputActions inputActions;

    // Eventos públicos que otros scripts pueden escuchar
    public event Action<Vector2> OnMove;
    public event Action OnDash;
    public event Action OnInteract;

    private InputAction movementAction;
    private InputAction dashAction;
    private InputAction interactAction;

    private void Start()
    {
        playerInput = GetComponent<PlayerInput>();

        if (playerInput == null)
        {
            Debug.LogError("No se encontró el componente PlayerInput en: " + gameObject.name);
            return;
        }

        if (playerInput.currentActionMap == null)
        {
            Debug.LogWarning("El mapa actual de input es null en: " + gameObject.name);
            return;
        }

        inputActions = new PlayerInputActions();

        string mapName = playerInput.currentActionMap.name;

        inputActions.asset.FindActionMap(mapName)?.Enable();

        movementAction = inputActions.asset.FindAction(mapName + "/Movement");
        dashAction = inputActions.asset.FindAction(mapName + "/Dash");
        interactAction = inputActions.asset.FindAction(mapName + "/Interact");

        movementAction.performed += ctx => OnMove?.Invoke(ctx.ReadValue<Vector2>());
        movementAction.canceled += ctx => OnMove?.Invoke(Vector2.zero);
        dashAction.performed += _ => OnDash?.Invoke();
        interactAction.performed += _ => OnInteract?.Invoke();
    }


    private void OnDisable()
    {
        movementAction?.Disable();
        dashAction?.Disable();
        interactAction?.Disable();
    }
}