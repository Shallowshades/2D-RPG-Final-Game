using UnityEngine;

public abstract class EnemyState : EntityState
{
    protected Enemy enemy;

    public EnemyState(Enemy enemy, StateMachine stateMachine, string animationBoolName) : base(stateMachine, animationBoolName)
    {
        this.enemy = enemy;
        this.rb = enemy.rb;
        this.animator = enemy.animator;
        this.stats = enemy.stats;
    }

    public override void Update()
    {
        base.Update();


    }

    public override void UpdateAnimationParameters()
    {
        base.UpdateAnimationParameters();

        float battleMoveSpeedMultiplier = enemy.battleMoveSpeed / enemy.moveSpeed;
        animator.SetFloat("battleMoveSpeedMultiplier", battleMoveSpeedMultiplier);
        animator.SetFloat("moveSpeedMultiplier", enemy.moveSpeedMultiplier);
        animator.SetFloat("xVelocity", rb.linearVelocity.x);
    }
}
