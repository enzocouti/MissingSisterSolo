using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(BossCombatController))]
public class BossHealth : MonoBehaviour
{
    [Header("Health Settings")]
    public int maxHealth = 30;
    private int currentHealth;

    [Header("UI")]
    public Scrollbar healthBar; // Assign at runtime or from inspector

    private BossCombatController combatCtrl;
    private bool isDead = false;

    void Awake()
    {
        currentHealth = maxHealth;
        combatCtrl = GetComponent<BossCombatController>();
        UpdateUI();
    }

    public void SetHealthBar(Scrollbar bar)
    {
        healthBar = bar;
        UpdateUI();
    }

    public void TakeDamage(int dmg)
    {
        if (isDead || currentHealth <= 0) return;

        currentHealth = Mathf.Max(0, currentHealth - dmg);
        UpdateUI();

    
        if (combatCtrl && combatCtrl.animator) combatCtrl.animator.SetTrigger("Hurt");

        if (currentHealth == 0)
        {
            isDead = true;
            if (healthBar != null) healthBar.size = 0f;
            if (healthBar != null) healthBar.gameObject.SetActive(false);

            combatCtrl.Die();
        }
    }

    private void UpdateUI()
    {
        if (healthBar != null)
        {
            float percent = (float)currentHealth / maxHealth;
            healthBar.size = (currentHealth <= 0) ? 0f : Mathf.Clamp01(percent);
        }
    }
}
