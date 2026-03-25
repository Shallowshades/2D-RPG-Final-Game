using UnityEngine;

public abstract class EntityState
{
    protected Player player;
    protected StateMechine stateMachine;
    protected string animationBoolName;

    protected Animator animator;
    protected Rigidbody2D rb;
    protected PlayerInputSet input;

    public EntityState(Player player, StateMechine stateMachine, string animationBoolName)
    {
        this.player = player;
        this.stateMachine = stateMachine;
        this.animationBoolName = animationBoolName;

        animator = player.animator;
        rb = player.rb;
        input = player.input;
    }

    public virtual void Enter()
    {
        animator.SetBool(animationBoolName, true);
    }

    public virtual void Update()
    {
        animator.SetFloat("yVelocity", rb.linearVelocity.y);
    }

    public virtual void Exit()
    {
        animator.SetBool(animationBoolName, false);
    }
}
