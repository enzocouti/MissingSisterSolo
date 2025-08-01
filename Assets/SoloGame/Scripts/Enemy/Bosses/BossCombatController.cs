using UnityEngine;
using System.Collections;

public class BossCombatController : EnemyCombatController
{
    public enum BossAttackType { Slam, Dash }
    public BossAttackType bossAttackType = BossAttackType.Slam;

    [Header("Slam Attack")]
    public float slamRange = 1.8f;
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

    protected override IEnumerator AttackCoroutine()
    {
        // If boss should use a special attack
        bool usedSpecial = false;

        if (bossAttackType == BossAttackType.Slam &&
            Time.time - lastSlamTime >= slamCooldown && Random.value < slamChance)
        {
            yield return StartCoroutine(SlamAttack());
            lastSlamTime = Time.time;
            usedSpecial = true;
        }
        else if (bossAttackType == BossAttackType.Dash &&
                 Time.time - lastDashTime >= dashCooldown)
        {
            yield return StartCoroutine(DashAttack());
            lastDashTime = Time.time;
            usedSpecial = true;
        }

        if (!usedSpecial)
        {
            // Fallback to normal attack
            yield return base.AttackCoroutine();
        }
    }

    IEnumerator SlamAttack()
    {
        isAttacking = true;
        Debug.Log("[Boss] SLAM!");
        if (spriteRenderer) spriteRenderer.color = Color.yellow;
        yield return new WaitForSeconds(0.18f);

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

        if (spriteRenderer) spriteRenderer.color = baseColor;
        yield return new WaitForSeconds(slamCooldown);
        isAttacking = false;
    }

    IEnumerator DashAttack()
    {
        isAttacking = true;
        Debug.Log("[Boss] DASH!");
        if (spriteRenderer) spriteRenderer.color = Color.yellow;

        Vector3 start = transform.position;
        Vector3 dir = player ? (player.position - transform.position).normalized : Vector3.right;
        dir.y = 0;
        float dashDuration = dashLength / dashSpeed;
        float t = 0f;

        isLaunched = true;

        while (t < dashDuration)
        {
            transform.position += dir * dashSpeed * Time.deltaTime;
            t += Time.deltaTime;
            yield return null;
        }

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
        yield return new WaitForSeconds(dashCooldown);

        if (spriteRenderer) spriteRenderer.color = baseColor;
        isAttacking = false;
    }
}
