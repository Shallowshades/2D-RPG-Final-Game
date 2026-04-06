using UnityEngine;

public class Player_IdleState : Player_GroundedState
{
    public Player_IdleState(Player player, StateMachine stateMachine, string stateName) : base(player, stateMachine, stateName)
    {
    }

    public override void Enter()
    {
        base.Enter();

        player.SetVelocity(0, rb.linearVelocity.y);
    }

    public override void Update()
    {
        base.Update();

        // 阻止对墙移动; 可选
        if (player.moveInput.x == player.facingDir && player.wallDetected)
        {
            return;
        }

        if (player.moveInput.x != 0f)
        {
            stateMachine.ChangeState(player.moveState);
        }
    }
}
