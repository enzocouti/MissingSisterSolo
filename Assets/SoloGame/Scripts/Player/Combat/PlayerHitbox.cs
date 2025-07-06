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

            // Apply knockback and feedback later
            
            var flash = other.GetComponent<EnemyFlashFeedback>();
            if (flash) flash.Flash();

            // Add force damage later
        }
    }
}
