using UnityEngine;
using UnityEngine.InputSystem;

// Handles reading and exposing combat inputs, using the Unity Input System.

public class PlayerCombatInput : MonoBehaviour
{
    public Vector2 moveInput { get; private set; }
    public bool jumpPressed { get; private set; }

    private CombatInputActions inputActions;

    public System.Action OnLightAttack;
    public System.Action OnHeavyAttack;
    public System.Action OnJump;
    public System.Action OnLauncher;

    private bool inputEnabled = true;

    private void Awake()
    {
        inputActions = new CombatInputActions();
    }

    private void OnEnable() => inputActions.Combat.Enable();
    private void OnDisable() => inputActions.Combat.Disable();

    private void Start()
    {
        inputActions.Combat.LightAttack.performed += ctx => { if (inputEnabled) OnLightAttack?.Invoke(); };
        inputActions.Combat.HeavyAttack.performed += ctx => { if (inputEnabled) OnHeavyAttack?.Invoke(); };
        inputActions.Combat.Jump.performed += ctx => { if (inputEnabled) OnJump?.Invoke(); };
        inputActions.Combat.Launcher.performed += ctx => { if (inputEnabled) OnLauncher?.Invoke(); };
    }

    private void Update()
    {
        if (inputEnabled)
            moveInput = inputActions.Combat.Move.ReadValue<Vector2>();
        else
            moveInput = Vector2.zero;
    }

    // enable and disable 
 
    public void SetInputEnabled(bool enabled)
    {
        inputEnabled = enabled;
    }
}
