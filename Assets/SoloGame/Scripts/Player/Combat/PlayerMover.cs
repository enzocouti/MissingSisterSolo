using UnityEngine;

public class PlayerMover : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 5f;
    //testing fix move for combat player 
    private Rigidbody2D rb;
    private PlayerCombatInput input;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        input = GetComponent<PlayerCombatInput>();
    }

    private void FixedUpdate()
    {
       
        Vector2 move = input.moveInput;

        
        rb.linearVelocity = move * moveSpeed;
    }
}