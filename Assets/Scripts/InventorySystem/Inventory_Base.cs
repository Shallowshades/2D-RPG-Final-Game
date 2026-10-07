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

    /// <summary>
    /// 使用消耗品: 执行物品效果并扣除一个堆叠
    /// </summary>
    /// <returns>true = 成功使用; false = 物品不在背包 / 没有可执行效果</returns>
    public bool TryUseItem(Inventory_Item itemToUse)
    {
        Inventory_Item consumable = itemList.Find(item => item == itemToUse);

        if (consumable == null)
        {
            Debug.LogWarning("[Inventory] 该物品不在背包中, 无法使用");
            return false;
        }

        if (consumable.itemEffect == null)
        {
            Debug.LogWarning($"[Inventory] {consumable.itemData.itemName} 没有可执行的效果");
            return false;
        }

        consumable.itemEffect.ExecuteEffect();

        // 最后一件: 整条移除(成功时由 RemoveItem 负责通知 UI)
        if (consumable.stackSize <= 1)
        {
            return RemoveItem(consumable);
        }

        consumable.RemoveStack();
        RaiseInventoryChanged();
        return true;
    }

    /// <summary>
    /// 只读查询: 这件物品能否放入背包(能叠到同类堆叠上, 或还有空格子)
    /// 注意: 写路径请直接调用 AddItem, 不要写成"先查后加"
    /// </summary>
    public bool CanAddItem(ItemData itemData)
    {
        if (itemData == null) return false;

        // 1) 能叠到已有的同类未满堆叠上 → 不需要新格子
        if (FindItemCanStack(itemData) != null) return true;

        // 2) 否则需要一个空格子
        return itemList.Count < maxInventorySize;
    }

    /// <summary>
    /// 唯一添加入口: 内部先做容量检测, 失败时不改动数据也不通知 UI
    /// </summary>
    /// <returns>true = 成功加入; false = 背包已满或物品非法</returns>
    public bool AddItem(Inventory_Item item)
    {
        if (item == null || item.itemData == null)
        {
            Debug.LogWarning("[Inventory] AddItem 收到非法物品, 加入失败");
            return false;
        }

        if (CanAddItem(item.itemData) == false)
        {
            Debug.LogWarning($"[Inventory] 背包已满, 无法加入 {item.itemData.itemName} " +
                             $"({itemList.Count}/{maxInventorySize})");
            return false;
        }

        Inventory_Item stackTarget = FindItemCanStack(item.itemData);
        if (stackTarget != null)
        {
            stackTarget.AddStack();
        }
        else
        {
            itemList.Add(item);
        }

        RaiseInventoryChanged();
        return true;
    }

    /// <summary>
    /// 只读查询: 该实例是否真的在背包里
    /// </summary>
    public bool CanRemoveItem(Inventory_Item item) => item != null && itemList.Contains(item);

    /// <summary>
    /// 唯一移除入口: 内部先做存在性检测, 失败时不改动数据也不通知 UI
    /// </summary>
    /// <returns>true = 成功移除; false = 该实例不在背包中</returns>
    public bool RemoveItem(Inventory_Item item)
    {
        if (CanRemoveItem(item) == false)
        {
            Debug.LogWarning("[Inventory] 该物品不在背包中, 移除失败");
            return false;
        }

        itemList.Remove(item);      // 按实例移除, 避免误删同 itemData 的另一条堆叠
        RaiseInventoryChanged();
        return true;
    }

    public Inventory_Item FindItem(ItemData itemData)
    {
        return itemList.Find(item => item.itemData == itemData);
    }

    public Inventory_Item FindItemCanStack(ItemData itemData)
    {
        return itemList.Find(item => item.itemData == itemData && item.CanAddStack());
    }

    public void TriggerUpdateUI() => RaiseInventoryChanged();

    /// <summary>
    /// 只在数据真的发生变化时调用, 避免失败路径白刷 UI
    /// </summary>
    protected void RaiseInventoryChanged() => onInventoryChange?.Invoke();
}
