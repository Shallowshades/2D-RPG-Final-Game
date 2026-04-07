using UnityEngine;

public class Enemy_GroundedState : EnemyState
{
    public Enemy_GroundedState(Enemy enemy, StateMachine stateMachine, string animationBoolName) : base(enemy, stateMachine, animationBoolName)
    {
    }

    public override void Update()
    {
        base.Update();

        if (enemy.PlayerDetected() == true)
        {
            stateMachine.ChangeState(enemy.battleState);
        }
    }
}
