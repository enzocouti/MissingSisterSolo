using UnityEngine;

public class PlayerHitbox : MonoBehaviour
{
    [HideInInspector] public PlayerAttackData attackData;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Enemy")) return;

        
        var playerCombat = GetComponentInParent<PlayerCombat>();
        bool facingRight = playerCombat?.isFacingRight ?? true;

        Debug.Log($"{attackData.attackName} hit {other.name}");

        var boss = other.GetComponent<BossHealth>();
        if (boss != null)
        {
            boss.TakeDamage(attackData, facingRight);
            return;
        }

        var enemy = other.GetComponent<EnemyHealth>();
        if (enemy != null)
        {
            enemy.TakeDamage(attackData, facingRight);
        }
    }
}
