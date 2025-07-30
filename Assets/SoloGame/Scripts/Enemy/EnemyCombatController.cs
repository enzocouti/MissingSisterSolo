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

    [Header("Attack Logic")]
    [Range(0, 180)] public float attackAngle = 60f;
    public float bufferZone = 2.3f;
    public float waitPaceDistance = 0.8f;
    public float waitPaceSpeed = 1.1f;

    public static List<EnemyCombatController> allEnemies = new List<EnemyCombatController>();

    protected Transform player;
    protected bool isAttacking = false;
    protected bool isDead = false;
    protected bool isHurt = false;
    protected bool isLaunched = false;

    protected SpriteRenderer spriteRenderer;
    public Color baseColor;

    private float waitPaceTimer = 0f;
    private int waitPaceDir = 1;

    protected virtual void OnEnable() { allEnemies.Add(this); }
    protected virtual void OnDisable() { allEnemies.Remove(this); }

    protected virtual void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player")?.transform;
        spriteRenderer = GetComponent<SpriteRenderer>();
        baseColor = spriteRenderer ? spriteRenderer.color : Color.white;
    }

    protected virtual void Update()
    {
        if (isDead || isHurt || isLaunched) return;
        if (player == null)
        {
            player = GameObject.FindGameObjectWithTag("Player")?.transform;
            if (player == null) return;
        }

        Vector2 toPlayer = player.position - transform.position;
        float distance = toPlayer.magnitude;

        // Always face the player
        if (toPlayer.x > 0.01f) spriteRenderer.flipX = false;
        else if (toPlayer.x < -0.01f) spriteRenderer.flipX = true;

        // Only the closest enemy can attack
        EnemyCombatController closest = FindClosestEnemyToPlayer();

        if (this == closest)
        {
            // Within attack range and in front? Attack!
            if (distance <= attackRange && PlayerIsInFront(toPlayer))
            {
                if (!isAttacking)
                    TryAttack();
            }
            else
            {
                // Move toward player if not in attack range yet
                MoveTowards(player.position, moveSpeed);
            }
        }
        else
        {
            // Maintain a buffer zone around the player and "pace" left/right
            float desiredDist = attackRange + bufferZone;
            float actualDist = toPlayer.magnitude;

            if (actualDist > desiredDist + 0.1f)
            {
                MoveTowards(player.position, moveSpeed);
                waitPaceTimer = 0f;
            }
            else if (actualDist < desiredDist - 0.1f)
            {
                Vector3 fromPlayer = (transform.position - player.position).normalized;
                MoveTowards(transform.position + fromPlayer, moveSpeed * 0.7f);
                waitPaceTimer = 0f;
            }
            else
            {
                // Pace left/right on the buffer ring (circle around player)
                waitPaceTimer += Time.deltaTime * waitPaceSpeed * waitPaceDir;
                if (Mathf.Abs(waitPaceTimer) > waitPaceDistance)
                {
                    waitPaceDir *= -1;
                }

                // Calculate a perpendicular vector to the player for pacing
                Vector3 bufferPos = player.position + (transform.position - player.position).normalized * desiredDist;
                Vector3 perp = Vector3.Cross(Vector3.forward, (bufferPos - player.position).normalized);
                Vector3 pacePos = bufferPos + perp * waitPaceTimer;

                MoveTowards(pacePos, moveSpeed * 0.65f);
            }
        }
    }

    protected void MoveTowards(Vector3 target, float speed)
    {
        Vector3 dir = (target - transform.position).normalized;
        transform.position += dir * speed * Time.deltaTime;
    }

    protected bool PlayerIsInFront(Vector2 toPlayer)
    {
        Vector2 facing = spriteRenderer.flipX ? Vector2.left : Vector2.right;
        float angle = Vector2.Angle(facing, toPlayer);
        return angle < attackAngle * 0.5f;
    }

    public static EnemyCombatController FindClosestEnemyToPlayer()
    {
        Transform player = GameObject.FindGameObjectWithTag("Player")?.transform;
        if (!player) return null;
        EnemyCombatController closest = null;
        float minDist = float.MaxValue;
        foreach (var enemy in allEnemies)
        {
            if (enemy == null || enemy.isDead) continue;
            float dist = (enemy.transform.position - player.position).sqrMagnitude;
            if (dist < minDist)
            {
                minDist = dist;
                closest = enemy;
            }
        }
        return closest;
    }

    protected virtual void TryAttack()
    {
        isAttacking = true;
        StartCoroutine(AttackCoroutine());
    }

    private IEnumerator AttackCoroutine()
    {
        float windup = 0.18f;
        float hitPause = 0.09f;
        if (spriteRenderer) spriteRenderer.color = Color.yellow;
        yield return new WaitForSeconds(windup);

        if (player != null)
        {
            PlayerHealth playerHealth = player.GetComponent<PlayerHealth>();
            if (playerHealth != null)
                playerHealth.TakeDamage(touchDamage);
        }
        yield return new WaitForSeconds(hitPause);

        if (spriteRenderer) spriteRenderer.color = baseColor;
        yield return new WaitForSeconds(attackCooldown - windup - hitPause);

        isAttacking = false;
    }

    public virtual void OnHurt(PlayerAttackData attackData, bool playerIsFacingRight)
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

    public virtual void OnDeath()
    {
        isDead = true;
        StopAllCoroutines();
        if (spriteRenderer)
            spriteRenderer.color = Color.gray;
    }

    protected IEnumerator HurtFeedback()
    {
        isHurt = true;
        if (spriteRenderer) spriteRenderer.color = Color.red;

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

    protected IEnumerator KnockbackCoroutine(PlayerAttackData attackData, bool playerIsFacingRight)
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

    protected IEnumerator LaunchCoroutine(PlayerAttackData attackData, bool playerIsFacingRight)
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
