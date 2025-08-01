using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class EnemyCombatController : MonoBehaviour
{
    [Header("AI & Combat")]
    public float moveSpeed = 2.5f;
    public float detectionRange = 7f;
    public float attackRange = 1.2f;
    public float attackCooldown = 1.4f;
    public int touchDamage = 2;
    [Tooltip("Base hitstop time after being hit")]
    public float baseHurtPause = 0.28f;

    [Header("Crowd Logic")]
    [Range(1, 5)] public int maxSimultaneousAttackers = 2;
    public float bufferDistance = 1.1f;
    public float waitShuffleSpeed = 0.13f;
    public float waitMinDistance = 1.8f;

    [Header("Death FX")]
    public float deathLayTime = 0.7f;
    public float deathFlashTime = 0.8f;
    public float deathFlashInterval = 0.13f;

    public static List<EnemyCombatController> allEnemies = new List<EnemyCombatController>();
    private static int currentAttackers = 0;

    protected Transform player;
    public bool isAttacking, isDead, isHurt, isLaunched;
    protected bool isWaiting;

    protected SpriteRenderer spriteRenderer;
    public Color baseColor;

    float shuffleTimer;
    Vector2 waitShuffleDir;

    void OnEnable() => allEnemies.Add(this);
    void OnDisable() { allEnemies.Remove(this); if (isAttacking) currentAttackers--; }

    void Start()
    {
        player = GameObject.FindWithTag("Player")?.transform;
        spriteRenderer = GetComponent<SpriteRenderer>();
        baseColor = spriteRenderer?.color ?? Color.white;
    }

    void Update()
    {
        if (isDead || isHurt || isLaunched || player == null) return;

        // Face player
        spriteRenderer.flipX = player.position.x < transform.position.x;

        Vector2 myPos = transform.position;

        // Buffer from other enemies
        foreach (var o in allEnemies)
        {
            if (o != null && o != this && !o.isDead)
            {
                Vector2 oPos = o.transform.position;
                float d = Vector2.Distance(myPos, oPos);
                if (d < bufferDistance)
                {
                    Vector2 push = (myPos - oPos).normalized;
                    transform.position += (Vector3)(push * (bufferDistance - d) * 0.2f);
                }
            }
        }

        float distToPlayer = Vector2.Distance(myPos, (Vector2)player.position);

        // Decide attack vs wait
        bool canAttack = (!isAttacking && !isWaiting && currentAttackers < maxSimultaneousAttackers) || isAttacking;
        if (canAttack)
        {
            Vector2 dir = ((Vector2)player.position - myPos).normalized;
            if (distToPlayer > attackRange)
                transform.position += (Vector3)dir * moveSpeed * Time.deltaTime;
            else if (!isAttacking)
            {
                isAttacking = true;
                currentAttackers++;
                StartCoroutine(AttackCoroutine());
            }
        }
        else
        {
            // WAITING behavior
            if (distToPlayer < waitMinDistance)
            {
                Vector2 away = (myPos - (Vector2)player.position).normalized;
                transform.position += (Vector3)away * waitShuffleSpeed * Time.deltaTime;
            }
            else
            {
                shuffleTimer -= Time.deltaTime;
                if (shuffleTimer <= 0f)
                {
                    waitShuffleDir = Random.insideUnitCircle.normalized;
                    shuffleTimer = Random.Range(0.25f, 0.6f);
                }
                transform.position += (Vector3)waitShuffleDir * waitShuffleSpeed * Time.deltaTime * 0.5f;
            }
            isWaiting = true;
        }
    }

    //  Make this virtual so bosses can override
    protected virtual IEnumerator AttackCoroutine()
    {
        float windup = 0.17f, pause = 0.10f;
        spriteRenderer.color = Color.yellow;
        yield return new WaitForSeconds(windup);

        player?.GetComponent<PlayerHealth>()?.TakeDamage(touchDamage);
        yield return new WaitForSeconds(pause);

        spriteRenderer.color = baseColor;
        yield return new WaitForSeconds(attackCooldown - windup - pause);

        isAttacking = false;
        currentAttackers = Mathf.Max(0, currentAttackers - 1);
    }

    public void OnHurt(PlayerAttackData data, bool facingRight)
    {
        if (isDead) return;
        float hp = (data != null && data.hitPause > 0f) ? data.hitPause : baseHurtPause;
        StartCoroutine(HitFeedback(hp, data, facingRight));
    }

    IEnumerator HitFeedback(float pause, PlayerAttackData data, bool facingRight)
    {
        isHurt = true;
        spriteRenderer.color = Color.red;
        yield return new WaitForSecondsRealtime(pause);

        if (data.isLauncher) yield return StartCoroutine(Launch(data, facingRight));
        else if (data.isHeavy) yield return StartCoroutine(Knockback(data, facingRight));
        else
        {
            Vector3 o = transform.position;
            for (int i = 0; i < 3; i++)
            {
                transform.position = o + (Vector3)Random.insideUnitCircle * 0.09f;
                yield return new WaitForSeconds(0.03f);
            }
            transform.position = o;
        }

        spriteRenderer.color = baseColor;
        isHurt = false;
    }

    public void OnDeath()
    {
        isDead = true;
        StopAllCoroutines();
        if (isAttacking) currentAttackers = Mathf.Max(0, currentAttackers - 1);
        spriteRenderer.color = Color.gray;
        StartCoroutine(DeathSequence());
    }

    IEnumerator DeathSequence()
    {
        yield return new WaitForSeconds(deathLayTime);

        float t = 0f;
        bool vis = true;
        while (t < deathFlashTime)
        {
            vis = !vis;
            spriteRenderer.enabled = vis;
            yield return new WaitForSeconds(deathFlashInterval);
            t += deathFlashInterval;
        }

        Destroy(gameObject);
    }

    IEnumerator Knockback(PlayerAttackData d, bool f)
    {
        isLaunched = true;
        Vector2 start = transform.position;
        Vector2 end = start + Vector2.right * (f ? d.knockbackForce : -d.knockbackForce);
        float t = 0f, dur = 0.35f, h = 1f;

        while (t < dur)
        {
            float p = t / dur;
            float y = Mathf.Sin(p * Mathf.PI) * h;
            transform.position = Vector2.Lerp(start, end, p) + Vector2.up * y;
            t += Time.deltaTime;
            yield return null;
        }

        transform.position = end;
        isLaunched = false;
    }

    IEnumerator Launch(PlayerAttackData d, bool f)
    {
        isLaunched = true;
        Vector3 start = transform.position;
        float t = 0f, dur = d.launchDuration;
        while (t < dur)
        {
            float p = t / dur;
            transform.position = start + Vector3.up * (Mathf.Sin(p * Mathf.PI) * d.launchHeight);
            t += Time.deltaTime;
            yield return null;
        }
        transform.position = start + Vector3.up * d.launchHeight;
        yield return new WaitForSeconds(d.launchHangTime);

        float fall = dur * 0.8f; t = 0f;
        while (t < fall)
        {
            float p = t / fall;
            transform.position = start + Vector3.up * Mathf.Lerp(d.launchHeight, 0, p);
            t += Time.deltaTime;
            yield return null;
        }
        transform.position = start;
        isLaunched = false;
    }
}
