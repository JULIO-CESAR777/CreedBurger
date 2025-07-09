using System;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public Player_Movement playerMovement;
    public PlayerAnimationHandler playerAnimationHandler;
    [SerializeField] public PlayerInteractionHandler playerInteractionHandler;
    public PlayerInputReader inputReader;

    private void Awake()
    {
        playerMovement = GetComponent<Player_Movement>();
        playerAnimationHandler = GetComponent<PlayerAnimationHandler>();
        //playerInteractionHandler = GetComponent<PlayerInteractionHandler>();
        inputReader = GetComponent<PlayerInputReader>();
    }
    
}
