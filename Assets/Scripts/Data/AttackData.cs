using System;
using UnityEngine;

[Serializable]
public class AttackData
{
    public float physicalDamage;
    public float elementalDamage;
    public bool isCrit;
    public ElementType elementType;

    public ElementalEffectData effectData;

    public AttackData(Entity_Stats entityStats, DamageScaleData scaleData)
    {
        physicalDamage = entityStats.GetPhysicalDamage(out isCrit, scaleData.physical);
        elementalDamage = entityStats.GetElementalDamage(out elementType, scaleData.elemental);

        effectData = new ElementalEffectData(entityStats, scaleData);
    }
}
