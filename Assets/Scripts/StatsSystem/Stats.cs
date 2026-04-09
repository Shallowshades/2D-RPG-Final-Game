using System;
using UnityEngine;

[Serializable]
public class Stats
{
    [SerializeField] private float baseValue;

    public float GetValue()
    {
        return baseValue;
    }
}
