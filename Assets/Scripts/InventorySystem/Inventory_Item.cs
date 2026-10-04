using System;
using UnityEngine;

[Serializable]
public class Inventory_Item
{
    private string itemId;

    public ItemData itemData;
    public int stackSize = 1;

    public ItemModifier[] modifiers {  get; private set; }
    public ItemEffect_DataSO itemEffect;

    public Inventory_Item(ItemData itemData)
    {
        this.itemData = itemData;
        modifiers = IsEquipmentData()?.modifiers;
        itemEffect = itemData.itemEffect;

        itemId = itemData.itemName + " - " + Guid.NewGuid().ToString();
    }

    public void AddModifiers(Entity_Stats playerStats)
    {
        foreach (var modifier in modifiers)
        {
            Stats stats = playerStats.GetStatsByType(modifier.statsType);
            stats.AddModifier(modifier.value, itemId);
        }
    }

    public void RemoveModifiers(Entity_Stats playerStats)
    {
        foreach (var modifier in modifiers)
        {
            Stats stats = playerStats.GetStatsByType(modifier.statsType);
            stats.RemoveModifier(itemId);
        }
    }

    private EquipmentData IsEquipmentData()
    {
        if (itemData is EquipmentData equipment)
        {
            return equipment;
        }
        return null;
    }

    public bool CanAddStack() => stackSize < itemData.maxStackSize;
    public void AddStack() => stackSize++;
    public void RemoveStack() => stackSize--;
}
