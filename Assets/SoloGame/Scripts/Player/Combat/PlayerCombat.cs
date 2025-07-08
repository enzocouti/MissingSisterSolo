using UnityEngine;
using System.Collections;

[RequireComponent(typeof(PlayerCombatInput))]
[RequireComponent(typeof(PlayerStateMachine))]
public class PlayerCombat : MonoBehaviour
{
    [Header("Components")]
    public Rigidbody2D rb;
    public SpriteRenderer spriteRenderer;
    public Transform hitboxOrigin;

    [HideInInspector] public PlayerCombatInput input;
    [HideInInspector] public PlayerStateMachine stateMachine;

    [Header("Attack Data")]
    public PlayerAttackData[] comboSequence;
    public PlayerAttackData launcherAttack;
    public PlayerAttackData airLightAttack;
    public PlayerAttackData airHeavyAttack;

    [Header("Jump / Dash")]
    public float jumpDuration = 0.4f; // fake air time
    public float dashSpeed = 12f;
    public float dashDuration = 0.2f;

    [Header("State")]
    public bool IsGrounded { get; private set; } = true;
    public bool isFacingRight = true;

    private int currentComboIndex = 0;
    private float comboTimer = 0f;
    public float comboResetTime = 0.5f;

    private void Awake()
    {
        input = GetComponent<PlayerCombatInput>();
        stateMachine = GetComponent<PlayerStateMachine>();
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
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
        UpdateFacingDirection();
        UpdateComboTimer();
    }

    private void UpdateFacingDirection()
    {
        if (input.moveInput.x > 0.01f) FaceRight();
        else if (input.moveInput.x < -0.01f) FaceLeft();
    }

    private void UpdateComboTimer()
    {
        if (comboTimer > 0)
        {
            comboTimer -= Time.deltaTime;
            if (comboTimer <= 0)
                currentComboIndex = 0;
        }
    }

    private void FaceRight()
    {
        if (!isFacingRight)
        {
            isFacingRight = true;
            spriteRenderer.flipX = false;
        }
    }

    private void FaceLeft()
    {
        if (isFacingRight)
        {
            isFacingRight = false;
            spriteRenderer.flipX = true;
        }
    }

    private void HandleLightAttack()
    {
        if (!IsGrounded)
        {
            stateMachine.ChangeState(new AttackState(this, airLightAttack));
            return;
        }

        PlayerAttackData attack = comboSequence[Mathf.Clamp(currentComboIndex, 0, comboSequence.Length - 1)];
        stateMachine.ChangeState(new AttackState(this, attack));
        currentComboIndex++;
        comboTimer = comboResetTime;
    }

    private void HandleHeavyAttack()
    {
        if (!IsGrounded)
        {
            stateMachine.ChangeState(new AttackState(this, airHeavyAttack));
            return;
        }

        stateMachine.ChangeState(new AttackState(this, comboSequence[comboSequence.Length - 1])); // heavy = last combo
        currentComboIndex = 0;
        comboTimer = 0;
    }

    private void HandleLauncher()
    {
        if (!IsGrounded) return;

        stateMachine.ChangeState(new AttackState(this, launcherAttack));
        currentComboIndex = 0;
        comboTimer = 0;
    }

    private void HandleJump()
    {
        if (!IsGrounded) return;

        IsGrounded = false;
        StartCoroutine(SimulateJump());
    }

    private IEnumerator SimulateJump()
    {
        Debug.Log("Jump started");
        float elapsed = 0f;

        while (elapsed < jumpDuration)
        {
            elapsed += Time.deltaTime;
            yield return null;
        }

        IsGrounded = true;
        Debug.Log("Landed");
    }

    public void Dash()
    {
        Vector2 dashDirection = isFacingRight ? Vector2.right : Vector2.left;
        rb.linearVelocity = dashDirection * dashSpeed;
        // start dash coroutine for I-frames later
    }

    // draw gizmos or debug visual helpers here
}
