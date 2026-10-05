using System.Collections.Generic;
using UnityEngine;

public class Inventory_Player : Inventory_Base
{
    private Player player;
    public List<Inventory_EquipmentSlot> equipList;

    protected override void Awake()
    {
        base.Awake();
        player = GetComponent<Player>();
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
        float savedHealthPercent = player.health.GetHealthPercent();

        slot.equippedItem = item;
        slot.equippedItem.AddModifiers(player.stats);
        
        player.health.SetHealthToPercent(savedHealthPercent);
        RemoveItem(item);
    }

    public void UnequipItem(Inventory_Item itemToUnequip)
    {
        if (CanAddItem() == false)
        {
            Debug.Log("No space");
            return;
        }

        float savedHealthPercent = player.health.GetHealthPercent();
        
        var slotToUnequip = equipList.Find(slot => slot.equippedItem == itemToUnequip);
        if (slotToUnequip != null)
        {
            slotToUnequip.equippedItem = null;
        }

        itemToUnequip.RemoveModifiers(player.stats);

        player.health.SetHealthToPercent(savedHealthPercent);
        AddItem(itemToUnequip);
    }
}
