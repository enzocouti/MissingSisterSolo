using System.Collections.Generic;
using UnityEngine;

public class PlayerCombat : MonoBehaviour
{
    [Header("Combo System")]
    [SerializeField] private List<ComboEntry> groundCombos;
    [SerializeField] private List<ComboEntry> airCombos;
    [SerializeField] private PlayerAttackData launcherAttack;
    [SerializeField] private PlayerAttackData slamAttack; 

    [Header("Settings")]
    public Transform hitboxOrigin;
    public float comboResetTime = 0.6f;

    [Header("References")]
    public Rigidbody2D rb;
    public SpriteRenderer spriteRenderer;
    public PlayerCombatInput input;
    public PlayerStateMachine stateMachine;

    [HideInInspector] public bool isFacingRight = true;
    [HideInInspector] public bool isAirborne = false;
    [HideInInspector] public bool isBusy = false;

    [Header("Hitbox Settings")]
    public GameObject hitboxPrefab; // Assign your PlayerHitbox prefab here in Inspector!

    private string currentCombo = "";
    private float comboTimer = 0f;

    private void Awake()
    {
        if (!input) input = GetComponent<PlayerCombatInput>();
        if (!stateMachine) stateMachine = GetComponent<PlayerStateMachine>();
        if (!rb) rb = GetComponent<Rigidbody2D>();
        if (!spriteRenderer) spriteRenderer = GetComponent<SpriteRenderer>();
    }

    private void OnEnable()
    {
        input.OnLightAttack += HandleLight;
        input.OnHeavyAttack += HandleHeavy;
        input.OnLauncher += HandleLauncher;
        input.OnDash += HandleDash;
        input.OnJump += HandleJump;
    }

    private void OnDisable()
    {
        input.OnLightAttack -= HandleLight;
        input.OnHeavyAttack -= HandleHeavy;
        input.OnLauncher -= HandleLauncher;
        input.OnDash -= HandleDash;
        input.OnJump -= HandleJump;
    }

    private void Update()
    {
        UpdateFacingDirection();

        if (comboTimer > 0f)
        {
            comboTimer -= Time.deltaTime;
            if (comboTimer <= 0f)
                currentCombo = "";
        }
    }

    private void UpdateFacingDirection()
    {
        if (isBusy) return;

        if (input.MoveInput.x > 0.1f)
            FaceRight();
        else if (input.MoveInput.x < -0.1f)
            FaceLeft();
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

    private void HandleLight()
    {
        AddComboInput("J");
    }

    private void HandleHeavy()
    {
        AddComboInput("K");
    }

    private void HandleLauncher()
    {
        if (isBusy) return;

        if (isAirborne)
        {
            //  If airborne, use the slam attack instead of the launcher
            if (slamAttack != null)
                stateMachine.ChangeState(new AttackState(this, slamAttack));
        }
        else
        {
            if (launcherAttack != null)
                stateMachine.ChangeState(new AttackState(this, launcherAttack));
        }
    }

    private void HandleJump()
    {
        if (isBusy) return;
        stateMachine.ChangeState(new AirborneState(this));
    }

    private void HandleDash()
    {
        if (isBusy) return;
        stateMachine.ChangeState(new DashState(this));
    }

    private void AddComboInput(string inputKey)
    {
        if (isBusy) return;

        currentCombo += inputKey;
        comboTimer = comboResetTime;

        PlayerAttackData nextAttack = MatchCombo(currentCombo);
        if (nextAttack != null)
        {
            stateMachine.ChangeState(new AttackState(this, nextAttack));
        }
    }

    private PlayerAttackData MatchCombo(string combo)
    {
        List<ComboEntry> source = isAirborne ? airCombos : groundCombos;
        foreach (var entry in source)
        {
            if (entry.comboPattern == combo)
                return entry.attackData;
        }

        return null;
    }
}
