using UnityEngine;

public class PlayerHitbox : MonoBehaviour
{
    public PlayerAttackData attackData;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Enemy")) return;

        var enemyHealth = other.GetComponent<EnemyHealth>();
        var bossHealth = other.GetComponent<BossHealth>();

        var playerCombat = GetComponentInParent<PlayerCombat>();
        bool facingRight = playerCombat ? playerCombat.isFacingRight : true;

        if (enemyHealth)
            enemyHealth.TakeDamage(attackData, facingRight);
        else if (bossHealth)
            bossHealth.TakeDamage(attackData, facingRight);
    }
}
