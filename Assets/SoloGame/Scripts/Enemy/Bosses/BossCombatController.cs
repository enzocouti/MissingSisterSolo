using UnityEngine;
using System.Collections;

// Boss inherits everything from normal enemy
public class BossCombatController : EnemyCombatController
{
    public enum BossAttackType { Slam, Dash }
    public BossAttackType bossAttackType = BossAttackType.Slam;

    [Header("Slam Attack")]
    public float slamRange = 1.8f;   // Area
    public int slamDamage = 5;
    public float slamCooldown = 3f;
    [Range(0f, 1f)] public float slamChance = 0.45f;

    [Header("Dash Attack")]
    public float dashSpeed = 5f;
    public float dashLength = 4f;
    public int dashDamage = 6;
    public float dashCooldown = 3.5f;

    private float lastSlamTime = -99f;
    private float lastDashTime = -99f;

    // Only this method is different!
    protected override void TryAttack()
    {
        // Slam
        if (bossAttackType == BossAttackType.Slam)
        {
            if (Time.time - lastSlamTime >= slamCooldown && Random.value < slamChance)
            {
                StartCoroutine(SlamAttack());
                lastSlamTime = Time.time;
                return;
            }
        }
        // Dash
        else if (bossAttackType == BossAttackType.Dash)
        {
            if (Time.time - lastDashTime >= dashCooldown)
            {
                StartCoroutine(DashAttack());
                lastDashTime = Time.time;
                return;
            }
        }

        // Fallback to regular attack
        base.TryAttack();
    }

    IEnumerator SlamAttack()
    {
        Debug.Log("[Boss] SLAM!");
        if (spriteRenderer) spriteRenderer.color = Color.yellow; // Yellow for attack windup
        yield return new WaitForSeconds(0.18f);

        // Hit player if in slam range
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, slamRange);
        foreach (var hit in hits)
        {
            if (hit.CompareTag("Player"))
            {
                PlayerHealth playerHealth = hit.GetComponent<PlayerHealth>();
                if (playerHealth != null)
                    playerHealth.TakeDamage(slamDamage);
            }
        }
        yield return new WaitForSeconds(0.12f);

        if (spriteRenderer) spriteRenderer.color = baseColor; // Restore color after attack
    }

    IEnumerator DashAttack()
    {
        Debug.Log("[Boss] DASH!");
        if (spriteRenderer) spriteRenderer.color = Color.yellow; // Yellow for dash windup

        Vector3 start = transform.position;
        Vector3 dir = player ? (player.position - transform.position).normalized : Vector3.right;
        dir.y = 0; // X axis only
        float dashDuration = dashLength / dashSpeed;
        float t = 0f;

        isLaunched = true; // Prevent moving/attacking during dash

        while (t < dashDuration)
        {
            transform.position += dir * dashSpeed * Time.deltaTime;
            t += Time.deltaTime;
            yield return null;
        }

        // Hit if player is close after dash
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, 1.2f);
        foreach (var hit in hits)
        {
            if (hit.CompareTag("Player"))
            {
                PlayerHealth playerHealth = hit.GetComponent<PlayerHealth>();
                if (playerHealth != null)
                    playerHealth.TakeDamage(dashDamage);
            }
        }

        isLaunched = false;

        if (spriteRenderer) spriteRenderer.color = baseColor;
    }
}
