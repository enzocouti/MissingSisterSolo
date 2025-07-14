using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody2D))]
public class OverworldPlayerController : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 4.0f;

    private Rigidbody2D rb;
    private Animator animator;
    private Vector2 moveInput;
    private Vector2 lastMoveDir = Vector2.down; // Default facing down

    private bool inputEnabled = true;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
    }

    // --- DialogueManager support ---
    public void SetInputEnabled(bool enabled)
    {
        inputEnabled = enabled;
        if (!enabled)
            moveInput = Vector2.zero;
    }

    // Input System (Player Input component "Move" event)
    public void OnMove(InputValue value)
    {
        if (!inputEnabled)
        {
            moveInput = Vector2.zero;
            return;
        }
        moveInput = value.Get<Vector2>();

        if (moveInput.sqrMagnitude > 0.01f) // Update last move direction only if actually moving
        {
            lastMoveDir = moveInput.normalized;
        }
    }

    private void FixedUpdate()
    {
        // --- Movement ---
        if (inputEnabled && moveInput.sqrMagnitude > 0.01f)
        {
            rb.MovePosition(rb.position + moveInput.normalized * moveSpeed * Time.fixedDeltaTime);
        }

        // --- Animation blend tree control ---
        Vector2 animDir = moveInput.sqrMagnitude > 0.01f ? moveInput.normalized : lastMoveDir;
        animator.SetFloat("MoveX", animDir.x);
        animator.SetFloat("MoveY", animDir.y);
    }
}
