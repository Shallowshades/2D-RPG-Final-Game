using UnityEngine;

public class Player_WallSlideState : EntityState
{
    // TODO: 
    // 滑墙状态需按方向键进入, 如墙在右侧, 需按右键
    // 滑墙状态时正按方向键时, 吸附于墙面
    // 滑墙状态时可通过反向方向键退出滑墙状态
    // 
    // 疑问?
    // 进入滑墙状态需在Air类中 ?
    // 当上跳,yVelocity > 0时, 若按正方向键, 吸附于墙面, yVelocity应为0
    // 当进入滑墙状态时, yVelocity就不应该大于0

    public Player_WallSlideState(Player player, StateMachine stateMachine, string animationBoolName) : base(player, stateMachine, animationBoolName)
    {
    }

    public override void Update()
    {
        base.Update();
        HandleWallSlide();

        if (input.Player.Jump.WasPressedThisFrame())
        {
            stateMachine.ChangeState(player.wallJumpState);
        }

        if (player.wallDetected == false)
        {
            stateMachine.ChangeState(player.fallState);
        }

        if (player.groundDetected)
        {
            stateMachine.ChangeState(player.idleState);
            player.Flip();
        }
    }

    private void HandleWallSlide()
    {
        if (player.moveInput.y < 0)
        {
            player.SetVelocity(0, rb.linearVelocity.y);
        }
        else
        {
            player.SetVelocity(0, rb.linearVelocity.y * player.wallSlideSlowMultiplier);
        }
    }
}
