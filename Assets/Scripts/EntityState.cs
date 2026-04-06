using UnityEngine;

public abstract class EntityState
{
    protected Player player;
    protected StateMachine stateMachine;
    protected string animationBoolName;

    protected Animator animator;
    protected Rigidbody2D rb;
    protected PlayerInputSet input;

    protected float stateTimer;
    protected bool triggerCalled;

    public EntityState(Player player, StateMachine stateMachine, string animationBoolName)
    {
        this.player = player;
        this.stateMachine = stateMachine;
        this.animationBoolName = animationBoolName;

        this.animator = player.animator;
        this.rb = player.rb;
        this.input = player.input;
    }

    public virtual void Enter()
    {
        animator.SetBool(animationBoolName, true);
        triggerCalled = false;
    }

    public virtual void Update()
    {
        stateTimer -= Time.deltaTime;
        animator.SetFloat("yVelocity", rb.linearVelocity.y);

        if (input.Player.Dash.WasPressedThisFrame() && CanDash())
        {
            stateMachine.ChangeState(player.dashState);
        }
    }

    public virtual void Exit()
    {
        animator.SetBool(animationBoolName, false);
    }

    public void CallAnimationTrigger()
    {
        triggerCalled = true;
    }

    private bool CanDash()
    {
        if (player.wallDetected)
        {
            return false;
        }

        if (stateMachine.currentState == player.dashState)
        {
            return false;
        }

        return true;
    }
}
