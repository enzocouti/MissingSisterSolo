using UnityEngine;
using UnityEngine.InputSystem;

public class OverworldPlayerController : MonoBehaviour
{
    public float moveSpeed = 4f;
    Rigidbody2D rb;
    Animator animator;
    Vector2 moveInput;
    Vector2 lastDir = Vector2.down; // Default facing down
    bool inputEnabled = true;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
    }

    public void SetInputEnabled(bool enabled)
    {
        inputEnabled = enabled;
        if (!enabled)
            moveInput = Vector2.zero;
    }

    // Called by PlayerInput "Move" action
    public void OnMove(InputValue value)
    {
        if (!inputEnabled)
        {
            moveInput = Vector2.zero;
            return;
        }
        moveInput = value.Get<Vector2>();
    }

    void FixedUpdate()
    {
        Vector2 snapped = SnapToCardinal(moveInput);

        // --- MOVEMENT ---
        if (inputEnabled && snapped != Vector2.zero)
        {
            lastDir = snapped;
            rb.MovePosition(rb.position + snapped * moveSpeed * Time.fixedDeltaTime);
            animator.SetBool("isMoving", true);
            animator.SetFloat("MoveX", snapped.x);
            animator.SetFloat("MoveY", snapped.y);
        }
        else
        {
            // When idle, set isMoving false, but keep facing last direction
            animator.SetBool("isMoving", false);
            animator.SetFloat("MoveX", lastDir.x);
            animator.SetFloat("MoveY", lastDir.y);
        }
    }

    // Helper: snap to up/down/left/right (no diagonals)
    Vector2 SnapToCardinal(Vector2 v)
    {
        if (v == Vector2.zero) return Vector2.zero;
        return (Mathf.Abs(v.x) > Mathf.Abs(v.y)) ? new Vector2(Mathf.Sign(v.x), 0) : new Vector2(0, Mathf.Sign(v.y));
    }
}
