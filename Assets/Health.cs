using System;
using Unity.VisualScripting;
using UnityEngine;

public class Health : MonoBehaviour
{
    private PlayerController controller;
    private void Start()
    {
        controller = gameObject.GetComponent<PlayerController>();
    }

    public void Die()
    {
        
        /*
        controller.playerMovement.canMove = false;
        if (controller.playerInteractionHandler.isGrabingSomething)
        {
            Destroy(controller.playerInteractionHandler.GrabbedObject);
        }
        controller.animationHandler?.PlayDie();
        Invoke("Respawn", 3f);
        
        */
        
        /*
         TODO:
         Restringir movimiento - Listo
         Dropear objetos si es que carga algo - Listo
         Animacion de morir
         Si se desea poner un tiempo de espera
         Reaparicion
         */ 
    }

    public void Respawn()
    {
        if (gameObject.name == "Player")
        {
            transform.position = GameManager.Instance.puntosDeSpawn[0].transform.position;
        }
        else
        {
            transform.position = GameManager.Instance.puntosDeSpawn[1].transform.position;
        }

        controller.playerMovement.canMove = true;

    }
    
    
    private void OnCollisionEnter(Collision other)
    {
        if (other.gameObject.CompareTag("Knight"))
        {
            Die();
        }
    }
}
