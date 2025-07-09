using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class Player_Grab : MonoBehaviour
{
    // Inputs
    private PlayerInputActions playerInputActions;
    
    // Objeto a interactuar
    public GameObject grabbedObject;
    
    // Referencia al objeto
    [SerializeField] Player_Interact playerInteract;
    
    private void Awake()
    {
        grabbedObject = null;
        
        playerInputActions = new PlayerInputActions();
        playerInputActions.Player.Enable();
        playerInputActions.Player.Interact.performed += Interact;
        
    }

    public void Interact(InputAction.CallbackContext context)
    {
        grabbedObject = playerInteract.interactObject;
        Grab();
    }

    public void Grab()
    {
        // Realiza la animacion
    }





}
