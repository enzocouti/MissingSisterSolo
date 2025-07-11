using UnityEngine;

public class PlayerHitbox : MonoBehaviour
{
    public PlayerAttackData attackData;

    private void Start()
    {
        Destroy(gameObject, attackData.attackDuration);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Enemy"))
        {
            Debug.Log($"{attackData.attackName} hit {other.name}");

            var enemyHealth = other.GetComponent<EnemyHealth>();
            var playerCombat = GetComponentInParent<PlayerCombat>();
            bool playerIsFacingRight = playerCombat ? playerCombat.isFacingRight : true; // fallback true

            if (enemyHealth != null)
            {
                enemyHealth.TakeDamage(attackData, playerIsFacingRight);
            }
        }
    }
}
