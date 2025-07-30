using UnityEngine;
using UnityEngine.UI;

public class BossHealth : MonoBehaviour
{
    public int maxHealth = 30;
    private int currentHealth;

    private EnemyCombatController controller;
    private Scrollbar healthBar;

    private void Awake()
    {
        currentHealth = maxHealth;
        controller = GetComponent<EnemyCombatController>();
        UpdateUI();
    }

    // Called by CombatZoneManager after spawn
    public void SetHealthBar(Scrollbar bar)
    {
        healthBar = bar;
        UpdateUI();
    }

    public void TakeDamage(PlayerAttackData attackData, bool playerIsFacingRight)
    {
        if (currentHealth <= 0) return;

        currentHealth -= (int)attackData.damage;
        if (currentHealth < 0) currentHealth = 0;
        UpdateUI();

        if (controller != null)
            controller.OnHurt(attackData, playerIsFacingRight);

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    private void UpdateUI()
    {
        if (healthBar)
            healthBar.size = (float)currentHealth / maxHealth;
    }

    public float GetHealthPercent()
    {
        return (float)currentHealth / maxHealth;
    }

    private void Die()
    {
        if (controller != null)
            controller.OnDeath();

        if (CombatZoneManager.Instance != null)
            CombatZoneManager.Instance.NotifyEnemyKilled(gameObject);

        Destroy(gameObject, 0.2f);
    }
}
