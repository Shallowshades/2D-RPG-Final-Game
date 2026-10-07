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

    /// <summary>
    /// 装备背包里的物品: 有空槽位则直接装备, 否则替换该类型的第一个槽位
    /// </summary>
    /// <returns>true = 装备成功; false = 物品不在背包 / 没有匹配槽位 / 旧装备卸不下来</returns>
    public bool TryEquipItem(Inventory_Item item)
    {
        if (item == null || item.itemData == null) return false;

        // 必须真的在背包里(避免后续对 null 取属性)
        if (CanRemoveItem(item) == false)
        {
            Debug.LogWarning("[Inventory] 该物品不在背包中, 无法装备");
            return false;
        }

        List<Inventory_EquipmentSlot> matchingSlots = equipList.FindAll(slot => slot.slotType == item.itemData.itemType);
        if (matchingSlots.Count == 0)
        {
            Debug.LogWarning($"[Inventory] 没有能装备 {item.itemData.itemName} 的槽位");
            return false;
        }

        // 1. 有空槽位 → 先从背包移除, 再装上
        foreach (Inventory_EquipmentSlot slot in matchingSlots)
        {
            if (slot.HasItem()) continue;

            if (RemoveItem(item) == false) return false;

            EquipToSlot(item, slot);
            return true;
        }

        // 2. 替换: 先把新装备从背包取出以腾出格子, 旧装备才有地方放回
        Inventory_EquipmentSlot slotToReplace = matchingSlots[0];
        Inventory_Item itemToUnequip = slotToReplace.equippedItem;

        if (RemoveItem(item) == false) return false;

        if (itemToUnequip != null && UnequipItem(itemToUnequip) == false)
        {
            AddItem(item);      // 回滚: 把新装备放回背包
            return false;
        }

        EquipToSlot(item, slotToReplace);
        return true;
    }

    /// <summary>
    /// 把物品装到指定槽位(不含背包移除, 由调用方负责)
    /// </summary>
    private void EquipToSlot(Inventory_Item itemToEquip, Inventory_EquipmentSlot slot)
    {
        float savedHealthPercent = player.health.GetHealthPercent();

        slot.equippedItem = itemToEquip;
        itemToEquip.AddModifiers(player.stats);
        itemToEquip.AddItemEffect(player);

        player.health.SetHealthToPercent(savedHealthPercent);

        // 槽位要等这里才真正填上, 必须再通知一次 UI, 否则装备格会显示为空
        RaiseInventoryChanged();
    }

    /// <summary>
    /// 只读查询: 这件装备能否卸下(确实在某个槽位上, 且背包放得下)
    /// "背包满了不能卸"就卡在 CanAddItem 这一步
    /// </summary>
    public bool CanUnequipItem(Inventory_Item item)
    {
        if (item == null) return false;

        if (equipList.Exists(slot => slot.equippedItem == item) == false) return false;

        return CanAddItem(item.itemData);
    }

    /// <summary>
    /// 卸下装备并放回背包
    /// </summary>
    /// <returns>true = 卸下成功; false = 装备不在槽位上 / 背包已满</returns>
    public bool UnequipItem(Inventory_Item itemToUnequip)
    {
        if (itemToUnequip == null) return false;

        Inventory_EquipmentSlot slotToUnequip = equipList.Find(slot => slot.equippedItem == itemToUnequip);
        if (slotToUnequip == null)
        {
            Debug.LogWarning("[Inventory] 该装备不在任何装备槽上, 无法卸下");
            return false;
        }

        if (CanAddItem(itemToUnequip.itemData) == false)
        {
            Debug.LogWarning($"[Inventory] 背包已满, 无法卸下 {itemToUnequip.itemData.itemName}");
            return false;
        }

        float savedHealthPercent = player.health.GetHealthPercent();

        slotToUnequip.equippedItem = null;
        itemToUnequip.RemoveModifiers(player.stats);
        itemToUnequip.RemoveItemEffect();

        // 双保险: 万一放不回背包就整体回滚
        if (AddItem(itemToUnequip) == false)
        {
            slotToUnequip.equippedItem = itemToUnequip;
            itemToUnequip.AddModifiers(player.stats);
            itemToUnequip.AddItemEffect(player);
            return false;
        }

        player.health.SetHealthToPercent(savedHealthPercent);
        return true;
    }
}
