using UnityEngine;

public class PlayerAnimationHandler : MonoBehaviour
{
    private Animator animator;

    private void Awake()
    {
        animator = GetComponentInChildren<Animator>();
    }

    public void SetMovementSpeed(float speed)
    {
        animator.SetFloat("movement", Mathf.Clamp01(speed));
    }

    public void PlayDash()
    {
        animator.SetTrigger("dash");
    }
    
}
