using UnityEngine;

public abstract class PlayerState : EntityState
{
    protected Player player;
    protected PlayerInputSet input;
    protected Player_SkillManager skillManager;

    public PlayerState(Player player, StateMachine stateMachine, string animationBoolName) : base(stateMachine, animationBoolName)
    {
        this.player = player;
        this.animator = player.animator;
        this.rb = player.rb;
        this.input = player.input;
        this.stats = player.stats;
        this.skillManager = player.skillManager;
    }

    public override void Update()
    {
        base.Update();

        if (input.Player.Dash.WasPressedThisFrame() && CanDash())
        {
            skillManager.dash.SetSkillOnCooldown();
            stateMachine.ChangeState(player.dashState);
        }
    }

    public override void UpdateAnimationParameters()
    {
        base.UpdateAnimationParameters();

        animator.SetFloat("yVelocity", rb.linearVelocity.y);
    }

    private bool CanDash()
    {
        if (skillManager.dash.CanUseSkill() == false) return false;

        if (player.wallDetected) return false;

        if (stateMachine.currentState == player.dashState) return false;

        return true;
    }
}
