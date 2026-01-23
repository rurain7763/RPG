using System;
using UnityEngine;

[Serializable]
public abstract class ValueModifierData
{
    public abstract ValueModifier CreateModifier(Arguments args = null);
}

[Serializable]
public class AddValueModifierData : ValueModifierData
{
    public float Value;

    public override ValueModifier CreateModifier(Arguments args = null)
    {
        return new AddValueModifier(Value);
    }
}

public class ScalableValueModifierDataArguments : Arguments
{
    public float Level;
}

[Serializable]
public class ScalableAddValueModifierData : ValueModifierData
{
    public AnimationCurve scalingCurve;

    public override ValueModifier CreateModifier(Arguments args = null)
    {
        if (args is ScalableValueModifierDataArguments scalableArgs)
        {
            float scaledValue = scalingCurve.Evaluate(scalableArgs.Level);
            return new AddValueModifier(scaledValue);
        }

        return new AddValueModifier(0f);
    }
}
