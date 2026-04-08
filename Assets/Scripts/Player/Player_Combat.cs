using System.Runtime.CompilerServices;
using UnityEngine;

public class Player_Combat : Entity_Combat
{
    [Header("Counter attack details")]
    [SerializeField] private float counterRecovery;
    public bool CounterAttackPerformed()
    {
        bool hasPerformCounter = false;

        foreach (var target in GetDetectedColliders())
        {
            ICounterable counterable = target.GetComponent<ICounterable>();
            if (counterable == null)
            {
                continue;
            }
            
            if (counterable.CanBeCountered)
            {
                counterable.HandleCounter();
                hasPerformCounter = true;
            }
        }

        return hasPerformCounter;
    }

    public float GetCounterRecoveryDuration() => counterRecovery;
}
