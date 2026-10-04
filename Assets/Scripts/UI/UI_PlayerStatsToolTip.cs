using TMPro;
using UnityEngine;

public class UI_PlayerStatsToolTip : UI_ToolTip
{
    private Player_Stats playerStats;
    private TextMeshProUGUI playerStatsToolTipText;

    protected override void Awake()
    {
        base.Awake();
        playerStats = FindFirstObjectByType<Player_Stats>();
        playerStatsToolTipText = GetComponentInChildren<TextMeshProUGUI>();
    }

    public void ShowToolTip(bool show, RectTransform targetRect, StatsType statsType)
    {
        base.ShowToolTip(show, targetRect);
        playerStatsToolTipText.text = GetStatsTextByType(statsType);
    }

    public string GetStatsTextByType(StatsType statsType)
    {
        switch(statsType)
        {
            // Major Attributes
            case StatsType.Strength:
                return "Increases physical damage by 1 per point." +
                       "\nIncreases critical power by 0.5% per point.";
            case StatsType.Agility:
                return "Increases critical chance by 0.3% per point." +
                       "\nIncreases evasion by 0.5% per point.";
            case StatsType.Intelligence:
                return "Increases elemental resistances by 0.5% per point." +
                        "\nAdds 1 elemental damage per point as a bonus. " +
                        "\nIf all elements have 0 damage, the bonus will not be applied.";
            case StatsType.Vitality:
                return "Increases maximum health by 5 per point" +
                       "\nIncreases armor by 1 per point.";

            // Physical Damage
            case StatsType.Damage:
                return "Determines the physical damage of your attacks.";
            case StatsType.CritChance:
                return "Chance for your attacks to critically strike.";
            case StatsType.CritPower:
                return "Increases the damage dealt by critical strikes.";
            case StatsType.ArmorReduction:
                return "Percent of armor that will be ignored by your attacks.";
            case StatsType.AttackSpeed:
                return "Determines how quickly you can attack.";

            // Defense
            case StatsType.MaxHealth:
                return "Determines how much total health you have.";
            case StatsType.HealthRegen:
                return "Amount of health restored per second.";
            case StatsType.Armor:
                return "Reduces incoming physical damage."
                    + "\nArmor mitigation is Limited at 85%."
                    + "Current mitigation is: " + playerStats.GetArmorMitigation(0) * 100 + "%.";
            case StatsType.Evasion:
                return "Chance to completely avoid attacks." + "\n Limited at 85%.";

            // Elemental Damage
            case StatsType.IceDamage:
                return "Determines the ice damage of your attacks.";
            case StatsType.FireDamage:
                return "Determines the fire damage of your attacks.";
            case StatsType.LightningDamage:
                return "Determines the lightning damage of your attacks.";
            case StatsType.ElementalDamage:
                return
                    "Elemental damage combines all three elements. " +
                    "\nThe highest element applies corresponding element status effect and full damage. " +
                    "\nThe other two elements contribute 50% of their damage as a bonus.";

            // Elemental Resistances
            case StatsType.IceResistance:
                return "Reduces ice damage taken.";
            case StatsType.FireResistance:
                return "Reduces fire damage taken.";
            case StatsType.LightningResistance:
                return "Reduces lightning damage taken.";

            default:
                return "No tooltip avalible for this stat.";
        }
    }
}
