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

    private void Awake()
    {
        inputActions = new CombatInputActions();
    }

    private void OnEnable() => inputActions.Combat.Enable();
    private void OnDisable() => inputActions.Combat.Disable();

    private void Start()
    {
        inputActions.Combat.LightAttack.performed += ctx => OnLightAttack?.Invoke();
        inputActions.Combat.HeavyAttack.performed += ctx => OnHeavyAttack?.Invoke();
        inputActions.Combat.Jump.performed += ctx => OnJump?.Invoke();
        inputActions.Combat.Launcher.performed += ctx => OnLauncher?.Invoke();
    }

    private void Update()
    {
        moveInput = inputActions.Combat.Move.ReadValue<Vector2>();
    }
}