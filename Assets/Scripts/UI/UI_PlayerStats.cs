using UnityEngine;

public class UI_PlayerStats : MonoBehaviour
{
    private UI_StatsSlot[] uiStatsSlots;
    private Inventory_Player inventory;

    private void Awake()
    {
        uiStatsSlots = GetComponentsInChildren<UI_StatsSlot>();

        inventory = FindFirstObjectByType<Inventory_Player>();
        inventory.onInventoryChange += UpdateStatsUI;
    }

    private void Start()
    {
        UpdateStatsUI();
    }

    private void UpdateStatsUI()
    {
        foreach(var statsSlot in uiStatsSlots)
        {
            statsSlot.UpdateStatsValue();
        }
    }
}
