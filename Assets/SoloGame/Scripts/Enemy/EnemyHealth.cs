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

    // Called by player hitbox pass in PlayerAttackData for flexible feedback
    public void TakeDamage(PlayerAttackData attackData, bool hitFromRight)
    {
        if (currentHealth <= 0) return;

        currentHealth -= (int)attackData.damage;

        Debug.Log($"[EnemyHealth] {gameObject.name} took {attackData.damage} ({currentHealth}/{maxHealth}) from {attackData.attackName}");

        // Let controller handle hurt feedback & logic
        if (controller != null)
            controller.OnHurt(attackData, hitFromRight);

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    public void Die()
    {
        Debug.Log($"[EnemyHealth] {gameObject.name} died!");

        if (controller != null)
            controller.OnDeath();

        // Notify CombatZoneManager that this enemy is gone
        if (CombatZoneManager.Instance != null)
            CombatZoneManager.Instance.NotifyEnemyKilled(gameObject);

        Destroy(gameObject, 0.1f); // Give time for feedback
    }
}
