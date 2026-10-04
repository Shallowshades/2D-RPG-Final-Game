using System;
using System.Collections.Generic;
using UnityEngine;

public class Inventory_Base : MonoBehaviour
{
    public event Action onInventoryChange;

    public int maxInventorySize = 10;
    public List<Inventory_Item> itemList = new List<Inventory_Item>();

    protected virtual void Awake()
    {

    }

    public void TryUseItem(Inventory_Item itemToUse)
    {
        Inventory_Item consumable = itemList.Find(item => item == itemToUse);

        if (consumable == null)
        {
            return;
        }

        consumable.itemEffect.ExecuteEffect();
        
        if (consumable.stackSize > 1)
        {
            consumable.RemoveStack();
        }
        else
        {
            RemoveItem(consumable);
        }
        onInventoryChange?.Invoke();
    }

    public bool CanAddItem()
    {
        if (itemList.Count < maxInventorySize) return true;

        foreach (var item in itemList)
        {
            if (item.CanAddStack()) return true;
        }

        return false;
    }

    public void AddItem(Inventory_Item item)
    {
        Inventory_Item itemInInventory = FindItemCanStack(item.itemData);
        if (itemInInventory != null)
        {
            itemInInventory.AddStack();
        }
        else
        {
            itemList.Add(item);
        }
        onInventoryChange?.Invoke();
    }

    public void RemoveItem(Inventory_Item item)
    {
        itemList.Remove(FindItem(item.itemData));
        onInventoryChange?.Invoke();
    }

    public Inventory_Item FindItem(ItemData itemData)
    {
        return itemList.Find(item => item.itemData == itemData);
    }

    public Inventory_Item FindItemCanStack(ItemData itemData)
    {
        return itemList.Find(item => item.itemData == itemData && item.CanAddStack());
    }
}
