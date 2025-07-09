using UnityEngine;

public class PlayerInteractionHandler : MonoBehaviour
{
    [SerializeField] private PlayerInputReader inputReader;
    
    private void OnEnable()
    {
        inputReader.OnInteract += Interact;
    }

    private void OnDisable()
    {
        inputReader.OnInteract -= Interact;
    }

    public void Interact()
    {
        Debug.Log("Interact");
    }
    
    
    
    
    
    
}
