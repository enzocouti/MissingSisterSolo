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
        Debug.Log("Attack Started: " + attackData.attackName);
    }

    public void Update()
    {
        timer -= Time.deltaTime;
        if (timer <= 0)
        {
            player.stateMachine.ChangeState(new IdleState(player));
        }
    }

    public void Exit() { }

    private void SpawnHitbox()
    {
        GameObject hitbox = new GameObject("AttackHitbox");
        hitbox.transform.parent = player.transform;

        Vector2 flippedOffset = attackData.hitboxOffset;
        flippedOffset.x *= player.isFacingRight ? 1 : -1;

        hitbox.transform.position = player.hitboxOrigin.position + (Vector3)flippedOffset;

        BoxCollider2D col = hitbox.AddComponent<BoxCollider2D>();
        col.size = attackData.hitboxSize;
        col.isTrigger = true;

        var hb = hitbox.AddComponent<PlayerHitbox>();
        hb.attackData = attackData;

        // flip visuals or debug gizmos
    }
}
