using UnityEngine;

public class AttackState : IPlayerState
{
    private PlayerCombat player;
    private PlayerAttackData attackData;
    private float timer;

    public AttackState(PlayerCombat combatRef, PlayerAttackData data)
    {
        player = combatRef;
        attackData = data;
    }

    public void Enter()
    {
        timer = attackData.attackDuration;
        SpawnHitbox();
        Debug.Log("Entered Attack: " + attackData.attackName);
    }

    public void Update()
    {
        timer -= Time.deltaTime;
        if (timer <= 0)
        {
            player.stateMachine.ChangeState(new IdleState(player));
            player.OnAttackEnd();
        }
    }

    public void Exit()
    {
        Debug.Log("Exited Attack: " + attackData.attackName);
    }

    private void SpawnHitbox()
    {
        GameObject hitbox = new GameObject("AttackHitbox");
        hitbox.transform.parent = player.transform;

        float xOffset = player.isFacingRight ? attackData.hitboxOffset.x : -attackData.hitboxOffset.x;
        Vector3 spawnPos = player.hitboxOrigin.position + new Vector3(xOffset, attackData.hitboxOffset.y, 0);
        hitbox.transform.position = spawnPos;

        BoxCollider2D col = hitbox.AddComponent<BoxCollider2D>();
        col.size = attackData.hitboxSize;
        col.isTrigger = true;

        var hb = hitbox.AddComponent<PlayerHitbox>();
        hb.attackData = attackData;
    }
}
