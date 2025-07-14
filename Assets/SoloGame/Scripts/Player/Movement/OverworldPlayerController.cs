using UnityEngine;
using UnityEngine.InputSystem;

// Handles movement in overworld using player input system

[RequireComponent(typeof(Rigidbody2D))]
public class OverworldPlayerController : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 4.0f;

    private Rigidbody2D rb;
    private Vector2 moveInput;
    private PlayerInputActions inputActions;
    private bool inputEnabled = true;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        inputActions = new PlayerInputActions();

        inputActions.Player.Move.performed += ctx =>
        {
            if (inputEnabled)
                moveInput = ctx.ReadValue<Vector2>();
            else
                moveInput = Vector2.zero;
        };
        inputActions.Player.Move.canceled += ctx => moveInput = Vector2.zero;
    }

    private void OnEnable()
    {
        inputActions.Player.Enable();
    }

    private void OnDisable()
    {
        inputActions.Player.Disable();
    }

    private void FixedUpdate()
    {
        rb.MovePosition(rb.position + moveInput * moveSpeed * Time.fixedDeltaTime);
    }

    // dialogue lock movement
    public void SetInputEnabled(bool enabled)
    {
        inputEnabled = enabled;
        if (!enabled) moveInput = Vector2.zero;
    }
}
