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

    // ---- Throw Sword Tree ----
    SwordThrow,
    SwordThrow_Spin,
    SwordThrow_Pierce,
    SwordThrow_Bounce,

    // ---- Time Echo Tree ----
    TimeEcho,  // Create a clone of a player. It can take damage from enemies.
    TimeEcho_SingleAttack, // Time Echo can perform a single attack
    TimeEcho_MultiAttack, // Time Echo can perform N attacks
    TimeEcho_ChanceToDuplicate, // Time Echo has a chance to create another time echo when attacks

    TimeEcho_HealWisp, // When time echo dies it creates a wips that flies towards the player to heal it.
                       // Heal is = to percantage of damage taken when died
    TimeEcho_CleanseWisp, // Wisp will now remove negative effects from player
    TimeEcho_CooldownWisp, // Wisp will reduce cooldown of all skills by N second. 

    // ------ Domain Expansion -------
    Domain_SlowingDown, // Create an area in which you slow down enemies by 90-100% . You can freely move and fight.
    Domain_EchoSpam, // You can no longer move, but you spam enemy with Time Echo ability
    Domain_ShardSpam // You can no longer move, but you spam enemy with Time Shard ability
}
