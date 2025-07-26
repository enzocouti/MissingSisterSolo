using UnityEngine;

public class AttackState : IPlayerState
{
    private PlayerCombat player;
    private PlayerAttackData attack;
    private float timer;

    public AttackState(PlayerCombat player, PlayerAttackData attack)
    {
        this.player = player;
        this.attack = attack;
    }

    public void Enter()
    {
        player.isBusy = true;
        player.rb.linearVelocity = Vector2.zero;
        timer = attack.attackDuration;

        if (!string.IsNullOrEmpty(attack.animationTriggerName))
            player.GetComponent<Animator>()?.SetTrigger(attack.animationTriggerName);

        SpawnHitbox();
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
        GameObject obj = new GameObject("Hitbox");
        obj.transform.SetParent(player.transform);

        float xOffset = player.isFacingRight ? attack.hitboxOffset.x : -attack.hitboxOffset.x;
        Vector3 spawnPos = player.hitboxOrigin.position + new Vector3(xOffset, attack.hitboxOffset.y, 0f);
        obj.transform.position = spawnPos;

        BoxCollider2D box = obj.AddComponent<BoxCollider2D>();
        box.size = attack.hitboxSize;
        box.isTrigger = true;

        PlayerHitbox hitbox = obj.AddComponent<PlayerHitbox>();
        hitbox.attackData = attack;

        GameObject.Destroy(obj, attack.attackDuration);
    }
}
