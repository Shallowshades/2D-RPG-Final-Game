using UnityEngine;

public class Entity_Stats : MonoBehaviour
{
    public StatsSetupData defaultStatsSetup;

    public Stats_ResourceGroup resources;
    public Stats_MajorGroup major;
    public Stats_OffenseGroup offense;
    public Stats_DefenseGroup defense;

    protected virtual void Awake()
    {
        
    }

    public AttackData GetAttackData(DamageScaleData scaleData)
    {
        return new AttackData(this, scaleData);
    }

    public float GetElementalDamage(out ElementType element, float scaleFactor)
    {
        float fireDamage = offense.fireDamage.GetValue();
        float iceDamage = offense.iceDamage.GetValue();
        float lightningDamage = offense.lightningDamage.GetValue();
        
        float highestDamage = fireDamage;
        element = ElementType.Fire;

        if (iceDamage > highestDamage)
        {
            highestDamage = iceDamage;
            element = ElementType.Ice;
        }

        if (lightningDamage > highestDamage)
        {
            highestDamage = lightningDamage;
            element = ElementType.Lightning;
        }

        if (highestDamage <= 0)
        {
            element = ElementType.None;
            return 0;
        }

        float bonusElementalDamage = major.intelligence.GetValue();

        float bonusFire = (element == ElementType.Fire) ? 0 : fireDamage * 0.5f;
        float bonusIce = (element == ElementType.Ice) ? 0 : iceDamage * 0.5f;
        float bonusLightning = (element == ElementType.Lightning) ? 0 : lightningDamage * 0.5f;
        
        float weakerElementsDamage = bonusFire + bonusIce + bonusLightning;
        float finalDamage = highestDamage + bonusElementalDamage + weakerElementsDamage;

        return finalDamage * scaleFactor;
    }

    public float GetElementalResistance(ElementType element)
    {
        float baseResistance = 0;
        float bonusResistance = major.intelligence.GetValue() * 0.5f;

        switch(element)
        {
            case ElementType.Fire:
                baseResistance = defense.fireResistance.GetValue(); 
                break;
            case ElementType.Ice:
                baseResistance = defense.iceResistance.GetValue();
                break;
            case ElementType.Lightning:
                baseResistance = defense.lightningResistance.GetValue();
                break;
            default:
                break;
        }

        float resistance = baseResistance + bonusResistance;
        float resistanceCap = 75f;
        float finalResistance = Mathf.Clamp(resistance, 0, resistanceCap);
        return finalResistance;
    }

    public float GetPhysicalDamage(out bool isCrit, float scaleFactor = 1)
    {
        float baseDamage = GetBaseDamage();
        float critChance = GetCritChance();
        float critPower = GetCritPower() / 100.0f;

        isCrit = Random.Range(0, 100) < critChance;
        float finalDamage = isCrit ? baseDamage * critPower : baseDamage;

        return finalDamage * scaleFactor;
    }

    public float GetBaseDamage() => offense.damage.GetValue() + major.strength.GetValue();
    public float GetCritChance() => offense.critChance.GetValue() + (major.agility.GetValue() * .3f);
    public float GetCritPower() => offense.critPower.GetValue() + (major.strength.GetValue() * .5f);
    /// <summary>
    /// 物理减伤率
    /// </summary>
    /// <returns></returns>
    public float GetArmorMitigation(float armorReduction)
    {
        float baseArmor = GetBaseArmor();

        float reductionMutliplier = Mathf.Clamp01(1 - armorReduction);
        float effectiveArmor = baseArmor * reductionMutliplier;

        float mitigation = effectiveArmor / (effectiveArmor + 100);
        float mitigationCap = 0.85f;

        float finalMitigation = Mathf.Clamp(mitigation, 0, mitigationCap);
        return finalMitigation;
    }

    public float GetBaseArmor() => defense.armor.GetValue() + major.vitality.GetValue();

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
        float baseMaxHealth = resources.maxHealth.GetValue();
        float bounsMaxHealth = major.vitality.GetValue() * 5;
        float finalMaxHealth = baseMaxHealth + bounsMaxHealth;

        return finalMaxHealth;
    }

    public Stats GetStatsByType(StatsType type)
    {
        switch (type)
        {
            case StatsType.MaxHealth: return resources.maxHealth;
            case StatsType.HealthRegen: return resources.healthRegen;

            case StatsType.Strength: return major.strength;
            case StatsType.Agility: return major.agility;
            case StatsType.Intelligence: return major.intelligence;
            case StatsType.Vitality: return major.vitality;

            case StatsType.AttackSpeed: return offense.attackSpeed;
            case StatsType.Damage: return offense.damage;
            case StatsType.CritChance: return offense.critChance;
            case StatsType.CritPower: return offense.critPower;
            case StatsType.ArmorReduction: return offense.armorReduction;

            case StatsType.FireDamage: return offense.fireDamage;
            case StatsType.IceDamage: return offense.iceDamage;
            case StatsType.LightningDamage: return offense.lightningDamage;

            case StatsType.Armor: return defense.armor;
            case StatsType.Evasion: return defense.evasion;

            case StatsType.IceResistance: return defense.iceResistance;
            case StatsType.FireResistance: return defense.iceResistance;
            case StatsType.LightningResistance: return defense.iceResistance;

            default:
                // Debug.LogWarning($"StatsType {type} not implemented yet.");
                return null;
        }
    }

    [ContextMenu("Update Default Stats Setup")]
    public void ApplyDefaultStatsSetup()
    {
        if (defaultStatsSetup == null)
        {
            Debug.Log("No default stat setup assigned");
            return;
        }

        resources.maxHealth.SetBaseValue(defaultStatsSetup.maxHealth);
        resources.healthRegen.SetBaseValue(defaultStatsSetup.healthRegen);

        major.strength.SetBaseValue(defaultStatsSetup.strength);
        major.agility.SetBaseValue(defaultStatsSetup.agility);
        major.intelligence.SetBaseValue(defaultStatsSetup.intelligence);
        major.vitality.SetBaseValue(defaultStatsSetup.vitality);

        offense.attackSpeed.SetBaseValue(defaultStatsSetup.attackSpeed);
        offense.damage.SetBaseValue(defaultStatsSetup.damage);
        offense.critChance.SetBaseValue(defaultStatsSetup.critChance);
        offense.critPower.SetBaseValue(defaultStatsSetup.critPower);
        offense.armorReduction.SetBaseValue(defaultStatsSetup.armorReduction);

        offense.iceDamage.SetBaseValue(defaultStatsSetup.iceDamage);
        offense.fireDamage.SetBaseValue(defaultStatsSetup.fireDamage);
        offense.lightningDamage.SetBaseValue(defaultStatsSetup.lightningDamage);

        defense.armor.SetBaseValue(defaultStatsSetup.armor);
        defense.evasion.SetBaseValue(defaultStatsSetup.evasion);

        defense.iceResistance.SetBaseValue(defaultStatsSetup.iceResistance);
        defense.fireResistance.SetBaseValue(defaultStatsSetup.fireResistance);
        defense.lightningResistance.SetBaseValue(defaultStatsSetup.lightningResistance);
    }
}
