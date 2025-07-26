using UnityEngine;
using System.Collections;

public class DashState : IPlayerState
{
    private PlayerCombat player;
    private float duration = 0.2f;
    private float distance = 3f;

    public DashState(PlayerCombat player)
    {
        this.player = player;
    }

    public void Enter()
    {
        player.isBusy = true;
        player.StartCoroutine(DashRoutine());
    }

    public void Update() { }

    public void Exit() => player.isBusy = false;

    private IEnumerator DashRoutine()
    {
        Vector3 start = player.transform.position;
        float dir = player.isFacingRight ? 1f : -1f;
        Vector3 target = start + new Vector3(distance * dir, 0, 0);

        float t = 0f;
        while (t < duration)
        {
            float progress = t / duration;
            player.transform.position = Vector3.Lerp(start, target, progress);
            t += Time.deltaTime;
            yield return null;
        }

        player.transform.position = target;
        player.stateMachine.ChangeState(new IdleState(player));
    }
}
