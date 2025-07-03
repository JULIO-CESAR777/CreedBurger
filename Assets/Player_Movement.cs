using System;
using UnityEngine;
using UnityEngine.InputSystem;
public class Player_Movement : MonoBehaviour
{
    private Rigidbody rb;
    private PlayerInputActions playerInputActions;
    
    public float speed = 5.0f;
    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        playerInputActions = new PlayerInputActions();
        playerInputActions.Player.Enable();
        playerInputActions.Player.Dash.performed += Dash;
        
    }

    private void FixedUpdate()
    {
        Vector2 input = playerInputActions.Player.Movement.ReadValue<Vector2>();
        rb.AddForce(new Vector3(input.x, 0, input.y) * speed, ForceMode.Force);
    }
    
    public void Dash(InputAction.CallbackContext context)
    {
        Debug.Log("Dash" + context.phase);
    }


}
