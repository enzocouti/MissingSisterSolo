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
    public float slamWindup = 0.5f;
    public float slamImpactTime = 0.10f;
    public float slamCooldown = 3f;
    [Range(0f, 1f)] public float slamChance = 0.45f;
    public float slamStunDuration = 0.5f;

    [Header("Death Sequence")]
    [Tooltip("Pause after boss health hits 0, before death animation (seconds)")]
    public float deathFreezeTime = 0.6f;
    [Tooltip("Pause after death animation before base cleared (seconds)")]
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
    private SpriteRenderer spriteRenderer;

    void Start()
    {
        if (animator == null) animator = GetComponent<Animator>();
        player = GameObject.FindGameObjectWithTag("Player")?.transform;
        spriteRenderer = GetComponent<SpriteRenderer>();
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

        if (dist > attackRange)
        {
            Vector3 dir = (player.position - transform.position).normalized;
            transform.position += dir * moveSpeed * Time.deltaTime;
            animator.SetFloat(HASH_SPEED, moveSpeed);
        }
        else
        {
            animator.SetFloat(HASH_SPEED, 0f);
            StartCoroutine(PerformAttack());
        }
    }

    IEnumerator PerformAttack()
    {
        isAttacking = true;

        bool doSlam = Time.time >= nextSlamTime && Random.value < slamChance;
        if (doSlam)
        {
            nextSlamTime = Time.time + slamCooldown;

            animator.SetTrigger(HASH_SLAM_WIND);
            yield return new WaitForSeconds(slamWindup);

            animator.SetTrigger(HASH_SLAM_IMP);
            var hits = Physics2D.OverlapCircleAll(transform.position, slamRange);
            foreach (var h in hits)
                if (h.CompareTag("Player"))
                    h.GetComponent<PlayerHealth>()?.TakeDamage(slamDamage);
            yield return new WaitForSeconds(slamImpactTime);

            isStunned = true;
            yield return new WaitForSeconds(slamStunDuration);
            isStunned = false;
        }
        else
        {
            animator.SetTrigger(HASH_PUNCH);
            yield return new WaitForSeconds(punchWindup);

            if (Vector3.Distance(transform.position, player.position) <= attackRange)
                player.GetComponent<PlayerHealth>()?.TakeDamage(punchDamage);
            yield return new WaitForSeconds(punchHitPause);

            float rem = punchCooldown - punchWindup - punchHitPause;
            if (rem > 0f) yield return new WaitForSeconds(rem);
        }

        isAttacking = false;
    }

    
    public void Die()
    {
        isDead = true;
        isAttacking = true;
        isStunned = true;

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
