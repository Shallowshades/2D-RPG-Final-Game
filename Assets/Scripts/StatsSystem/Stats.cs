using System.Collections.Generic;
using System;
using UnityEngine;

[Serializable]
public class Stats
{
    [SerializeField] private float baseValue;
    [SerializeField] private List<StatsModifier> modifiers = new List<StatsModifier>();

    private bool needToBeReCalculated = true;
    private float finalValue;

    public float GetValue()
    {
        if (needToBeReCalculated)
        {
            finalValue = GetFinalValue();
            needToBeReCalculated = false;
        }

        return finalValue;
    }

    public void AddModifier(float value, string source)
    {
        StatsModifier modToAdd = new StatsModifier(value, source);
        modifiers.Add(modToAdd);
        needToBeReCalculated = true;
    }

    public void RemoveModifier(string source)
    {
        modifiers.RemoveAll(modifier => modifier.source == source);
        needToBeReCalculated = true;
    }

    private float GetFinalValue()
    {
        float finalValue = baseValue;

        foreach (var modifier in modifiers)
        {
            finalValue = finalValue + modifier.value;
        }

        return finalValue;
    }

    public void SetBaseValue(float value) => baseValue = value;
}

[Serializable]
public class StatsModifier
{
    public float value;
    public string source;

    public StatsModifier(float value, string source)
    {
        this.value = value;
        this.source = source;
    }
}