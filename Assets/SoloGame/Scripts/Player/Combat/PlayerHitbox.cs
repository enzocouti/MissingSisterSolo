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
            if (enemyHealth != null)
            {
                bool hitFromRight = (transform.position.x < other.transform.position.x);
                enemyHealth.TakeDamage(attackData, hitFromRight);
            }
        }
    }
}
