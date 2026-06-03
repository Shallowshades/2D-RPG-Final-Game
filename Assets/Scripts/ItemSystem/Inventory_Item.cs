using System;
using UnityEngine;

[Serializable]
public class Inventory_Item
{
    public ItemData itemData;
    public int stackSize = 1;

    public ItemModifier[] modifiers {  get; private set; }

    public Inventory_Item(ItemData itemData)
    {
        this.itemData = itemData;

        modifiers = IsEquipmentData()?.modifiers;
    }

    public void AddModifiers(Entity_Stats playerStats)
    {
        foreach (var modifier in modifiers)
        {
            Stats stats = playerStats.GetStatsByType(modifier.statsType);
            stats.AddModifier(modifier.value, itemData.itemName);
        }
    }

    public void RemoveModifiers(Entity_Stats playerStats)
    {
        foreach (var modifier in modifiers)
        {
            Stats stats = playerStats.GetStatsByType(modifier.statsType);
            stats.RemoveModifier(itemData.itemName);
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
