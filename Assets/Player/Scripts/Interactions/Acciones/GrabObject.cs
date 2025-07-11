using UnityEngine;

public class GrabObject : MonoBehaviour, IInteractable
{
    
    public bool isGrabbed = false;
    
    public void Interact(GameObject interactor)
    {

        if (isGrabbed == false)
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
                isGrabbed = true;
            }
            else
            {
                Debug.LogWarning($"{interactor.name} no tiene un hijo llamado 'Manos'");
            }    
        }
        else
        {
            print("se llamo al drop dentro del grab");
            // Quitar el objeto de la mano
            transform.SetParent(null, true);

            // Activar la física si tiene Rigidbody
            var rb = GetComponent<Rigidbody>();
            if (rb != null)
                rb.isKinematic = false;

            // Opcional: darle un pequeño empuje al soltar
            rb.AddForce(interactor.transform.forward * 2f, ForceMode.Impulse);
            isGrabbed = false;
        }


    }

    public InteractType GetInteractType() => InteractType.Grab;
}
