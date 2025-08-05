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
    public float baseHurtPause = 0.28f;

    [Header("Crowd Logic")]
    [Range(1, 5)] public int maxSimultaneousAttackers = 2;
    public float bufferDistance = 1.1f;
    public float waitShuffleSpeed = 0.13f;
    public float waitMinDistance = 1.8f;

    [Header("Death FX")]
    public float deathArcDuration = 0.5f;
    public float deathArcHeight = 1.2f;
    public float deathArcDistance = 2f;
    public float deathLayTime = 0.2f;
    public float deathFlashTime = 0.8f;
    public float deathFlashInterval = 0.13f;

    [Header("Animation")]
    public Animator animator;
    protected static readonly string T_ATTACK = "Attack";
    protected static readonly string T_HURT = "Hurt";
    protected static readonly string T_KNOCKBACK = "Knockback";
    protected static readonly string T_KNOCKDOWN = "Knockdown";
    protected static readonly string T_GETUP = "GetUp";

    
    public static List<EnemyCombatController> allEnemies = new List<EnemyCombatController>();
    protected static int currentAttackers = 0;

    protected Transform player;
    protected SpriteRenderer spriteRenderer;
    public Color baseColor;

    public bool isAttacking, isDead, isHurt, isLaunched;
    bool isWaiting;
    float shuffleTimer;
    Vector2 waitShuffleDir;

    void OnEnable() => allEnemies.Add(this);
    void OnDisable() { allEnemies.Remove(this); if (isAttacking) currentAttackers--; }

    void Awake()
    {
        animator = GetComponent<Animator>()
                   ?? GetComponentInChildren<Animator>();
        if (animator == null)
            Debug.LogError($"[Enemy] No Animator found on {name} or children!");

        spriteRenderer = GetComponent<SpriteRenderer>();
        baseColor = spriteRenderer ? spriteRenderer.color : Color.white;
    }

    void Start()
    {
        player = GameObject.FindWithTag("Player")?.transform;
    }

    void Update()
    {
        if (isDead || isHurt || isLaunched || player == null) return;

        
        spriteRenderer.flipX = player.position.x < transform.position.x;

        
        Vector2 me = transform.position;
        foreach (var other in allEnemies)
        {
            if (other != this && other != null && !other.isDead)
            {
                Vector2 them = other.transform.position;
                float d = Vector2.Distance(me, them);
                if (d < bufferDistance)
                    transform.position += (Vector3)((me - them).normalized * (bufferDistance - d) * 0.2f);
            }
        }

        
        float dist = Vector2.Distance(me, player.position);
        bool canAttack = (!isAttacking && !isWaiting && currentAttackers < maxSimultaneousAttackers)
                         || isAttacking;

        float speed = 0f;
        if (canAttack && dist > attackRange)
        {
            Vector2 dir = ((Vector2)player.position - me).normalized;
            transform.position += (Vector3)(dir * moveSpeed * Time.deltaTime);
            speed = moveSpeed;
        }
        if (animator) animator.SetFloat("Speed", speed);

        if (canAttack && dist <= attackRange && !isAttacking)
        {
            isAttacking = true;
            currentAttackers++;
            StartCoroutine(AttackCoroutine());
        }
        else if (!canAttack)
        {
            isWaiting = true;
            if (dist < waitMinDistance)
            {
                Vector2 away = (me - (Vector2)player.position).normalized;
                transform.position += (Vector3)(away * waitShuffleSpeed * Time.deltaTime);
            }
            else
            {
                shuffleTimer -= Time.deltaTime;
                if (shuffleTimer <= 0f)
                {
                    waitShuffleDir = Random.insideUnitCircle.normalized;
                    shuffleTimer = Random.Range(0.25f, 0.6f);
                }
                transform.position += (Vector3)(waitShuffleDir * waitShuffleSpeed * 0.5f * Time.deltaTime);
            }
        }
        else
        {
            isWaiting = false;
        }
    }

    protected virtual IEnumerator AttackCoroutine()
    {
        if (animator) animator.SetTrigger(T_ATTACK);

        
        if (SoundManager.Instance) SoundManager.Instance.PlayEnemyPunch();

        spriteRenderer.color = Color.yellow;
        yield return new WaitForSeconds(0.17f);

        player?.GetComponent<PlayerHealth>()?.TakeDamage(touchDamage);
        yield return new WaitForSeconds(0.10f);

        spriteRenderer.color = baseColor;
        yield return new WaitForSeconds(attackCooldown - 0.27f);

        isAttacking = false;
        currentAttackers = Mathf.Max(0, currentAttackers - 1);
    }

    public void OnHurt(PlayerAttackData data, bool ignored)
    {
        if (isDead) return;
        if (animator) animator.SetTrigger(T_HURT);

        
        if (SoundManager.Instance) SoundManager.Instance.PlayEnemyHurt();

        float pause = (data != null && data.hitPause > 0f)
                      ? data.hitPause
                      : baseHurtPause;

        StartCoroutine(HitFeedback(pause, data));
    }

    IEnumerator HitFeedback(float pause, PlayerAttackData data)
    {
        isHurt = true;
        spriteRenderer.color = Color.red;
        yield return new WaitForSecondsRealtime(pause);

        if (data.isLauncher)
            yield return StartCoroutine(Launch(data));
        else if (data.isHeavy)
            yield return StartCoroutine(Knockback(data));

        spriteRenderer.color = baseColor;
        isHurt = false;
    }

    IEnumerator Knockback(PlayerAttackData d)
    {
        isLaunched = true;
        if (animator) animator.SetTrigger(T_KNOCKBACK);

        Vector3 start = transform.position;
        float dir = spriteRenderer.flipX ? +1f : -1f;
        Vector3 end = start + Vector3.right * (dir * d.knockbackForce);

        float dur = 0.35f, h = 1f, t = 0f;
        while (t < dur)
        {
            float p = t / dur;
            float y = Mathf.Sin(p * Mathf.PI) * h;
            transform.position = Vector3.Lerp(start, end, p) + Vector3.up * y;
            t += Time.deltaTime;
            yield return null;
        }
        transform.position = end;

        if (animator) animator.SetTrigger(T_KNOCKDOWN);
        yield return new WaitForSeconds(0.2f);
        if (animator) animator.SetTrigger(T_GETUP);

       
        if (SoundManager.Instance) SoundManager.Instance.PlayEnemyGetUp();

        yield return new WaitForSeconds(animator.GetCurrentAnimatorStateInfo(0).length);

        isLaunched = false;
    }

    IEnumerator Launch(PlayerAttackData d)
    {
        isLaunched = true;
        if (animator) animator.SetTrigger(T_KNOCKBACK);

        Vector3 start = transform.position;
        float t = 0f, dur = d.launchDuration;
        while (t < dur)
        {
            float p = t / dur;
            float y = Mathf.Sin(p * Mathf.PI) * d.launchHeight;
            transform.position = start + Vector3.up * y;
            t += Time.deltaTime;
            yield return null;
        }

        yield return new WaitForSeconds(d.launchHangTime);

        float fallDur = dur * 0.8f; t = 0f;
        while (t < fallDur)
        {
            float p = t / fallDur;
            transform.position = start + Vector3.up * Mathf.Lerp(d.launchHeight, 0, p);
            t += Time.deltaTime;
            yield return null;
        }
        transform.position = start;

        if (animator) animator.SetTrigger(T_KNOCKDOWN);
        yield return new WaitForSeconds(0.2f);
        if (animator) animator.SetTrigger(T_GETUP);

        
        if (SoundManager.Instance) SoundManager.Instance.PlayEnemyGetUp();

        yield return new WaitForSeconds(animator.GetCurrentAnimatorStateInfo(0).length);

        isLaunched = false;
    }

    public virtual void OnDeath()
    {
        isDead = true;
        StopAllCoroutines();
        if (isAttacking) currentAttackers = Mathf.Max(0, currentAttackers - 1);

        
        if (SoundManager.Instance) SoundManager.Instance.PlayEnemyDeath();

        StartCoroutine(DeathArcSequence());
    }

    IEnumerator DeathArcSequence()
    {
        isLaunched = true;
        if (animator) animator.SetTrigger(T_KNOCKBACK);

        Vector3 start = transform.position;
        float dir = spriteRenderer.flipX ? +1f : -1f;
        Vector3 end = start + Vector3.right * (dir * deathArcDistance);

        float t = 0f;
        while (t < deathArcDuration)
        {
            float p = t / deathArcDuration;
            float y = Mathf.Sin(p * Mathf.PI) * deathArcHeight;
            float x = Mathf.Lerp(start.x, end.x, p);
            transform.position = new Vector3(x, start.y + y, start.z);
            t += Time.deltaTime;
            yield return null;
        }
        transform.position = new Vector3(end.x, start.y, start.z);

        if (animator) animator.SetTrigger(T_KNOCKDOWN);
        isLaunched = false;

        yield return new WaitForSeconds(deathLayTime);

        float f = 0f;
        bool visible = true;
        while (f < deathFlashTime)
        {
            visible = !visible;
            spriteRenderer.enabled = visible;
            yield return new WaitForSeconds(deathFlashInterval);
            f += deathFlashInterval;
        }

        Destroy(gameObject);
    }
}
