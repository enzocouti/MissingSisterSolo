using UnityEngine;

public class PlayerHitbox : MonoBehaviour
{
    public PlayerAttackData attackData;
    private bool alreadyHit = false;

    private void Start()
    {
        Destroy(gameObject, attackData.attackDuration);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (alreadyHit || !other.CompareTag("Enemy")) return;

        var playerCombat = GetComponentInParent<PlayerCombat>();
        bool facingRight = playerCombat ? playerCombat.isFacingRight : true;

        var enemy = other.GetComponent<EnemyHealth>();
        var boss = other.GetComponent<BossHealth>();

        if (enemy != null)
            enemy.TakeDamage(attackData, facingRight);
        else if (boss != null)
            boss.TakeDamage(attackData, facingRight);

        alreadyHit = true;

        if (attackData.isHeavy || attackData.isLauncher)
            CameraShaker.Shake(0.3f, 0.1f);

        
        Time.timeScale = 0f;
        Time.fixedDeltaTime = 0.02f * Time.timeScale;
        Invoke(nameof(ResumeTime), attackData.hitPause);
    }

    private void ResumeTime()
    {
        Time.timeScale = 1f;
        Time.fixedDeltaTime = 0.02f;
    }

    public void Setup(PlayerAttackData data, bool flip)
    {
        attackData = data;

        
        if (flip)
            transform.localPosition = new Vector3(-data.hitboxOffset.x, data.hitboxOffset.y, 0f);
        else
            transform.localPosition = new Vector3(data.hitboxOffset.x, data.hitboxOffset.y, 0f);

        var box = GetComponent<BoxCollider2D>();
        if (box != null)
            box.size = data.hitboxSize;
    }
}
