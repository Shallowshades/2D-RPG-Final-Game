using System;
using UnityEngine;

[Serializable]
public class Inventory_Item
{
    public ItemData itemData;

    public Inventory_Item(ItemData itemData)
    {
        this.itemData = itemData;
    }
}
