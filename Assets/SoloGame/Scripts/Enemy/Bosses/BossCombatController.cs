using UnityEngine;
using System.Collections;

public class BossCombatController : EnemyCombatController
{
    public enum BossSpecialType { None, AreaAttack, Dash }
    public BossSpecialType specialType = BossSpecialType.AreaAttack;

    [Header("Special: Area Slam")]
    public float slamRange = 1.8f;
    public int slamDamage = 5;
    public float slamCooldown = 3f;
    [Range(0f, 1f)] public float slamChance = 0.45f;

    [Header("Special: Dash")]
    public float dashSpeed = 7f;
    public float dashLength = 5f;
    public int dashDamage = 8;
    public float dashCooldown = 3.5f;

    private float lastSpecialTime = -99f;

    protected override void TryAttack()
    {
        // Area Slam
        if (specialType == BossSpecialType.AreaAttack)
        {
            if (Time.time - lastSpecialTime >= slamCooldown && Random.value < slamChance)
            {
                StartCoroutine(SlamAttack());
                lastSpecialTime = Time.time;
                return;
            }
        }
        // Dash
        else if (specialType == BossSpecialType.Dash)
        {
            if (Time.time - lastSpecialTime >= dashCooldown)
            {
                StartCoroutine(DashAttack());
                lastSpecialTime = Time.time;
                return;
            }
        }

        // Fallback to base attack
        base.TryAttack();
    }

    private IEnumerator SlamAttack()
    {
        spriteRenderer.color = Color.red;
        yield return new WaitForSeconds(0.22f);

        // Damage player if within radius
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, slamRange);
        foreach (var hit in hits)
        {
            if (hit.CompareTag("Player"))
            {
                PlayerHealth hp = hit.GetComponent<PlayerHealth>();
                if (hp != null)
                    hp.TakeDamage(slamDamage);
            }
        }
        yield return new WaitForSeconds(0.15f);
        spriteRenderer.color = baseColor;
    }

    private IEnumerator DashAttack()
    {
        spriteRenderer.color = Color.cyan;
        yield return new WaitForSeconds(0.13f);

        Vector3 start = transform.position;
        Vector3 dashDir = (player ? (player.position - start) : Vector3.right).normalized;
        dashDir.y = 0; // X axis only for sidescroller!
        float dashTime = dashLength / dashSpeed;
        float t = 0f;

        isLaunched = true; // Prevents movement/attack during dash

        while (t < dashTime)
        {
            transform.position += dashDir * dashSpeed * Time.deltaTime;
            t += Time.deltaTime;
            yield return null;
        }

        // Hit player if nearby at end of dash
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, 1.2f);
        foreach (var hit in hits)
        {
            if (hit.CompareTag("Player"))
            {
                PlayerHealth hp = hit.GetComponent<PlayerHealth>();
                if (hp != null)
                    hp.TakeDamage(dashDamage);
            }
        }

        isLaunched = false;
        spriteRenderer.color = baseColor;
    }
}
