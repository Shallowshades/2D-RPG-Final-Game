using UnityEngine;

public abstract class EntityState
{
    protected StateMachine stateMachine;
    protected string animationBoolName;

    protected Animator animator;
    protected Rigidbody2D rb;

    protected float stateTimer;
    protected bool triggerCalled;

    protected EntityState(StateMachine stateMachine, string animationBoolName)
    {
        this.stateMachine = stateMachine;
        this.animationBoolName = animationBoolName;
    }

    public virtual void Enter()
    {
        animator.SetBool(animationBoolName, true);
        triggerCalled = false;
    }

    public virtual void Update()
    {
        stateTimer -= Time.deltaTime;
    }

    public virtual void Exit()
    {
        animator.SetBool(animationBoolName, false);
    }

    public void CallAnimationTrigger()
    {
        triggerCalled = true;
    }
}
