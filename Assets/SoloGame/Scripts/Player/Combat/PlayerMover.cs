using UnityEngine;

public class PlayerMover : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 5f;
    private Rigidbody2D rb;
    private PlayerCombatInput input;
    private PlayerCombat combat;

    
    private float stepTimer = 0f;
    [SerializeField] private float stepInterval = 0.35f; 
    private bool wasMoving = false;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        input = GetComponent<PlayerCombatInput>();
        combat = GetComponent<PlayerCombat>();
    }

    private void FixedUpdate()
    {
        if (combat.isAttacking)
        {
            rb.linearVelocity = Vector2.zero;
            StopStepSound();
            return;
        }

        Vector2 move = input.moveInput;
        rb.linearVelocity = move * moveSpeed;

        bool isMoving = move.magnitude > 0.1f;

        if (isMoving)
        {
            stepTimer -= Time.fixedDeltaTime;
            if (stepTimer <= 0f)
            {
                PlayStepSound();
                stepTimer = stepInterval;
            }
        }
        else if (wasMoving)
        {
            StopStepSound();
        }

        wasMoving = isMoving;
    }

    private void PlayStepSound()
    {
        if (SoundManager.Instance)
            SoundManager.Instance.PlayMovement();
    }

    private void StopStepSound()
    {
        
    }
}
