using UnityEngine;

public class Enemy_MoveState : Enemy_GroundedState
{
    public Enemy_MoveState(Enemy enemy, StateMachine stateMachine, string animationBoolName) : base(enemy, stateMachine, animationBoolName)
    {
    }

    public override void Enter()
    {
        base.Enter();

        if (enemy.groundDetected == false || enemy.wallDetected == true)
        {
            enemy.Flip();
        }
    }

    public override void Update()
    {
        base.Update();

        enemy.SetVelocity(enemy.GetMoveSpeed() * enemy.facingDir, rb.linearVelocity.y);

        if (enemy.groundDetected == false || enemy.wallDetected == true)
        {
            stateMachine.ChangeState(enemy.idleState);
        }
    }
}
