using UnityEngine;
using System.Collections;

public class AttackState : IPlayerState
{
    private PlayerCombat combat;
    private PlayerAttackData data;
    private Transform playerTransform;
    private Vector3 startPos;
    private bool isSlam;
    private bool isLauncher;

    public AttackState(PlayerCombat combat, PlayerAttackData data)
    {
        this.combat = combat;
        this.data = data;
        this.playerTransform = combat.transform;
        this.startPos = playerTransform.position;
        this.isSlam = data.isSlam;
        this.isLauncher = data.isLauncher;
    }

    public void Enter()
    {
        combat.isBusy = true;

        if (isLauncher)
            combat.StartCoroutine(PerformLauncher());
        else if (isSlam)
            combat.StartCoroutine(PerformSlam());
        else
            combat.StartCoroutine(PerformAttack());
    }

    public void Exit()
    {
        combat.isBusy = false;
    }

    public void Update() { }

    // Normal attack (ground or air)
    private IEnumerator PerformAttack()
    {
        yield return new WaitForSeconds(data.windup);

        SpawnHitbox();

        if (data.hitPause > 0)
            yield return new WaitForSeconds(data.hitPause);

        yield return new WaitForSeconds(data.recovery);

        combat.stateMachine.ChangeState(combat.isAirborne ? new AirborneState(combat) : new IdleState(combat));
    }

    // Launcher punch that sends the player into the air
    private IEnumerator PerformLauncher()
    {
        float duration = data.launchDuration;
        float height = data.launchHeight;
        // Calculate the arc endpoint (keep Z from transform)
        Vector3 end = new Vector3(
            startPos.x + (combat.isFacingRight ? 2f : -2f),
            startPos.y,
            startPos.z
        );

        yield return new WaitForSeconds(data.windup);

        SpawnHitbox();

        if (data.hitPause > 0)
            yield return new WaitForSeconds(data.hitPause);

        float t = 0f;
        while (t < duration)
        {
            float progress = t / duration;
            float yOffset = Mathf.Sin(progress * Mathf.PI) * height;
            Vector3 horizontal = Vector3.Lerp(startPos, end, progress);
            playerTransform.position = new Vector3(horizontal.x, startPos.y + yOffset, startPos.z);
            t += Time.deltaTime;
            yield return null;
        }

        combat.isAirborne = true;
        combat.stateMachine.ChangeState(new AirborneState(combat));
    }

    // Slam attack that pulls the player downward
    private IEnumerator PerformSlam()
    {
        Vector3 start = playerTransform.position;
        Vector3 end = new Vector3(start.x, 0f, start.z); // assumes ground is at y = 0

        yield return new WaitForSeconds(data.windup);

        float duration = data.launchDuration * 0.6f;
        float t = 0f;
        while (t < duration)
        {
            float progress = t / duration;
            playerTransform.position = Vector3.Lerp(start, end, progress);
            t += Time.deltaTime;
            yield return null;
        }

        playerTransform.position = end;

        SpawnHitbox();

        if (data.hitPause > 0)
            yield return new WaitForSeconds(data.hitPause);

        yield return new WaitForSeconds(data.recovery);

        combat.isAirborne = false;
        combat.stateMachine.ChangeState(new IdleState(combat));
    }

    private void SpawnHitbox()
    {
        if (combat != null && combat.hitboxPrefab != null && combat.hitboxOrigin != null)
        {
            GameObject hitbox = Object.Instantiate(combat.hitboxPrefab, combat.hitboxOrigin.position, Quaternion.identity);
            PlayerHitbox box = hitbox.GetComponent<PlayerHitbox>();
            if (box != null)
            {
                box.Setup(data, !combat.isFacingRight); // flip X if facing left
            }
        }

    }
}
