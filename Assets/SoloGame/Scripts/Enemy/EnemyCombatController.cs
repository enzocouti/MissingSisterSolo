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

    [Header("Spacing")]
    public float minSeparation = 0.7f;
    public float verticalAlignThreshold = 0.32f;
    public float verticalSeparation = 0.5f;

    [Header("Behavior Tuning")]
    public int maxAttackers = 1;
    public float paceDistance = 0.7f;
    public float paceSpeed = 1.1f;

    protected Transform player;
    protected static List<EnemyCombatController> attackers = new List<EnemyCombatController>();

    protected bool isAttacking = false;
    protected bool isPacing = false;
    private bool isDead = false;
    private bool isHurt = false;
    protected bool isLaunched = false;
    protected float lastAttackTime = -99f;

    protected SpriteRenderer spriteRenderer;
    private Color baseColor;

    private Vector3 startPacePos;
    private int paceDir = 1;

    private void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player")?.transform;
        spriteRenderer = GetComponent<SpriteRenderer>();
        baseColor = spriteRenderer ? spriteRenderer.color : Color.white;

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
        if (isDead || isHurt || isLaunched) return;
        if (player == null) return;

        Vector2 toPlayer = player.position - transform.position;
        float xDist = Mathf.Abs(toPlayer.x);
        float yDist = Mathf.Abs(toPlayer.y);
        float attackDist = toPlayer.magnitude;

        if (attackDist <= attackRange)
        {
            if (!isAttacking && CanAttack())
            {
                TryAttack(); // <-- Use this for extensibility!
            }
            else if (!isAttacking)
            {
                if (!isPacing)
                    StartCoroutine(PaceCoroutine());
            }
        }
        else if (attackDist <= detectionRange)
        {
            if (yDist > verticalAlignThreshold)
            {
                float yDir = Mathf.Sign(toPlayer.y);
                transform.position += new Vector3(0, yDir * moveSpeed * Time.deltaTime, 0);
            }
            else
            {
                bool blocked = false;
                Collider2D[] nearby = Physics2D.OverlapCircleAll(transform.position, minSeparation);
                foreach (var col in nearby)
                {
                    if (col != null && col != GetComponent<Collider2D>() && col.CompareTag("Enemy"))
                    {
                        Vector2 otherPos = col.transform.position;
                        float horizontalDiff = otherPos.x - transform.position.x;
                        float verticalDiff = Mathf.Abs(otherPos.y - transform.position.y);

                        bool sameDirection = Mathf.Sign(horizontalDiff) == Mathf.Sign(toPlayer.x);
                        bool closeVertically = verticalDiff < verticalSeparation;

                        if (Mathf.Abs(horizontalDiff) < minSeparation && sameDirection && closeVertically)
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
        }
    }

    // This is what bosses will override!
    protected virtual void TryAttack()
    {
        attackers.Add(this);
        StartCoroutine(AttackCoroutine());
    }

    private bool CanAttack()
    {
        attackers.RemoveAll(a => a == null);
        return attackers.Count < maxAttackers || attackers.Contains(this);
    }

    private IEnumerator AttackCoroutine()
    {
        isAttacking = true;
        float windup = 0.18f;
        float hitPause = 0.09f;
        if (spriteRenderer) spriteRenderer.color = Color.yellow;
        yield return new WaitForSeconds(windup);

        if (spriteRenderer) spriteRenderer.color = Color.red;
        if (player != null)
        {
            PlayerHealth playerHealth = player.GetComponent<PlayerHealth>();
            if (playerHealth != null)
                playerHealth.TakeDamage(touchDamage);
        }
        yield return new WaitForSeconds(hitPause);

        if (spriteRenderer) spriteRenderer.color = baseColor;
        lastAttackTime = Time.time;
        yield return new WaitForSeconds(attackCooldown - windup - hitPause);

        isAttacking = false;
        attackers.Remove(this);
    }

    private IEnumerator PaceCoroutine()
    {
        isPacing = true;
        startPacePos = transform.position;
        float paceTime = Random.Range(0.3f, 0.75f);
        paceDir = Random.value < 0.5f ? -1 : 1;

        float timer = 0;
        while (timer < paceTime && !isAttacking)
        {
            float move = paceDir * paceSpeed * Time.deltaTime;
            transform.position = startPacePos + new Vector3(move * Mathf.Sin(timer * 2f), 0, 0);
            timer += Time.deltaTime;
            yield return null;
        }
        isPacing = false;
    }

    public void OnHurt(PlayerAttackData attackData, bool playerIsFacingRight)
    {
        if (isDead) return;
        if (attackData.isLauncher)
        {
            StartCoroutine(LaunchCoroutine(attackData, playerIsFacingRight));
        }
        else if (attackData.isHeavy)
        {
            StartCoroutine(KnockbackCoroutine(attackData, playerIsFacingRight));
        }
        else
        {
            StartCoroutine(HurtFeedback());
        }
    }

    public void OnDeath()
    {
        isDead = true;
        StopAllCoroutines();
        if (spriteRenderer)
            spriteRenderer.color = Color.gray;
        attackers.Remove(this);
    }

    IEnumerator HurtFeedback()
    {
        isHurt = true;
        if (spriteRenderer)
            spriteRenderer.color = Color.red;

        float freeze = 0.07f;
        float t = 0;
        while (t < freeze)
        {
            t += Time.unscaledDeltaTime;
            yield return null;
        }

        Vector3 originalPos = transform.position;
        float shakeAmt = 0.07f;
        for (int i = 0; i < 3; i++)
        {
            transform.position = originalPos + (Vector3)Random.insideUnitCircle * shakeAmt;
            yield return new WaitForSeconds(0.03f);
        }
        transform.position = originalPos;

        if (spriteRenderer)
            spriteRenderer.color = baseColor;

        isHurt = false;
    }

    IEnumerator KnockbackCoroutine(PlayerAttackData attackData, bool playerIsFacingRight)
    {
        isLaunched = true;
        float duration = 0.35f;
        float height = 1f;
        float distance = attackData.knockbackForce;
        Vector3 start = transform.position;
        float direction = playerIsFacingRight ? 1f : -1f;
        Vector3 target = start + new Vector3(direction * distance, 0, 0);

        float timer = 0f;
        while (timer < duration)
        {
            float progress = timer / duration;
            float yOffset = Mathf.Sin(progress * Mathf.PI) * height;
            Vector3 horizontal = Vector3.Lerp(start, target, progress);
            transform.position = new Vector3(horizontal.x, start.y + yOffset, start.z);

            timer += Time.deltaTime;
            yield return null;
        }
        transform.position = new Vector3(target.x, start.y, start.z);

        isLaunched = false;
    }

    IEnumerator LaunchCoroutine(PlayerAttackData attackData, bool playerIsFacingRight)
    {
        isLaunched = true;
        float duration = attackData.launchDuration;
        float height = attackData.launchHeight;
        Vector3 start = transform.position;

        float timer = 0f;
        while (timer < duration)
        {
            float progress = timer / duration;
            float yOffset = Mathf.Sin(progress * Mathf.PI) * height;
            transform.position = new Vector3(start.x, start.y + yOffset, start.z);

            timer += Time.deltaTime;
            yield return null;
        }
        transform.position = new Vector3(start.x, start.y + height, start.z);

        float hang = attackData.launchHangTime;
        float hangTimer = 0f;
        while (hangTimer < hang)
        {
            hangTimer += Time.deltaTime;
            yield return null;
        }

        float fallTime = duration * 0.8f;
        Vector3 peak = transform.position;
        float fallTimer = 0f;
        while (fallTimer < fallTime)
        {
            float progress = fallTimer / fallTime;
            float yOffset = Mathf.Lerp(height, 0, progress);
            transform.position = new Vector3(start.x, start.y + yOffset, start.z);

            fallTimer += Time.deltaTime;
            yield return null;
        }
        transform.position = start;

        isLaunched = false;
    }
}
