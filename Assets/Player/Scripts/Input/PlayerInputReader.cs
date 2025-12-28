using UnityEngine;
using UnityEngine.InputSystem;
using System;

public class PlayerInputReader : MonoBehaviour
{
    private PlayerInput playerInput;
    private PlayerInputActions inputActions;
    private PlayerController controller;

    //Candado para las acciones
    public bool IsLocked { get; private set; } = false;
    
    // Eventos públicos que otros scripts pueden escuchar
    public event Action<Vector2> OnMove;
    public event Action OnDash;
    public event Action OnInteract;
    public bool isDoingSomething = false;
    public event Action OnTraps;

    private InputAction movementAction;
    private InputAction dashAction;
    private InputAction interactAction;
    private InputAction setTrapsAction;

    private void Start()
    {
        controller = GetComponent<PlayerController>();
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
        setTrapsAction = inputActions.asset.FindAction(mapName + "/SetTraps");
        
        movementAction.performed += ctx => OnMove?.Invoke(ctx.ReadValue<Vector2>());
        movementAction.canceled += ctx => OnMove?.Invoke(Vector2.zero);
        
        dashAction.performed     += OnDashPerformed;
        interactAction.performed += OnInteractPerformed;
        setTrapsAction.performed += OnSetTrapsPerformed;
    }
    
    private void OnDisable()
    {
        movementAction?.Disable();
        if (dashAction != null)
        {
            dashAction.performed -= OnDashPerformed;
            dashAction.Disable();
        }
        if (interactAction != null)
        {
            interactAction.performed -= OnInteractPerformed;
            interactAction.Disable();
        }
        if (setTrapsAction != null)
        {
            setTrapsAction.performed -= OnSetTrapsPerformed;
            setTrapsAction.Disable();
        }
    }
    
    public void LockInputs()    => IsLocked = true;
    public void UnlockInputs()  => IsLocked = false;
    
    // ---------- Callbacks que respetan el lock ----------
    private void OnDashPerformed(InputAction.CallbackContext ctx)
    {
        if (!ctx.performed || IsLocked) return;
        if(controller.isPaused) return;
        OnDash?.Invoke();
    }

    private void OnInteractPerformed(InputAction.CallbackContext ctx)
    {
        if (!ctx.performed || IsLocked) return;
        if(controller.isPaused || isDoingSomething) return;
        OnInteract?.Invoke();
    }

    private void OnSetTrapsPerformed(InputAction.CallbackContext ctx)
    {
        if (!ctx.performed || IsLocked) return;
        if(controller.isPaused) return;
        OnTraps?.Invoke();
    }
    
    
}