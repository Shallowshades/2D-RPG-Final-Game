using UnityEngine;

public class Enemy_DeadState : EnemyState
{
    private Collider2D collider;

    public Enemy_DeadState(Enemy enemy, StateMachine stateMachine, string animationBoolName) : base(enemy, stateMachine, animationBoolName)
    {
        collider = enemy.GetComponent<Collider2D>();
    }

    public override void Enter()
    {
        animator.enabled = false;
        collider.enabled = false;

        rb.gravityScale = 12;
        rb.linearVelocity = new Vector2(rb.linearVelocity.x, 15);

        stateMachine.SwitchOffStateMachine(); 
    }

}
