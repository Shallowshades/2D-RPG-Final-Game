using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

public class UI_StatsSlot : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    private Player_Stats playerStats;
    private RectTransform rect;
    private UI ui;

    [SerializeField] private StatsType statsSlotType;
    [SerializeField] private TextMeshProUGUI statsName;
    [SerializeField] private TextMeshProUGUI statsValue;

    private void OnValidate()
    {
        gameObject.name = "UI_Stats - " + GetStatsNameByType(statsSlotType);
        statsName.text = GetStatsNameByType(statsSlotType);
    }
    
    private void Awake()
    {
        ui = GetComponentInParent<UI>();
        rect = GetComponent<RectTransform>();
        playerStats = FindFirstObjectByType<Player_Stats>();

        // 名字之前只在 OnValidate 里写过(那只在编辑器校验时执行),
        // 运行时不会刷新 -> 这里补一次, 否则每行都停留在占位文本
        if (statsName != null)
        {
            statsName.text = GetStatsNameByType(statsSlotType);
        }
    }

    public void UpdateStatsValue()
    {
        Stats statsToUpdate = playerStats.GetStatsByType(statsSlotType);

        if (statsToUpdate == null && statsSlotType != StatsType.ElementalDamage) {
            return;
        }

        float value = 0;
        switch (statsSlotType)
        {
            // Major stats
            case StatsType.Strength:
                value = playerStats.major.strength.GetValue(); break;
            case StatsType.Agility:
                value = playerStats.major.agility.GetValue(); break;
            case StatsType.Intelligence:
                value = playerStats.major.intelligence.GetValue(); break;
            case StatsType.Vitality:
                value = playerStats.major.vitality.GetValue(); break;

            // Offense stats
            case StatsType.Damage:
                value = playerStats.GetBaseDamage(); break;
            case StatsType.CritChance:
                value = playerStats.GetCritChance(); break;
            case StatsType.CritPower:
                value = playerStats.GetCritPower(); break;
            case StatsType.ArmorReduction:
                value = playerStats.GetArmorReduction() * 100; break;
            case StatsType.AttackSpeed:
                value = playerStats.offense.attackSpeed.GetValue() * 100; break;

            // Defense stats
            case StatsType.MaxHealth:
                value = playerStats.GetMaxHealth(); break;
            case StatsType.HealthRegen:
                value = playerStats.resources.healthRegen.GetValue(); break;
            case StatsType.Evasion:
                value = playerStats.GetEvasion(); break;
            case StatsType.Armor:
                value = playerStats.GetBaseArmor(); break;

            // Elemental Damage stats
            case StatsType.IceDamage:
                value = playerStats.offense.iceDamage.GetValue(); break;
            case StatsType.FireDamage:
                value = playerStats.offense.fireDamage.GetValue(); break;
            case StatsType.LightningDamage:
                value = playerStats.offense.lightningDamage.GetValue(); break;
            case StatsType.ElementalDamage:
                value = playerStats.GetElementalDamage(out ElementType element, 1); break;
  
            // Elemental Resistance stats
            case StatsType.IceResistance:
                value = playerStats.GetElementalResistance(ElementType.Ice); break;
            case StatsType.FireResistance:
                value = playerStats.GetElementalResistance(ElementType.Fire); break;
            case StatsType.LightningResistance:
                value = playerStats.GetElementalResistance(ElementType.Lightning); break;
            
        }

        statsValue.text = IsPercentageStats(statsSlotType) ? value + "%" : value.ToString();
    }

    private bool IsPercentageStats(StatsType type)
    {
        switch (type)
        {
            case StatsType.CritChance:
            case StatsType.CritPower:
            case StatsType.ArmorReduction:
            case StatsType.IceResistance:
            case StatsType.FireResistance:
            case StatsType.LightningResistance:
            case StatsType.AttackSpeed:
            case StatsType.Evasion:
                return true;
            default:
                return false;
        }
    }

    private string GetStatsNameByType(StatsType type)
    {
        switch (type)
        {
            case StatsType.MaxHealth: return "Max Health";
            case StatsType.HealthRegen: return "Health Regeneration";
            case StatsType.Strength: return "Strength";
            case StatsType.Agility: return "Agility";
            case StatsType.Intelligence: return "Intelligence";
            case StatsType.Vitality: return "Vitality";
            case StatsType.AttackSpeed: return "Attack Speed";
            case StatsType.Damage: return "Damage";
            case StatsType.CritChance: return "Critical Chance";
            case StatsType.CritPower: return "Critical Power";
            case StatsType.ArmorReduction: return "Armor Reduction";
            case StatsType.FireDamage: return "Fire Damage";
            case StatsType.IceDamage: return "Ice Damage";
            case StatsType.LightningDamage: return "Lightning Damage";
            case StatsType.ElementalDamage: return "Elemental Damage";
            case StatsType.Armor: return "Armor";
            case StatsType.Evasion: return "Evasion";
            case StatsType.IceResistance: return "Ice Resistance";
            case StatsType.FireResistance: return "Fire Resistance";
            case StatsType.LightningResistance: return "Lightning Resistance";
            default: return "Unknown Stat";
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        ui.playerStatsToolTip.ShowToolTip(true, rect, statsSlotType);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        ui.playerStatsToolTip.ShowToolTip(false, null);
    }
}