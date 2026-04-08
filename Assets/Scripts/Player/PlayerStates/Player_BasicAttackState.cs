using UnityEngine;

public class Player_BasicAttackState : PlayerState
{
    private float attackVelocityTimer;
    private float lastTimeAttacked;

    private int attackDir;
    private int comboIndex = 0;
    private bool comboAttackQueued;


    public Player_BasicAttackState(Player player, StateMachine stateMachine, string animationBoolName) : base(player, stateMachine, animationBoolName)
    {
        if (player.attackVelocity.Length != 3)
        {
            Debug.Log("攻击后座力数组长度不匹配");
        }
    }

    public override void Enter()
    {
        base.Enter();
        ResetComboIndexIfNeeded();
        attackDir = player.moveInput.x != 0 ? (int)player.moveInput.x : player.facingDir;

        animator.SetInteger("basicAttackIndex", comboIndex);
        ApplyAttackVelocity();
    }

    public override void Update()
    {
        base.Update();
        HandleAttackVelocity();

        // 检测敌人

        if (input.Player.Attack.WasPressedThisFrame())
        {
            QueueNextAttack();
        }

        if (triggerCalled)
        {
            if (comboAttackQueued)
            {
                comboAttackQueued = false;
                animator.SetBool(animationBoolName, false);
                player.EnterAttackStateWithDelay();
            }
            else
            {
                stateMachine.ChangeState(player.idleState);
            }
        }
    }

    public override void Exit()
    {
        base.Exit();
        comboIndex = (comboIndex + 1) % 3;
        lastTimeAttacked = Time.time;
    }

    private void QueueNextAttack()
    {
        if (comboIndex < 2)
        {
            comboAttackQueued = true;
        }
    }

    private void ResetComboIndexIfNeeded()
    {
        // 长时间不攻击, 连招中断
        if (Time.time > lastTimeAttacked + player.comboxResetTime)
        {
            comboIndex = 0;
        }
    }

    private void HandleAttackVelocity()
    {
        attackVelocityTimer -= Time.deltaTime;
        if (attackVelocityTimer < 0)
        {
            player.SetVelocity(0, rb.linearVelocity.y);
        }
    }

    /// <summary>
    /// 应用一个攻击的后坐力, 让人物攻击时更加真实
    /// </summary>
    private void ApplyAttackVelocity()
    {
        attackVelocityTimer = player.attackVelocityDuration;
        player.SetVelocity(player.attackVelocity[comboIndex].x * attackDir, player.attackVelocity[comboIndex].y);
    }
}
