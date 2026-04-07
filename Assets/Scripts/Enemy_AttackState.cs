using UnityEngine;

public class Enemy_AttackState : EnemyState
{
    public Enemy_AttackState(Enemy enemy, StateMachine stateMachine, string animationBoolName) : base(enemy, stateMachine, animationBoolName)
    {
    }

    public override void Update()
    {
        base.Update();
        Debug.Log("triggerCalled = " + triggerCalled);
        if (triggerCalled)
        {
            stateMachine.ChangeState(enemy.battleState);
        }
    }
}
