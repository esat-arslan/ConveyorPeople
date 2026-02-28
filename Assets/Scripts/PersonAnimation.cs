using UnityEngine;

public class PersonAnimation : MonoBehaviour
{
    private Animator animator;
    private SpriteRenderer spriteRenderer;

    private void Awake()
    {
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    public void SetWalking(bool isWalking)
    {
        animator.SetBool("isWalking", isWalking);
    }

    public void SetDirection(bool IsGoingRight)
    {
        if(IsGoingRight) spriteRenderer.flipX = true;
        else spriteRenderer.flipX = false;
    }
}
