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
    }

    public void Update()
    {
        timer -= Time.deltaTime;
        if (timer <= 0f)
        {
            player.stateMachine.ChangeState(new IdleState(player));
            player.OnAttackEnd();
        }
    }

    public void Exit() { }

    private void SpawnHitbox()
    {
        GameObject hitbox = new GameObject("AttackHitbox");
        hitbox.transform.SetParent(player.transform, false);

        float dir = player.isFacingRight ? 1f : -1f;
        Vector3 offset = new Vector3(
            attackData.hitboxOffset.x * dir,
            attackData.hitboxOffset.y,
            0f
        );
        hitbox.transform.position = player.hitboxOrigin.position + offset;

        var col = hitbox.AddComponent<BoxCollider2D>();
        col.size = attackData.hitboxSize;
        col.isTrigger = true;

        var phb = hitbox.AddComponent<PlayerHitbox>();
        phb.attackData = attackData;

        Object.Destroy(hitbox, attackData.attackDuration);
    }
}
