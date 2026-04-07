using UnityEngine;

public class Player_JumpAttackState : PlayerState
{
    private bool touchedGround;

    public Player_JumpAttackState(Player player, StateMachine stateMachine, string animationBoolName) : base(player, stateMachine, animationBoolName)
    {
    }

    public override void Enter()
    {
        base.Enter();
        animator.Update(0);
        touchedGround = false;
        player.SetVelocity(player.jumpAttackVelocity.x * player.facingDir, player.jumpAttackVelocity.y);
    }

    public override void Update()
    {
        base.Update();

        // 触发一次
        if (player.groundDetected && touchedGround == false)
        {
            touchedGround = true;
            animator.SetTrigger("jumpAttackTrigger");
            player.SetVelocity(0, rb.linearVelocity.y);
        }

        // 攻击结束信号
        if (triggerCalled && player.groundDetected)
        {
            stateMachine.ChangeState(player.idleState);
        }
    }

    public override void Exit() {
        base.Exit();
    }
}
