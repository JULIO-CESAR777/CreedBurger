using System;
using UnityEngine;
using UnityEngine.UIElements;

public class PlayerController : MonoBehaviour
{
    public Player_Movement playerMovement;
    public PlayerAnimationHandler animationHandler;
    [SerializeField] public PlayerInteractionHandler playerInteractionHandler;
    public PlayerInputReader inputReader;
    public GameObject playerMesh;

    public GameManager gameManager;
    
    [Header("Sospechoso")] 
    public bool suspect;
    
    [Header("Trampas")]
    [SerializeField] public GameObject trapPrefab;

    public bool isPaused;
    
    private void Awake()
    {
        suspect = false;
        playerMovement = GetComponent<Player_Movement>();
        animationHandler = GetComponent<PlayerAnimationHandler>();
        inputReader = GetComponent<PlayerInputReader>();
        playerMesh = transform.GetChild(0).gameObject;
    }

    private void Start()
    {
        gameManager = GameManager.GetInstance();
        // Sistema de pausa
        GameManager.GetInstance().onChangeGameState += OnChangeGameStateCallback;
        if(GameManager.GetInstance().gameState ==  GameState.Pause) isPaused = true;
    }
    
    public void OnChangeGameStateCallback(GameState newState)
    {
        isPaused = newState != GameState.Play;
    }

    public void GrabTrigger()
    {
        if(isPaused) return;
        playerInteractionHandler.OnGrabAnimationEvent();
    }

    // Reproducir un asesinato
    public void KillTrigger()
    {
        if(isPaused) return;
        AudioManager.I.Play("vfx_dieclient");
        playerInteractionHandler.OnKillAnimationEvent();
    }

    // Limpiar sangre
    public void CleanTrigger()
    {
        if(isPaused) return;
        playerInteractionHandler.OnCleanAnimationEvent();
    }
    

    public void UseTrap()
    {
        if (isPaused) return;
        playerInteractionHandler.UseTrap();
    }
    
}
