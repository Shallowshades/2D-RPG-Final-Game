using UnityEngine;

public class Player_IdleState : Player_GroundedState
{
    public Player_IdleState(Player player, StateMechine stateMachine, string stateName) : base(player, stateMachine, stateName)
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

        if (player.moveInput.x != 0f)
        {
            stateMachine.ChangeState(player.moveState);
        }
    }
}
