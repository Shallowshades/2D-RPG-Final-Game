using System.Security.Cryptography;
using UnityEngine;

public class Enemy_BattleState : EnemyState
{
    private Transform player;
    private float lastTimeWasInBattle;

    public Enemy_BattleState(Enemy enemy, StateMachine stateMachine, string animationBoolName) : base(enemy, stateMachine, animationBoolName)
    {
    }

    public override void Enter()
    {
        base.Enter();

        // 全局查找, 必定tle
        // player = GameObject.Find("Player").transform;

        //// 看似诡异的双重判断
        //// 1, 正面检测到玩家
        //if (player == null)
        //{
        //    player = enemy.PlayerDetected().transform;
        //}
        //// 2, 被玩家背刺
        //if (player == null)
        //{
        //    player = enemy.player;
        //}

        UpdateBattleTimer();

        // 另一种写法
        // ?? 相当于 is null 的检测
        player ??= enemy.GetPlayerReference();

        if (ShouldRetreat())
        {
            rb.linearVelocity = new Vector2(enemy.retreatVelocity.x * -DirectionToPlayer(), enemy.retreatVelocity.y);
            enemy.HandleFlip(DirectionToPlayer());
        }
    }

    public override void Update()
    {
        base.Update();

        if (enemy.PlayerDetected())
        {
            UpdateBattleTimer();
        }

        if (BattleTimeIsOver())
        {
            stateMachine.ChangeState(enemy.idleState);
        }

        if (WithinAttackRange() && enemy.PlayerDetected())
        {
            stateMachine.ChangeState(enemy.attackState);
        }
        else
        {
            // 调整对着玩家
            enemy.SetVelocity(enemy.battleMoveSpeed * DirectionToPlayer(), rb.linearVelocity.y);
        } 
    }

    private void UpdateBattleTimer() => lastTimeWasInBattle = Time.time;

    private bool BattleTimeIsOver() => Time.time > lastTimeWasInBattle + enemy.battleTimeDuration;

    private bool WithinAttackRange() => DistanceToPlayer() < enemy.attackDistance;

    private bool ShouldRetreat() => DistanceToPlayer() < enemy.minRetreatDistance;

    public float DistanceToPlayer()
    {
        if (player == null)
        {
            return float.MaxValue;
        }

        return Mathf.Abs(player.position.x - enemy.transform.position.x);
    }

    private int DirectionToPlayer()
    {
        if (player == null)
        {
            return 0;
        }

        return player.position.x > enemy.transform.position.x ? 1 : -1;
    }
}
