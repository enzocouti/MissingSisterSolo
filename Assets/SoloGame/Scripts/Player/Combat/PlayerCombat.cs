using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[RequireComponent(typeof(PlayerCombatInput))]
[RequireComponent(typeof(PlayerStateMachine))]
public class PlayerCombat : MonoBehaviour
{
    
    public Rigidbody2D rb;
    public SpriteRenderer spriteRenderer;
    public Transform hitboxOrigin;

    public PlayerCombatInput input;
    public PlayerStateMachine stateMachine;

    
    public PlayerAttackData[] comboSequence;
    public PlayerAttackData launcherAttack;
    public PlayerAttackData airLightAttack;
    public PlayerAttackData airHeavyAttack;

    
    public float jumpHeight = 2f;
    public float jumpDuration = 0.5f;
    public bool isJumping { get; private set; }
    public bool IsGrounded { get; private set; } = true;

    
    public bool isFacingRight = true;
    public float dashSpeed = 12f;
    public bool isAttacking = false;

    //dash
    public float dashDistance = 3f;
    public float dashDuration = 0.2f;
    public string dashIgnoreTag = "Enemy";
    private bool isDashing = false;

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
        input.OnDash += HandleDash;
    }

    private void OnDisable()
    {
        input.OnLightAttack -= HandleLightAttack;
        input.OnHeavyAttack -= HandleHeavyAttack;
        input.OnJump -= HandleJump;
        input.OnLauncher -= HandleLauncher;
        input.OnDash -= HandleDash;
    }

    private void Update()
    {
        UpdateFacingDirection();
        UpdateComboTimer();
    }

    private void UpdateFacingDirection()
    {
        if (isAttacking) return;

        if (input.moveInput.x > 0.01f) FaceRight();
        else if (input.moveInput.x < -0.01f) FaceLeft();
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

    private void UpdateComboTimer()
    {
        if (comboTimer > 0)
        {
            comboTimer -= Time.deltaTime;
            if (comboTimer <= 0)
                currentComboIndex = 0;
        }
    }

    private void HandleLightAttack()
    {
        if (!IsGrounded)
        {
            StartAttack(airLightAttack);
            return;
        }

        PlayerAttackData attack = comboSequence[Mathf.Clamp(currentComboIndex, 0, comboSequence.Length - 1)];
        StartAttack(attack);
        currentComboIndex++;
        comboTimer = comboResetTime;
    }

    private void HandleHeavyAttack()
    {
        if (!IsGrounded)
        {
            StartAttack(airHeavyAttack);
            return;
        }

        StartAttack(comboSequence[comboSequence.Length - 1]);
        currentComboIndex = 0;
        comboTimer = 0;
    }

    private void HandleLauncher()
    {
        if (!IsGrounded || isAttacking) return;

        Debug.Log("Launcher input");
        StartAttack(launcherAttack);
        currentComboIndex = 0;
        comboTimer = 0;
    }

    private void HandleJump()
    {
        if (!IsGrounded || isJumping) return;
        StartCoroutine(SimulateJump());
    }

    private void HandleDash()
    {
        if (isAttacking || isJumping || isDashing) return;
        StartCoroutine(DashCoroutine());
        Debug.Log("Dash");
    }

    private void StartAttack(PlayerAttackData attack)
    {
        isAttacking = true;
        rb.linearVelocity = Vector2.zero;
        stateMachine.ChangeState(new AttackState(this, attack));
    }

    public void OnAttackEnd()
    {
        isAttacking = false;
    }

    private IEnumerator SimulateJump()
    {
        isJumping = true;
        IsGrounded = false;

        Vector3 startPos = transform.position;
        float timer = 0f;

        bool moving = Mathf.Abs(input.moveInput.x) > 0.01f;
        Vector3 jumpTarget = startPos;
        string jumpType = "Straight jump";

        if (moving)
        {
            float arcDistance = 1.2f;
            float horizontalOffset = isFacingRight ? arcDistance : -arcDistance;
            jumpTarget = startPos + new Vector3(horizontalOffset, 0, 0);
            jumpType = isFacingRight ? "Directional jump RIGHT" : "Directional jump LEFT";
        }

        Debug.Log(jumpType);

        while (timer < jumpDuration)
        {
            float progress = timer / jumpDuration;
            float yOffset = Mathf.Sin(progress * Mathf.PI) * jumpHeight;
            Vector3 horizontal = Vector3.Lerp(startPos, jumpTarget, progress);
            transform.position = new Vector3(horizontal.x, startPos.y + yOffset, startPos.z);

            timer += Time.deltaTime;
            yield return null;
        }

        transform.position = new Vector3(jumpTarget.x, startPos.y, startPos.z);
        isJumping = false;
        IsGrounded = true;
    }

    private IEnumerator DashCoroutine()
    {
        isDashing = true;

        Collider2D[] allCols = GetComponents<Collider2D>();
        List<Collider2D> ignored = new List<Collider2D>();

        Collider2D[] enemyColliders = GameObject.FindObjectsByType<Collider2D>(FindObjectsSortMode.None);

        foreach (var col in enemyColliders)
        {
            if (col.CompareTag(dashIgnoreTag))
            {
                foreach (var myCol in allCols)
                    Physics2D.IgnoreCollision(myCol, col, true);
                ignored.Add(col);
            }
        }

        Vector3 start = transform.position;
        float direction = isFacingRight ? 1f : -1f;
        Vector3 target = start + new Vector3(dashDistance * direction, 0f, 0f);

        float elapsed = 0f;
        while (elapsed < dashDuration)
        {
            float t = elapsed / dashDuration;
            transform.position = Vector3.Lerp(start, target, t);
            elapsed += Time.deltaTime;
            yield return null;
        }

        transform.position = target;

        foreach (var col in ignored)
        {
            foreach (var myCol in allCols)
                Physics2D.IgnoreCollision(myCol, col, false);
        }

        isDashing = false;
    }
}
