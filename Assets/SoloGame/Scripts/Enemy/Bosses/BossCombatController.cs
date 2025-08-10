using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Animator))]
public class BossCombatController : MonoBehaviour
{
    [Header("Movement & Targeting")]
    public float detectionRange = 10f;
    public float attackRange = 1.2f;
    public float moveSpeed = 2.5f;

    [Header("Punch Attack")]
    public int punchDamage = 2;
    public float punchWindup = 0.18f;
    public float punchHitPause = 0.10f;
    public float punchCooldown = 1.5f;

    [Header("Slam Attack")]
    public int slamDamage = 5;
    public float slamRange = 1.8f;
    public float slamWindup = 0.7f;   // Matches your animator 
    public float slamImpactTime = 0.9f; // Matches your animator 
    public float slamCooldown = 3f;
    [Range(0f, 1f)] public float slamChance = 0.45f;
    public float slamStunDuration = 0.5f;

    [Header("Stunned")]
    public int stunThreshold = 4;      // How many hits before stunned
    public float stunResetTime = 2.0f; // Time window to count hits
    public float stunnedDuration = 1.3f; // How long to stay stunned
    private int hitsInWindow = 0;
    private float lastHitTime = -999f;
    private static readonly int HASH_STUNNED = Animator.StringToHash("Stunned");

    [Header("Death Sequence")]
    public float deathFreezeTime = 0.6f;
    public float deathSequenceDelay = 1.3f;

    [Header("Animation")]
    public Animator animator;
    private static readonly int HASH_SPEED = Animator.StringToHash("Speed");
    private static readonly int HASH_PUNCH = Animator.StringToHash("Punch");
    private static readonly int HASH_SLAM_WIND = Animator.StringToHash("SlamWindup");
    private static readonly int HASH_SLAM_IMP = Animator.StringToHash("SlamImpact");
    private static readonly int HASH_DIE = Animator.StringToHash("Die");

    private Transform player;
    private bool isAttacking;
    private bool isStunned;
    private bool isDead;
    private float nextSlamTime = 0f;
    private float nextPunchTime = 0f;
    private SpriteRenderer spriteRenderer;

    void Start()
    {
        if (animator == null) animator = GetComponent<Animator>();
        player = GameObject.FindGameObjectWithTag("Player")?.transform;
        spriteRenderer = GetComponent<SpriteRenderer>();
        nextSlamTime = Time.time + Random.Range(1.5f, 2.5f);
    }

    void Update()
    {
        if (isAttacking || isStunned || isDead || player == null) return;

        float dist = Vector3.Distance(transform.position, player.position);

        // Face player
        if (spriteRenderer)
            spriteRenderer.flipX = player.position.x < transform.position.x;

        if (dist > detectionRange)
        {
            animator.SetFloat(HASH_SPEED, 0f);
            return;
        }

        // Movement
        if (dist > attackRange)
        {
            Vector3 dir = (player.position - transform.position).normalized;
            transform.position += dir * moveSpeed * Time.deltaTime;
            animator.SetFloat(HASH_SPEED, moveSpeed);
        }
        else
        {
            animator.SetFloat(HASH_SPEED, 0f);
        }

        // Random Slam Logic: attempts slam at random intervals
        if (Time.time >= nextSlamTime && !isAttacking)
        {
            if (Random.value < slamChance)
            {
                StartCoroutine(SlamAttack());
                nextSlamTime = Time.time + slamCooldown;
                return;
            }
            else
            {
                // Even if boss skips slam push the next slam attempt into future
                nextSlamTime = Time.time + 1.3f;
            }
        }

        // Standard Punch only if in attack range after cooldown 
        if (dist <= attackRange && !isAttacking && Time.time >= nextPunchTime)
        {
            StartCoroutine(PunchAttack());
            nextPunchTime = Time.time + punchCooldown;
        }
    }

    IEnumerator PunchAttack()
    {
        isAttacking = true;
        if (SoundManager.Instance) SoundManager.Instance.PlayBossPunch();

        animator.SetTrigger(HASH_PUNCH);
        yield return new WaitForSeconds(punchWindup);

        if (player != null && Vector3.Distance(transform.position, player.position) <= attackRange)
        {
            player.GetComponent<PlayerHealth>()?.TakeDamage(punchDamage);
        }
        yield return new WaitForSeconds(punchHitPause);

        isAttacking = false;
    }

    IEnumerator SlamAttack()
    {
        isAttacking = true;
        if (SoundManager.Instance) SoundManager.Instance.PlayBossSlam();

        animator.SetTrigger(HASH_SLAM_WIND);
        yield return new WaitForSeconds(slamWindup);

        // Impact!
        if (SoundManager.Instance) SoundManager.Instance.PlayBossSlam();
        animator.SetTrigger(HASH_SLAM_IMP);

        var hits = Physics2D.OverlapCircleAll(transform.position, slamRange);
        foreach (var h in hits)
            if (h.CompareTag("Player"))
                h.GetComponent<PlayerHealth>()?.TakeDamage(slamDamage);

        yield return new WaitForSeconds(slamImpactTime);

        isAttacking = false;
    }

    // Called by BossHealth whenever boss takes damage
    public void RegisterHit()
    {
        // Track quick consecutive hits for stun
        if (Time.time - lastHitTime <= stunResetTime)
        {
            hitsInWindow++;
        }
        else
        {
            hitsInWindow = 1;
        }
        lastHitTime = Time.time;

        if (!isStunned && hitsInWindow >= stunThreshold)
        {
            StartCoroutine(StunRoutine());
            hitsInWindow = 0;
        }
    }

    IEnumerator StunRoutine()
    {
        isStunned = true;
        isAttacking = false;

        // Play stunned animation
        if (animator) animator.SetTrigger(HASH_STUNNED);

        float elapsed = 0f;
        while (elapsed < stunnedDuration)
        {
            // Boss can't move or attack while stunned
            yield return null;
            elapsed += Time.deltaTime;
        }

        isStunned = false;
    }

    public void PlayHurtSFX()
    {
        if (SoundManager.Instance) SoundManager.Instance.PlayBossHurt();
    }

    public void Die()
    {
        isDead = true;
        isAttacking = true;
        isStunned = true;

        if (SoundManager.Instance) SoundManager.Instance.PlayBossDeath();

        var coll = GetComponent<Collider2D>();
        if (coll) coll.enabled = false;

        StartCoroutine(DeathSequence());
    }

    IEnumerator DeathSequence()
    {
        yield return new WaitForSeconds(deathFreezeTime);

        if (animator) animator.SetTrigger(HASH_DIE);

        yield return new WaitForSeconds(deathSequenceDelay);

        CombatZoneManager.Instance?.NotifyEnemyKilled(gameObject);
        Destroy(gameObject);
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, slamRange);
    }
}
