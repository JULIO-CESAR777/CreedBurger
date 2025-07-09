using System;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public Player_Movement playerMovement;
    public Player_Grab playerGrab;
    public Player_Interact playerInteract;
    public PlayerAnimationHandler playerAnimationHandler;

    private void Awake()
    {
        playerMovement = GetComponent<Player_Movement>();
        playerGrab = GetComponent<Player_Grab>();
        playerAnimationHandler = GetComponent<PlayerAnimationHandler>();
    }
}
