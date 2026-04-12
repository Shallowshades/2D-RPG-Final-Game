using UnityEngine;

public enum SkillUpgradeType
{
    None,

    // ---- Dash Tree ----
    Dash,
    Dash_CloneOnStart,
    Dash_CloneOnStartAndArrival,
    Dash_ShardOnStart,
    Dash_ShardOnStartAndArrival,

    // ---- Shard Tree ----
    Shard,
    Shard_MoveToEnemy,
    Shard_Multicast,
    Shard_Teleport,
    Shard_TeleportHpRewind,
}
