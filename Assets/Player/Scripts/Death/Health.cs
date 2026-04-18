using System;
using UnityEngine;

public class Health : MonoBehaviour
{
    private PlayerController controller;
    public bool isDead;
    
    
    [Header("Visual Effects")]
    public GameObject splashEffectPrefab;
    private void Start()
    {
        controller = gameObject.GetComponent<PlayerController>();
        isDead = false;
    }

    public void Die()
    {
        if (isDead) return; // 🔥 ESTA ES LA CLAVE
        if (controller.isPaused) return;

        isDead = true;

        print("murio");

        controller.playerMovement.canMove = false;

        // Dropear objetos
        if (controller.playerInteractionHandler.isGrabingSomething)
        {
            Destroy(controller.playerInteractionHandler.GrabbedObject);
            controller.playerInteractionHandler.GrabbedObject = null;
        }

        controller.playerMesh.SetActive(false);

        // Splash
        if (splashEffectPrefab != null)
        {
            Instantiate(splashEffectPrefab, transform.position, Quaternion.Euler(90, 0, 0));
        }

        AudioManager.I.Play("vfx_dieplayer");

        ScoreSystem.Instance.MinusScore(5);

        Invoke(nameof(Respawn), 1.0f);
        
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
        isDead = false;
        
    }
    
    private void OnCollisionEnter(Collision other)
    {
        if (isDead) return;
        
        if (other.gameObject.CompareTag("Knight"))
        {
            Die();
        }
    }
    
}
