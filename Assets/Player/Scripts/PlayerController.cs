using System;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public Player_Movement playerMovement;
    public PlayerAnimationHandler animationHandler;
    [SerializeField] public PlayerInteractionHandler playerInteractionHandler;
    public PlayerInputReader inputReader;

    private void Awake()
    {
        playerMovement = GetComponent<Player_Movement>();
        animationHandler = GetComponent<PlayerAnimationHandler>();
        inputReader = GetComponent<PlayerInputReader>();
    }


    public void GrabTrigger()
    {
        playerInteractionHandler.OnGrabAnimationEvent();
    }


}
