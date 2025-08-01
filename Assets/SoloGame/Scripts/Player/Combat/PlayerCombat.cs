using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerCombat : MonoBehaviour
{
    [Header("Combo System")]
    [SerializeField] private List<ComboEntry> groundCombos;

    [Header("Settings")]
    public Transform hitboxOrigin;
    public float comboInputWindow = 0.5f;

    [Header("References")]
    public Rigidbody2D rb;
    public PlayerCombatInput input;
    public PlayerStateMachine stateMachine;

    [HideInInspector] public bool isFacingRight = true;
    [HideInInspector] public bool isAttacking = false;
    [HideInInspector] public bool isDashInvincible = false;

    private string currentCombo = "";
    private Queue<string> bufferedInputs = new Queue<string>();
    private float bufferTimer = 0f;
    private bool waitingForInput = false;

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
    }

    private void OnEnable()
    {
        input.OnLightAttack += () => HandleInput("J");
        input.OnHeavyAttack += () => HandleInput("K");
        input.OnDash += HandleDash;
    }

    private void OnDisable()
    {
        input.OnLightAttack -= () => HandleInput("J");
        input.OnHeavyAttack -= () => HandleInput("K");
        input.OnDash -= HandleDash;
    }

    private void Update()
    {
        UpdateFacingDirection();

        if (waitingForInput)
        {
            bufferTimer -= Time.deltaTime;
            if (bufferTimer <= 0f)
            {
                waitingForInput = false;
                bufferedInputs.Clear();
                currentCombo = "";
                Debug.Log("[ComboBuffer] Combo window ended. Reset combo.");
            }
        }
    }

    private void UpdateFacingDirection()
    {
        if (isAttacking || isDashing) return;

        float xInput = input.moveInput.x;
        if (xInput > 0.1f) FaceRight();
        else if (xInput < -0.1f) FaceLeft();
    }

    private void FaceRight()
    {
        isFacingRight = true;
        transform.localScale = new Vector3(1f, 1f, 1f);
    }

    private void FaceLeft()
    {
        isFacingRight = false;
        transform.localScale = new Vector3(-1f, 1f, 1f);
    }

    private void HandleInput(string key)
    {
        if (!isAttacking)
        {
            currentCombo = key;
            StartComboAttack(currentCombo);
        }
        else
        {
            bufferedInputs.Enqueue(key);
            bufferTimer = comboInputWindow;
            waitingForInput = true;
            Debug.Log($"[ComboBuffer] Buffered: {string.Join("", bufferedInputs)} after {currentCombo}");
        }
    }

    private void StartComboAttack(string pattern)
    {
        var entry = groundCombos.Find(c => c.comboPattern == pattern);
        if (entry != null && entry.attackData != null)
        {
            isAttacking = true;
            stateMachine.ChangeState(new AttackState(this, entry.attackData));
            Debug.Log($"[ComboBuffer] Started attack: {entry.comboPattern} ({entry.attackData.attackName})");
        }
        else
        {
            Debug.Log($"[ComboBuffer] No combo found for {pattern}, resetting.");
            isAttacking = false;
            currentCombo = "";
            bufferedInputs.Clear();
        }
    }

    public void OnAttackEnd()
    {
        isAttacking = false;

        if (bufferedInputs.Count > 0)
        {
            currentCombo += bufferedInputs.Dequeue();
            Debug.Log($"[ComboBuffer] Advancing to: {currentCombo}");
            StartComboAttack(currentCombo);
        }
        else
        {
            waitingForInput = true;
            bufferTimer = comboInputWindow;
        }
    }

    private void HandleDash()
    {
        if (isAttacking || isDashing) return;
        StartCoroutine(DashRoutine());
    }

    private IEnumerator DashRoutine()
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
