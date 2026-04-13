using Unity.VisualScripting;
using UnityEngine;

public class Player_SwordThrowState : PlayerState
{
    private Camera mainCamera;

    public Player_SwordThrowState(Player player, StateMachine stateMachine, string animationBoolName) : base(player, stateMachine, animationBoolName)
    {
    }

    public override void Enter()
    {
        base.Enter();

        skillManager.swordThrow.EnableDots(true);

        if (mainCamera != Camera.main)
        {
            mainCamera = Camera.main;
        }
    }

    public override void Update()
    {
        base.Update();

        Vector2 directionToMouse = DirectionToMouse();

        player.SetVelocity(0, rb.linearVelocity.y);
        player.HandleFlip(directionToMouse.x);
        skillManager.swordThrow.PredictTrajectory(directionToMouse);

        player.SetVelocity(0, rb.linearVelocity.y);

        // 执行动作
        if (input.Player.Attack.WasPressedThisFrame())
        {
            animator.SetBool("attackThrowSword_Perform", true);

            skillManager.swordThrow.EnableDots(false);
            skillManager.swordThrow.ConfirmTrajectory(directionToMouse);

            // skill manager create sword
        }

        // 取消动作
        // 无法确定投掷吉剑动作结束, 通过动画结束帧tigger来确定动作结束
        // 松开键位或者已经投掷出去结束标识
        if (input.Player.RangeAttack.WasReleasedThisFrame() || triggerCalled)
        {
            stateMachine.ChangeState(player.idleState);
        }
    }

    public override void Exit()
    {
        base.Exit();

        animator.SetBool("attackThrowSword_Perform", false);
        skillManager.swordThrow.EnableDots(false);
    }

    private Vector2 DirectionToMouse()
    {
        Vector2 playerPosition = player.transform.position;
        Vector2 worldMousePosition = mainCamera.ScreenToWorldPoint(player.mousePosition);

        Vector2 direction = worldMousePosition - playerPosition;

        return direction.normalized;
    }
}
