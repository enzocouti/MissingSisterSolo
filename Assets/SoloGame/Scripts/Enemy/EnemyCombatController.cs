using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class EnemyCombatController : MonoBehaviour
{
    [Header("AI & Combat")]
    public float moveSpeed = 2.4f;
    public float detectionRange = 7f;
    public float attackRange = 1.2f;
    public float attackCooldown = 1.4f;
    public int touchDamage = 2;
    [Range(1, 3)] public int maxAttackers = 1;

    [Header("Spacing")]
    public float minSeparation = 0.8f;
    public float verticalAlignThreshold = 0.34f;
    public float verticalSeparation = 0.46f;

    [Header("Get Up")]
    public float getUpDelay = 0.7f;

    protected Transform player;
    protected SpriteRenderer spriteRenderer;
    protected Rigidbody2D rb;
    protected static List<EnemyCombatController> attackers = new List<EnemyCombatController>();

    protected bool isAttacking = false;
    protected bool isDead = false;
    protected bool isLaunched = false;
    protected bool isKnockedDown = false;
    protected bool isGettingUp = false;

    protected Color baseColor; // Changed from private to protected for boss
    protected float lastAttackTime = -99f; //  ADDED

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        rb = GetComponent<Rigidbody2D>();
        baseColor = spriteRenderer ? spriteRenderer.color : Color.white;
    }

    private void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player")?.transform;

        Collider2D myCol = GetComponent<Collider2D>();
        foreach (var enemy in FindObjectsByType<EnemyCombatController>(FindObjectsSortMode.None))
        {
            if (enemy != this)
            {
                Collider2D otherCol = enemy.GetComponent<Collider2D>();
                if (myCol && otherCol)
                    Physics2D.IgnoreCollision(myCol, otherCol, true);
            }
        }
    }

    void Update()
    {
        if (isDead || isLaunched || isKnockedDown || isGettingUp || isAttacking) return;
        if (player == null) return;

        Vector2 toPlayer = player.position - transform.position;
        float dist = toPlayer.magnitude;
        float xDist = Mathf.Abs(toPlayer.x);
        float yDist = Mathf.Abs(toPlayer.y);

        if (dist <= attackRange)
        {
            if (!isAttacking && CanAttack())
            {
                TryAttack();
            }
            else if (!isAttacking)
            {
                StartCoroutine(PaceCoroutine());
            }
        }
        else if (dist <= detectionRange)
        {
            if (yDist < verticalAlignThreshold)
            {
                bool blocked = false;
                Collider2D[] nearby = Physics2D.OverlapCircleAll(transform.position, minSeparation);
                foreach (var col in nearby)
                {
                    if (col != null && col != GetComponent<Collider2D>() && col.CompareTag("Enemy"))
                    {
                        float horizontalDiff = col.transform.position.x - transform.position.x;
                        float verticalDiff = Mathf.Abs(col.transform.position.y - transform.position.y);
                        if (Mathf.Abs(horizontalDiff) < minSeparation && verticalDiff < verticalSeparation)
                        {
                            blocked = true;
                            break;
                        }
                    }
                }
                if (!blocked)
                {
                    float xDir = Mathf.Sign(toPlayer.x);
                    transform.position += new Vector3(xDir * moveSpeed * Time.deltaTime, 0, 0);
                    if (spriteRenderer)
                        spriteRenderer.flipX = (xDir < 0);
                }
            }
            else
            {
                float yDir = Mathf.Sign(toPlayer.y);
                transform.position += new Vector3(0, yDir * moveSpeed * 0.72f * Time.deltaTime, 0);
            }
        }
    }

    //  This needs to be here so boss can override and base can use it
    protected virtual void TryAttack()
    {
        attackers.RemoveAll(a => a == null);
        if (attackers.Count >= maxAttackers && !attackers.Contains(this)) return;
        attackers.Add(this);
        StartCoroutine(AttackCoroutine());
    }

    //  Added CanAttack as a protected function
    protected bool CanAttack()
    {
        attackers.RemoveAll(a => a == null);
        return attackers.Count < maxAttackers || attackers.Contains(this);
    }

    private IEnumerator AttackCoroutine()
    {
        isAttacking = true;
        spriteRenderer.color = Color.yellow;
        yield return new WaitForSeconds(0.16f);

        spriteRenderer.color = Color.red;
        if (player != null)
        {
            PlayerHealth hp = player.GetComponent<PlayerHealth>();
            if (hp != null)
                hp.TakeDamage(touchDamage);
        }
        yield return new WaitForSeconds(0.11f);

        spriteRenderer.color = baseColor;
        lastAttackTime = Time.time;
        yield return new WaitForSeconds(attackCooldown);
        isAttacking = false;
        attackers.Remove(this);
    }

    private IEnumerator PaceCoroutine()
    {
        float paceTime = Random.Range(0.32f, 0.8f);
        float timer = 0;
        int dir = Random.value < 0.5f ? -1 : 1;
        Vector3 start = transform.position;
        while (timer < paceTime && !isAttacking && !isDead)
        {
            float offset = dir * 0.32f * Mathf.Sin(timer * 4f);
            transform.position = new Vector3(start.x + offset, start.y, start.z);
            timer += Time.deltaTime;
            yield return null;
        }
    }

    public void OnHurt(PlayerAttackData data, bool playerIsFacingRight)
    {
        if (isDead || isKnockedDown || isLaunched || isGettingUp) return;

        if (data.isSlam)
        {
            StartCoroutine(SlamCoroutine());
        }
        else if (data.isLauncher || data.isHeavy)
        {
            StartCoroutine(LaunchArcCoroutine(data, playerIsFacingRight));
        }
        else
        {
            StartCoroutine(HurtFlash());
        }
    }

    public void OnDeath()
    {
        isDead = true;
        StopAllCoroutines();
        spriteRenderer.color = Color.gray;
        attackers.Remove(this);
    }

    private IEnumerator HurtFlash()
    {
        spriteRenderer.color = Color.red;
        yield return new WaitForSeconds(0.08f);
        spriteRenderer.color = baseColor;
    }

    private IEnumerator LaunchArcCoroutine(PlayerAttackData data, bool playerIsFacingRight)
    {
        isLaunched = true;
        float duration = data.launchDuration;
        float height = data.launchHeight;
        Vector3 start = transform.position;
        float dir = playerIsFacingRight ? 1f : -1f;
        Vector3 end = start + new Vector3(dir * 2.2f, 0f, 0f);

        float t = 0f;
        while (t < duration)
        {
            float progress = t / duration;
            float yOffset = Mathf.Sin(progress * Mathf.PI) * height;
            Vector3 horizontal = Vector3.Lerp(start, end, progress);
            transform.position = new Vector3(horizontal.x, start.y + yOffset, start.z);
            t += Time.deltaTime;
            yield return null;
        }
        transform.position = new Vector3(end.x, start.y, start.z);

        isLaunched = false;
        isKnockedDown = true;
        spriteRenderer.color = Color.magenta;
        yield return new WaitForSeconds(getUpDelay);
        spriteRenderer.color = baseColor;
        isKnockedDown = false;
        isGettingUp = false;
    }

    private IEnumerator SlamCoroutine()
    {
        isKnockedDown = true;
        isLaunched = true;
        Vector3 start = transform.position;
        Vector3 end = new Vector3(start.x, 0f, start.z);
        float fallTime = 0.27f;
        float t = 0f;
        while (t < fallTime)
        {
            transform.position = Vector3.Lerp(start, end, t / fallTime);
            t += Time.deltaTime;
            yield return null;
        }
        transform.position = end;
        yield return new WaitForSeconds(getUpDelay);
        spriteRenderer.color = baseColor;
        isKnockedDown = false;
        isLaunched = false;
        isGettingUp = false;
    }
}
