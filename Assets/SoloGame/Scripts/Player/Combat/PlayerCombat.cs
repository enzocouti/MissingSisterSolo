using UnityEngine;

[RequireComponent(typeof(PlayerCombatInput))]
[RequireComponent(typeof(PlayerStateMachine))]
public class PlayerCombat : MonoBehaviour
{
    private PlayerCombatInput input;
    public PlayerStateMachine stateMachine;

    [Header("Cooldown Settings")]
    public float lightCooldown = 0.2f;
    public float heavyCooldown = 0.4f;
    private float attackCooldown = 0f;

    public PlayerAttackData lightAttack;
    public PlayerAttackData heavyAttack;

    private void Awake()
    {
        input = GetComponent<PlayerCombatInput>();
        stateMachine = GetComponent<PlayerStateMachine>();
    }

    private void OnEnable()
    {
        input.OnLightAttack += HandleLightAttack;
        input.OnHeavyAttack += HandleHeavyAttack;
        input.OnJump += HandleJump;
        input.OnLauncher += HandleLauncher;
    }

    private void OnDisable()
    {
        input.OnLightAttack -= HandleLightAttack;
        input.OnHeavyAttack -= HandleHeavyAttack;
        input.OnJump -= HandleJump;
        input.OnLauncher -= HandleLauncher;
    }

    private void Update()
    {
        attackCooldown -= Time.deltaTime;
    }

    private void HandleLightAttack()
    {
        if (attackCooldown > 0) return;

        attackCooldown = lightCooldown;
        stateMachine.ChangeState(new AttackState(this, lightAttack));
    }

    private void HandleHeavyAttack()
    {
        if (attackCooldown > 0) return;

        attackCooldown = heavyCooldown;
        stateMachine.ChangeState(new AttackState(this, heavyAttack));
    }

    private void HandleJump()
    {
        Debug.Log("Jump input!");
        // Jump state switch later
    }

    private void HandleLauncher()
    {
        Debug.Log("Launcher input!");
        // Launcher logic
    }
}
