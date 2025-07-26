using UnityEngine;
using System.Collections;

public class AirborneState : IPlayerState
{
    private PlayerCombat player;
    private float jumpDuration = 0.5f;
    private float jumpHeight = 2f;

    public AirborneState(PlayerCombat player)
    {
        this.player = player;
    }

    public void Enter()
    {
        player.isAirborne = true;
        player.isBusy = true;
        player.StartCoroutine(JumpArc());
    }

    public void Update() { }

    public void Exit()
    {
        player.isAirborne = false;
        player.isBusy = false;
    }

    private IEnumerator JumpArc()
    {
        Vector3 start = player.transform.position;
        float time = 0f;

        float direction = player.isFacingRight ? 1f : -1f;
        float distance = 1.2f;
        Vector3 target = start + new Vector3(direction * distance, 0f, 0f);

        while (time < jumpDuration)
        {
            float progress = time / jumpDuration;
            float yOffset = Mathf.Sin(progress * Mathf.PI) * jumpHeight;
            Vector3 horizontal = Vector3.Lerp(start, target, progress);
            player.transform.position = new Vector3(horizontal.x, start.y + yOffset, start.z);

            time += Time.deltaTime;
            yield return null;
        }

        player.transform.position = new Vector3(target.x, start.y, start.z);
        player.stateMachine.ChangeState(new IdleState(player));
    }
}
