using UnityEngine;

public class IdleState : IPlayerState
{
    private PlayerCombat player;

    public IdleState(PlayerCombat player)
    {
        this.player = player;
    }

    public void Enter()
    {
        player.isBusy = false;
        player.rb.linearVelocity = Vector2.zero;
    }

    public void Update()
    {
        Vector2 move = player.input.MoveInput;
        player.rb.linearVelocity = new Vector2(move.x * 5f, player.rb.linearVelocity.y);
    }

    public void Exit() { }
}
