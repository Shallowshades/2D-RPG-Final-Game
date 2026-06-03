using System;
using UnityEngine;

[CreateAssetMenu(menuName = "RPG Setup/Item Data/Equipment Item", fileName = "Equipment Data - ")]
public class EquipmentData : ItemData
{
    [Header("Item modifiers")]
    public ItemModifier[] modifiers;
}

[Serializable]
public class ItemModifier
{
    public StatsType statsType;
    public float value;
}
