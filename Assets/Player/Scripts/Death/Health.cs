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
        AudioManager.I.Play("vfx_dieplayer");

        // Reaparicion
        Invoke("Respawn", 0.5f);
        
    }

    public void Respawn()
    {
        
        controller.playerMovement.characterController.enabled = false;
        
        
        if (gameObject.name == "Player")
        {
            gameObject.transform.position = GameManager.Instance.puntosDeSpawn[0].position;
            print("primer spawn");
        }
        else
        {
            gameObject.transform.position = GameManager.Instance.puntosDeSpawn[1].position;
        }
        
        controller.playerMovement.characterController.enabled = true;
        controller.playerMovement.canMove = true;
        
        controller.playerMesh.SetActive(true);
        
    }
    
    private void OnCollisionEnter(Collision other)
    {
        if (other.gameObject.CompareTag("Knight"))
        {
            Die();
        }
    }
}
