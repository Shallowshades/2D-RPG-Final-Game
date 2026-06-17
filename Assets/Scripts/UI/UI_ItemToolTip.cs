using System.Text;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using static UnityEditor.Rendering.ShadowCascadeGUI;

public class UI_ItemToolTip : UI_ToolTip
{
    [SerializeField] private TextMeshProUGUI itemName;
    [SerializeField] private TextMeshProUGUI itemType;
    [SerializeField] private TextMeshProUGUI itemInfo;

    public void ShowToolTip(bool show, RectTransform targetRect, Inventory_Item itemToShow)
    {
        base.ShowToolTip(show, targetRect);
        itemName.text = itemToShow.itemData.itemName;
        itemType.text = itemToShow.itemData.itemType.ToString();
        itemInfo.text = GetItemInfo(itemToShow);
    }

    public string GetItemInfo(Inventory_Item item)
    {
        if (item.itemData.itemType == ItemType.Material)
        {
            return "Used for crafting";
        }

        StringBuilder sb = new StringBuilder();
        sb.AppendLine("");

        foreach (var mod in item.modifiers)
        {
            string modType = mod.statsType.ToString();
            string modValue = IsPercentageStats(mod.statsType) ? mod.value.ToString() + "%" : mod.value.ToString();
            sb.AppendLine("+ " + modValue + " " + modType);
        }

        return sb.ToString();
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
            case StatsType.Armor: return "Armor";
            case StatsType.Evasion: return "Evasion";
            case StatsType.IceResistance: return "Ice Resistance";
            case StatsType.FireResistance: return "Fire Resistance";
            case StatsType.LightningResistance: return "Lightning Resistance";
            default: return "Unknown Stat";
        }
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
}
