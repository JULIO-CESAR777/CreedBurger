using UnityEngine;

public class PlayerAnimationHandler : MonoBehaviour
{
    private Animator animator;
    
    private PlayerController controller;

    private void Awake()
    {
        animator = GetComponentInChildren<Animator>();
        controller = GetComponent<PlayerController>();
    }

    public void SetMovementSpeed(float speed)
    {
        animator.SetFloat("Movement", Mathf.Clamp01(speed));
    }

    public void PlayIdle()
    {
        animator.SetTrigger("Drop");
    }

    public void PlayDash(bool condition)
    {
        animator.SetBool("Dash", condition);
    }
    
    // Para animaciones de interacción, puede ser bool o trigger según el Animator
    public void PlayTake()
    {
        // Usando Trigger para asegurarte que siempre inicia la animación
        animator.SetTrigger("Take");
    }

    public void PlayKill()
    {
        animator.SetTrigger("Kill");
    }

    public void PlayDie()
    {
        animator.SetTrigger("DIE");
    }

    public void PlayClean()
    {
        animator.SetTrigger("Clean");
    }
    
    public void PlayCut()
    {
        animator.SetTrigger("Cut");
    }
    
    // Llamar esto desde un Animation Event al final de la animación de tomar/interactuar
    public void CanMove()
    {
        controller.playerMovement.canMove = true;
        controller.suspect = false;
    }
    
    
    public void ResetAll()
    {
        animator.SetBool("dash", false);
        animator.ResetTrigger("Take");
        animator.SetFloat("movement", 0f);
        
    }
    
}
