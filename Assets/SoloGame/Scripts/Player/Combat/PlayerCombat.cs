using System.Collections.Generic;
using UnityEngine;

public class PlayerCombat : MonoBehaviour
{
    [Header("Combo System")]
    [SerializeField] private List<ComboEntry> groundCombos;

    [Header("Settings")]
    public Transform hitboxOrigin;
    public float comboInputWindow = 0.4f; // Time allowed to continue the combo

    [Header("References")]
    public Rigidbody2D rb;
    public SpriteRenderer spriteRenderer;
    public PlayerCombatInput input;
    public PlayerStateMachine stateMachine;

    [HideInInspector] public bool isFacingRight = true;
    [HideInInspector] public bool isAttacking = false;
    [HideInInspector] public bool isDashInvincible = false;

    private string currentComboPattern = ""; // E.g. "J", "JJ", "JK", etc.
    private float comboTimer = 0f;
    private bool isComboActive = false; // Are we in a combo chain?

    [Header("Dash Settings")]
    public float dashDistance = 3f;
    public float dashDuration = 0.2f;
    public float dashInvincibleTime = 0.18f;

    private bool isDashing = false;

    private void Awake()
    {
        if (!input) input = GetComponent<PlayerCombatInput>();
        if (!stateMachine) stateMachine = GetComponent<PlayerStateMachine>();
        if (!rb) rb = GetComponent<Rigidbody2D>();
        if (!spriteRenderer) spriteRenderer = GetComponent<SpriteRenderer>();
    }

    private void OnEnable()
    {
        input.OnLightAttack += OnJPressed;
        input.OnHeavyAttack += OnKPressed;
        input.OnDash += HandleDash;
    }

    private void OnDisable()
    {
        input.OnLightAttack -= OnJPressed;
        input.OnHeavyAttack -= OnKPressed;
        input.OnDash -= HandleDash;
    }

    private void Update()
    {
        UpdateFacingDirection();

        // If combo is active, countdown window
        if (isComboActive && !isAttacking)
        {
            comboTimer -= Time.deltaTime;
            if (comboTimer <= 0f)
            {
                Debug.Log("[Combo] Combo timer expired, resetting.");
                ResetCombo();
            }
        }
    }

    private void UpdateFacingDirection()
    {
        if (isAttacking || isDashing) return;
        if (input.moveInput.x > 0.1f) FaceRight();
        else if (input.moveInput.x < -0.1f) FaceLeft();
    }

    private void FaceRight()
    {
        isFacingRight = true;
        spriteRenderer.flipX = false;
    }

    private void FaceLeft()
    {
        isFacingRight = false;
        spriteRenderer.flipX = true;
    }

    // === INPUT HANDLING ===

    private void OnJPressed()
    {
        HandleComboInput("J");
    }

    private void OnKPressed()
    {
        HandleComboInput("K");
    }

    private void HandleComboInput(string inputKey)
    {
        // Ignore if attacking (let attack finish)
        if (isAttacking) return;

        // If we're not in a combo, start one
        if (!isComboActive)
        {
            currentComboPattern = inputKey;
            isComboActive = true;
        }
        else
        {
            // Continue chain (append input)
            currentComboPattern += inputKey;
        }

        Debug.Log($"[ComboBuffer] Buffer now: {currentComboPattern}");

        var entry = groundCombos.Find(c => c.comboPattern == currentComboPattern);
        if (entry != null)
        {
            // Reset/combo timer for next input
            comboTimer = comboInputWindow;
            isAttacking = true;
            stateMachine.ChangeState(new AttackState(this, entry.attackData));
            Debug.Log($"[ComboBuffer] Matched: {currentComboPattern} -> {entry.attackData.attackName}");
        }
        else
        {
            Debug.Log($"[ComboBuffer] Pattern {currentComboPattern} not found, resetting.");
            ResetCombo();
        }
    }

    // Called by AttackState when attack ends
    public void OnAttackEnd()
    {
        isAttacking = false;
        if (isComboActive)
        {
            // After attack, give window for next input. If timer expires, combo resets in Update().
            comboTimer = comboInputWindow;
        }
    }

    private void ResetCombo()
    {
        isComboActive = false;
        currentComboPattern = "";
        comboTimer = 0f;
        isAttacking = false;
    }

    // === DASH ===

    private void HandleDash()
    {
        if (isAttacking || isDashing) return;
        StartCoroutine(DashRoutine());
    }

    private System.Collections.IEnumerator DashRoutine()
    {
        isDashing = true;
        isDashInvincible = true;

        Vector3 start = transform.position;
        float dir = isFacingRight ? 1f : -1f;
        Vector3 end = start + new Vector3(dashDistance * dir, 0f, 0f);

        float elapsed = 0f;
        while (elapsed < dashDuration)
        {
            float t = elapsed / dashDuration;
            transform.position = Vector3.Lerp(start, end, t);
            elapsed += Time.deltaTime;
            yield return null;
        }
        transform.position = end;

        yield return new WaitForSeconds(dashInvincibleTime);
        isDashInvincible = false;
        isDashing = false;
    }
}
