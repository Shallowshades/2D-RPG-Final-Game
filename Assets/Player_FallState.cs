using UnityEngine;

public class Player_FallState : Player_AiredState
{
    public Player_FallState(Player player, StateMechine stateMachine, string animationBoolName) : base(player, stateMachine, animationBoolName)
    {
    }

    public override void Update()
    {
        base.Update();

        if (player.groundDetected)
        {
            // rb.linearVelocity = Vector2.zero;
            stateMachine.ChangeState(player.idleState);
        }

        if (player.wallDetected)
        {
            stateMachine.ChangeState(player.wallSlideState);
        }
    }
}
