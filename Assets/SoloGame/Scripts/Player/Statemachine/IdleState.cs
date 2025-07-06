public class IdleState : IPlayerState
{
    private PlayerCombat player;

    public IdleState(PlayerCombat combatRef)
    {
        player = combatRef;
    }

    public void Enter() { }
    public void Update() { }
    public void Exit() { }
}