using UnityEngine;

public class EnemyHealth : MonoBehaviour
{
    public int maxHealth = 12;
    private int currentHealth;
    private EnemyCombatController controller;

    private void Awake()
    {
        currentHealth = maxHealth;
        controller = GetComponent<EnemyCombatController>();
    }

    public void TakeDamage(PlayerAttackData data, bool playerIsFacingRight)
    {
        if (currentHealth <= 0) return;

        currentHealth -= data.damage;
        if (controller != null)
            controller.OnHurt(data, playerIsFacingRight);

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    public void Die()
    {
        if (controller != null)
            controller.OnDeath();
        if (CombatZoneManager.Instance != null)
            CombatZoneManager.Instance.NotifyEnemyKilled(gameObject);
        Destroy(gameObject, 0.12f);
    }
}
