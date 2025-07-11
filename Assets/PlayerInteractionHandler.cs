using System;
using System.Collections;
using Unity.VisualScripting;
using UnityEngine;

public class PlayerInteractionHandler : MonoBehaviour
{
    [SerializeField] private PlayerController controller;
    [SerializeField] private GameObject Hands;
    
    public GameObject interableObject;
    
    private void OnEnable()
    {
        if (controller != null && controller.inputReader != null)
            controller.inputReader.OnInteract += Interact;
        interableObject = null;
    }

    private void OnDisable()
    {
        controller.inputReader.OnInteract -= Interact;
    }

    public void Interact()
    {
        if (interableObject == null)
            return;
        
        if (controller != null)
            controller.playerMovement.canMove = false;
        
        // Apaga físicas temporalmente (pero solo cuando termine el Lerp se pone isKinematic)
        StartCoroutine(LerpObjectToHand(interableObject, Hands.transform, 0.35f));
        
        controller.animationHandler?.PlayTake();
        Debug.Log("Toma");
        
    }
    
    private IEnumerator LerpObjectToHand(GameObject obj, Transform hand, float duration)
    {
        // Guarda posiciones y rotaciones iniciales y finales
        Vector3 startPos = obj.transform.position;
        Quaternion startRot = obj.transform.rotation;
        Vector3 endPos = hand.position;
        Quaternion endRot = hand.rotation;

        float elapsed = 0f;
        while (elapsed < duration)
        {
            float t = elapsed / duration;
            obj.transform.position = Vector3.Lerp(startPos, endPos, t);
            obj.transform.rotation = Quaternion.Slerp(startRot, endRot, t);
            elapsed += Time.deltaTime;
            yield return null;
        }

        // Al final asegúrate que está en la mano
        obj.transform.position = endPos;
        obj.transform.rotation = endRot;

        // Hacerlo hijo de la mano (usa false para adoptar la rotación/posición local de la mano)
        obj.transform.SetParent(hand, false);

        // Apaga físicas
        var rb = obj.GetComponent<Rigidbody>();
        if (rb != null)
            rb.isKinematic = true;
    }
    
    private void OnTriggerEnter(Collider other)
    {
        interableObject = other.gameObject;
    }

    private void OnTriggerExit(Collider other)
    {
        interableObject = null;
    }
}
