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

            //var flash = other.GetComponent<EnemyFlashFeedback>();
            //flash?.Flash();

            //var enemy = other.GetComponent<EnemyCombatController>();
            //if (enemy != null)
            {
                //var facing = GetComponentInParent<PlayerCombat>()?.isFacingRight ?? true;
                //enemy.TakeHit(attackData, facing);
            }
        }
    }
}
