using UnityEngine;

public class GrabObject : MonoBehaviour, IInteractable
{
    public void Interact(GameObject interactor)
    {
        
        Transform handTransform = interactor.GetComponent<PlayerInteractionHandler>().Hands.transform;
        if (handTransform != null)
        {
            
            transform.SetParent(handTransform, true);

            var rb = GetComponent<Rigidbody>();
            if (rb != null)
                rb.isKinematic = true;

            // Opcional: poner posición relativa a la mano
            transform.localPosition = Vector3.zero;
            print("lo tomo");
        }
        else
        {
            Debug.LogWarning($"{interactor.name} no tiene un hijo llamado 'Manos'");
        }
    }

    public InteractType GetInteractType() => InteractType.Grab;
}
