using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerCombatInput : MonoBehaviour
{
    public Vector2 moveInput { get; private set; }
    public bool jumpPressed { get; private set; }

    private CombatInputActions inputActions;

    public System.Action OnLightAttack;
    public System.Action OnHeavyAttack;
    public System.Action OnJump;
    public System.Action OnLauncher;
    public System.Action OnDash;

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
        inputActions.Combat.Dash.performed += ctx => { if (inputEnabled) OnDash?.Invoke(); };
    }

    private void Update()
    {
        moveInput = inputEnabled ? inputActions.Combat.Move.ReadValue<Vector2>() : Vector2.zero;
    }

    public void SetInputEnabled(bool enabled)
    {
        inputEnabled = enabled;
    }
}