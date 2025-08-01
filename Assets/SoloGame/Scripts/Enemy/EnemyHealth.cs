using UnityEngine;

public class EnemyHealth : MonoBehaviour
{
    public int maxHealth = 12;
    int currentHealth;
    EnemyCombatController controller;

    void Awake()
    {
        currentHealth = maxHealth;
        controller = GetComponent<EnemyCombatController>();
    }

    public void TakeDamage(PlayerAttackData data, bool facingRight)
    {
        if (currentHealth <= 0) return;
        currentHealth -= (int)data.damage;
        Debug.Log($"Hit {gameObject.name}: {data.damage} dmg, left {currentHealth}/{maxHealth}");
        controller?.OnHurt(data, facingRight);

        if (currentHealth <= 0)
            Die();
    }

    void Die()
    {
        Debug.Log($"{gameObject.name} died");
        controller?.OnDeath();
        CombatZoneManager.Instance?.NotifyEnemyKilled(gameObject);
    }
}
