using System.Collections.Generic;
using UnityEngine;

public class Inventory_Player : Inventory_Base
{
    private Entity_Stats playerStats;
    public List<Inventory_EquipmentSlot> equipList;

    protected override void Awake()
    {
        base.Awake();
        playerStats = GetComponent<Entity_Stats>();
    }

    public void TryEquipItem(Inventory_Item item)
    {
        Inventory_Item inventoryItem = FindItem(item.itemData);
        List<Inventory_EquipmentSlot> matchingSlots = equipList.FindAll(slot => slot.slotType == item.itemData.itemType);

        // 1. 装备
        foreach (Inventory_EquipmentSlot slot in matchingSlots)
        {
            if (slot.HasItem() == false)
            {
                EquipItem(inventoryItem, slot);
                return;
            }
        }

        // 2. 替换
        var slotToReplace = matchingSlots?[0];
        var itemToUnequip = slotToReplace?.equippedItem;

        UnequipItem(itemToUnequip);
        EquipItem(inventoryItem, slotToReplace);
    }

    private void EquipItem(Inventory_Item item, Inventory_EquipmentSlot slot)
    {
        slot.equippedItem = item;
        slot.equippedItem.AddModifiers(playerStats);

        RemoveItem(item);
    }

    public void UnequipItem(Inventory_Item item)
    {
        if (CanAddItem() == false)
        {
            Debug.Log("No space");
            return;
        }

        foreach (var slot in equipList)
        {
            if (slot.equippedItem == item)
            {
                slot.equippedItem = null;
                break;
            }
        }

        item.RemoveModifiers(playerStats);
        AddItem(item);
    }
}
