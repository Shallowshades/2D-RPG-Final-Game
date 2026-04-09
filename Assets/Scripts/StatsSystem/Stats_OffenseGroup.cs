using System;

[Serializable]
public class Stats_OffenseGroup
{
    // 物理伤害
    public Stats damage;
    public Stats critPower;
    public Stats critChance;
    public Stats armorReduction;

    // 元素伤害
    public Stats fireDamage;
    public Stats iceDamage;
    public Stats lightningDamage;
}
