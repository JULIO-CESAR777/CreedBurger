using System;
using Unity.VisualScripting;
using UnityEngine;

public class Health : MonoBehaviour
{
    private PlayerController controller;
    
    [Header("Visual Effects")]
    public GameObject splashEffectPrefab;
    private void Start()
    {
        controller = gameObject.GetComponent<PlayerController>();
    }

    public void Die()
    {
        
        // Restringir movimiento - Listo
        controller.playerMovement.canMove = false;
        
        // Dropear objetos si es que carga algo - Listo
        if (controller.playerInteractionHandler.isGrabingSomething)
        {
            Destroy(controller.playerInteractionHandler.GrabbedObject);
            controller.playerInteractionHandler.GrabbedObject = null;
        }
        
        controller.playerMesh.SetActive(false);
        
        // 5. Instanciar splash effect
        if (splashEffectPrefab != null)
        {
            Instantiate(splashEffectPrefab, transform.position, Quaternion.Euler(90,0,0));
        }
        
        // Reaparicion
        Invoke("Respawn", 3f);
        
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
        
        controller.playerMesh.SetActive(true);

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
