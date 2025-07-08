using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class EnemyHealth : MonoBehaviour
{
    public float maxHealth = 30f;
    private float currentHealth;

    private Rigidbody2D rb;
    private SpriteRenderer spriteRenderer;
    private EnemyFlashFeedback flashFeedback;

    private bool isDead = false;

    private void Awake()
    {
        currentHealth = maxHealth;
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        flashFeedback = GetComponent<EnemyFlashFeedback>();
    }

    public void TakeHit(PlayerAttackData attackData, bool fromRight)
    {
        if (isDead) return;

        currentHealth -= attackData.damage;

        if (flashFeedback) flashFeedback.Flash();

        ApplyKnockback(attackData, fromRight);

        if (currentHealth <= 0)
        {
            Die();
        }
        else
        {
            //play hurt animation or pause briefly
        }
    }

    private void ApplyKnockback(PlayerAttackData attackData, bool fromRight)
    {
        Vector2 force = attackData.knockbackForce;
        if (!fromRight) force.x *= -1;

        rb.linearVelocity = Vector2.zero; // Reset velocity
        rb.AddForce(force, ForceMode2D.Impulse);
    }

    private void Die()
    {
        isDead = true;
        Debug.Log($"{name} has died.");
        // play death anim
        Destroy(gameObject, 0.2f); // Remove enemy
    }
}
