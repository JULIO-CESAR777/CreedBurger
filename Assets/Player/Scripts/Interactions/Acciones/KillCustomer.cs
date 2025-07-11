using UnityEngine;

public class KillCustomer : MonoBehaviour, IInteractable
{
    
    public void Interact(GameObject interactor)
    {
        // Aquí va el código de matar
        Debug.Log("has sido asesinado por: " + interactor.name);
        // Por ejemplo, puedes llamar a un método del handler del jugador aquí si quieres.
    }
    
    public InteractType GetInteractType() => InteractType.Kill;
    
}
