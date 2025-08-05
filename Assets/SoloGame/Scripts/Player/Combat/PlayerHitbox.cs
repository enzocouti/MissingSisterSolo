using UnityEngine;

public class PlayerHitbox : MonoBehaviour
{
    [HideInInspector] public PlayerAttackData attackData;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Enemy")) return;

        Debug.Log($"{attackData.attackName} hit {other.name}");

        
        var bossHealth = other.GetComponent<BossHealth>();
        if (bossHealth != null)
        {
            bossHealth.TakeDamage(attackData.damage);
            return;
        }

       
        var enemyHealth = other.GetComponent<EnemyHealth>();
        if (enemyHealth != null)
        {
            
            var playerCombat = GetComponentInParent<PlayerCombat>();
            bool facingRight = playerCombat != null && playerCombat.isFacingRight;
            enemyHealth.TakeDamage(attackData, facingRight);
        }
    }
}
