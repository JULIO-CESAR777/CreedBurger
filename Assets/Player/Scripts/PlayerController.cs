using System;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public Player_Movement playerMovement;
    public PlayerAnimationHandler animationHandler;
    [SerializeField] public PlayerInteractionHandler playerInteractionHandler;
    public PlayerInputReader inputReader;

    [Header("Sospechoso")] public bool suspect;
    
    private void Awake()
    {
        suspect = false;
        playerMovement = GetComponent<Player_Movement>();
        animationHandler = GetComponent<PlayerAnimationHandler>();
        inputReader = GetComponent<PlayerInputReader>();
    }


    public void GrabTrigger()
    {
        playerInteractionHandler.OnGrabAnimationEvent();
    }

    public void KillTrigger()
    {
        playerInteractionHandler.OnKillAnimationEvent();
    }

    public void CleanTrigger()
    {
        playerInteractionHandler.OnCleanAnimationEvent();
    }

}
