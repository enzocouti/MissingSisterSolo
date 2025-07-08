using UnityEngine;

public class PlayerMover : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 5f;
    //testing fix move for combat player 
    private Rigidbody2D rb;
    private PlayerCombatInput input;
    private PlayerCombat combat;

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
            return;
        }

        Vector2 move = input.moveInput;
        rb.linearVelocity = move * moveSpeed;
    }
}