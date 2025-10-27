using UnityEngine;

public class KillCustomer : MonoBehaviour, IInteractable
{
    
    public void Interact(GameObject interactor)
    {
        
    
        MoveClient moveClient = gameObject.GetComponent<MoveClient>();
        
        if(moveClient == null) return;
        
        moveClient.Alto();
        moveClient.Morir();
        
        ScoreSystem.Instance?.AwardKillCustomer();


    }
    
    public InteractType GetInteractType() => InteractType.Kill;
    
}
