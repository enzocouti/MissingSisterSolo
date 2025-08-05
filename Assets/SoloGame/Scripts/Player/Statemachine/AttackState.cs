using UnityEngine;

public class AttackState : IPlayerState
{
    private PlayerCombat player;
    private PlayerAttackData attackData;
    private float timer;

    
    private RuntimeAnimatorController originalController;

    public AttackState(PlayerCombat combatRef, PlayerAttackData data)
    {
        player = combatRef;
        attackData = data;
    }

    public void Enter()
    {

        originalController = player.animator.runtimeAnimatorController;
        var overrideController = new AnimatorOverrideController(originalController);
        overrideController["FirstPunch_Clip"] = attackData.animationClip;
        player.animator.runtimeAnimatorController = overrideController;

        player.animator.SetTrigger("Attack");

        
        if (SoundManager.Instance != null)
            SoundManager.Instance.PlayRandomPunch();

        SpawnHitbox();
        timer = attackData.attackDuration;
    }

    public void Update()
    {
        
        timer -= Time.deltaTime;
        if (timer <= 0f)
        {
            
            player.animator.runtimeAnimatorController = originalController;

            
            player.stateMachine.ChangeState(new IdleState(player));
            player.OnAttackEnd();
        }
    }

    public void Exit()
    {
        
    }

    private void SpawnHitbox()
    {
        GameObject hb = new GameObject("AttackHitbox");
        hb.transform.SetParent(player.transform, false);

        float dir = player.isFacingRight ? 1f : -1f;
        Vector3 offset = new Vector3(
            attackData.hitboxOffset.x * dir,
            attackData.hitboxOffset.y,
            0f
        );
        hb.transform.position = player.hitboxOrigin.position + offset;

        var col = hb.AddComponent<BoxCollider2D>();
        col.size = attackData.hitboxSize;
        col.isTrigger = true;

        var phb = hb.AddComponent<PlayerHitbox>();
        phb.attackData = attackData;

        Object.Destroy(hb, attackData.attackDuration);
    }
}
