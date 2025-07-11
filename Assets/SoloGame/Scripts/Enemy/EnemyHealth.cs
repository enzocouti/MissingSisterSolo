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

    // playerIsFacingRight means true = knock right, false = knock left
    public void TakeDamage(PlayerAttackData attackData, bool playerIsFacingRight)
    {
        if (currentHealth <= 0) return;

        currentHealth -= (int)attackData.damage;

        Debug.Log($"[EnemyHealth] {gameObject.name} took {attackData.damage} ({currentHealth}/{maxHealth}) from {attackData.attackName}");

        // Pass correct direction to the controller
        if (controller != null)
            controller.OnHurt(attackData, playerIsFacingRight);

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
