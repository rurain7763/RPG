using System;
using UnityEngine;

[Serializable]
public class CostCondition : ICondition
{
    [SerializeField] private int requiredCost;

    public CostCondition() : this(0) { }

    public CostCondition(int requiredCost)
    {
        this.requiredCost = requiredCost;
    }

    public bool Evaluate()
    {
        return true;
    }
}