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
        animator.SetFloat("movement", Mathf.Clamp01(speed));
    }

    public void PlayDash(bool condition)
    {
        animator.SetBool("dash", condition);
    }
    
    // Para animaciones de interacción, puede ser bool o trigger según el Animator
    public void PlayTake()
    {
        // Usando Trigger para asegurarte que siempre inicia la animación
        animator.SetTrigger("Take");
    }
    
    // Llamar esto desde un Animation Event al final de la animación de tomar/interactuar
    public void OnTakeAnimationEnd()
    {
        controller.playerMovement.canMove = true;
        Debug.Log("¡Terminó la animación de Take!");
    }
    
    
    public void ResetAll()
    {
        animator.SetBool("dash", false);
        animator.ResetTrigger("Take");
        animator.SetFloat("movement", 0f);
        
    }
    
}
