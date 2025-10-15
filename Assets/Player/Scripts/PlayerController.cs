using UnityEngine;
using UnityEngine.UIElements;

public class PlayerController : MonoBehaviour
{
    public Player_Movement playerMovement;
    public PlayerAnimationHandler animationHandler;
    [SerializeField] public PlayerInteractionHandler playerInteractionHandler;
    public PlayerInputReader inputReader;
    public GameObject playerMesh;

    [Header("Sospechoso")] 
    public bool suspect;
    
    [Header("Trampas")]
    [SerializeField] public GameObject trapPrefab;
    
    
    private void Awake()
    {
        suspect = false;
        playerMovement = GetComponent<Player_Movement>();
        animationHandler = GetComponent<PlayerAnimationHandler>();
        inputReader = GetComponent<PlayerInputReader>();
        playerMesh = transform.GetChild(0).gameObject;
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

    public void CookMeat()
    {
        
    }
    

}
