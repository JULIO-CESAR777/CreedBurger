using System;
using UnityEngine;

public class Player_Interact : MonoBehaviour
{
    
    public GameObject interactObject;
    
    void Start()
    {
        interactObject = null;
    }

    private void OnTriggerEnter(Collider other)
    {
        interactObject = other.gameObject;
        Debug.Log(other.gameObject.name);
    }

    private void OnTriggerExit(Collider other)
    {
        interactObject = null;
    }
}
