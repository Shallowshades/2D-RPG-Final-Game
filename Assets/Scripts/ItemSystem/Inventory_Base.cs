using System;
using System.Collections.Generic;
using UnityEngine;

public class Inventory_Base : MonoBehaviour
{
    public event Action onInventoryChange;

    public int maxInventorySize = 10;
    public List<Inventory_Item> itemList = new List<Inventory_Item>();

    public bool CanAddItem() => itemList.Count < maxInventorySize;

    public void AddItem(Inventory_Item item)
    {
        Inventory_Item itemInInventory = FindItem(item.itemData);
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

    public Inventory_Item FindItem(ItemData itemData)
    {
        return itemList.Find(item => item.itemData == itemData && item.CanAddStack());
    }
}
