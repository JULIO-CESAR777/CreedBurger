using System;
using System.Collections;
using Unity.VisualScripting;
using UnityEngine;

public class PlayerInteractionHandler : MonoBehaviour
{
    [SerializeField] private PlayerController controller;
    [SerializeField] private GameObject Hands;
    
    public GameObject interableObject;

    private void Start()
    {
        if (controller != null && controller.inputReader != null)
            controller.inputReader.OnInteract += Interact;
        interableObject = null;
    }

    private void OnDestroy()
    {
        controller.inputReader.OnInteract -= Interact;
    }
    

    public void Interact()
    {

        Debug.Log("Presiono el interact");
        if (interableObject == null)
            return;
        
        if (controller != null)
            controller.playerMovement.canMove = false;
        
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
