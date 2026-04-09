using UnityEngine;

public class Entity_Stats : MonoBehaviour
{
    public Stats maxHealth;
    public Stats_MajorGroup major;
    public Stats_OffenseGroup offense;
    public Stats_DefenseGroup defense;

    public float GetPhysicalDamage(out bool isCrit)
    {
        float baseDamage = offense.damage.GetValue();
        float bonusDamage = major.strength.GetValue();
        float totalBaseDamage = baseDamage + bonusDamage;

        float baseCritChance = offense.critChance.GetValue();
        float bonusCritChance = major.agility.GetValue() * 0.3f;
        float critChance = baseCritChance + bonusCritChance;

        float baseCritPower = offense.critPower.GetValue();
        float bonusCritPower = major.strength.GetValue() * 0.5f;
        float critPower = (baseCritPower + bonusCritPower) / 100;

        isCrit = Random.Range(0, 100) < critChance;
        float finalDamage = isCrit ? totalBaseDamage * critPower : totalBaseDamage;

        return finalDamage;
    }

    /// <summary>
    /// 物理减伤率
    /// </summary>
    /// <returns></returns>
    public float GetArmorMitigation(float armorReduction)
    {
        float baseArmor = defense.armor.GetValue();
        float bonusArmor = major.vitality.GetValue();
        float totalArmor = baseArmor + bonusArmor;

        float reductionMutliplier = Mathf.Clamp01(1 - armorReduction);
        float effectiveArmor = totalArmor * reductionMutliplier;

        float mitigation = effectiveArmor / (effectiveArmor + 100);
        float mitigationCap = 0.85f;

        float finalMitigation = Mathf.Clamp(mitigation, 0, mitigationCap);
        return finalMitigation;
    }

    public float GetArmorReduction()
    {
        float finalReduction = offense.armorReduction.GetValue() / 100;

        return finalReduction;
    }

    /// <summary>
    /// 闪避计算, 上限为85
    /// </summary>
    /// <returns></returns>
    public float GetEvasion()
    {
        float baseEvasion = defense.evasion.GetValue();
        float bonusEvasion = major.agility.GetValue() * 0.5f;

        float totalEvasion = baseEvasion + bonusEvasion;
        float evasionCap = 85f;

        float finalEvasion = Mathf.Clamp(totalEvasion, 0, evasionCap);

        return finalEvasion;
    }

    /// <summary>
    /// 最大生命值计算
    /// </summary>
    /// <returns></returns>
    public float GetMaxHealth()
    {
        float baseMaxHealth = maxHealth.GetValue();
        float bounsMaxHealth = major.vitality.GetValue() * 5;
        float finalMaxHealth = baseMaxHealth + bounsMaxHealth;

        return finalMaxHealth;
    }
}
