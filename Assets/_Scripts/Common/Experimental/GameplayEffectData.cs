using System;
using System.Collections.Generic;
using UnityEngine;

namespace Experimental
{
    [CreateAssetMenu(fileName = "NewGameplayEffect", menuName = "Common/Experimental/Gameplay Effect")]
    public class GameplayEffectData : ScriptableObject
    {
        public GameplayEffectDurationPolicy DurationPolicy;
        [SerializeReference, SubclassSelector] public GameplayEffectDurationMagnitude DurationMagnitude;

        public int MaxStacks = 1;
        public GameplayEffectStackExpirationPolicy StackExpirationPolicy;
        public bool RefreshDurationOnStack;

        public float Period = 0f;
        public bool ExecutePeriodicEffectOnApplied = false;
        public GameplayTagContainer IgnorePeriodicEffectTags;

        public GameplayTagContainer GrantedTags;

        public bool IsStackable => MaxStacks > 1;
        public bool HasPeriod => Period > 0f;

        [SerializeReference, SubclassSelector] public List<GameplayAttributeModifierData> ModifierDatas;
    }

    public class GameplayEffectContextArgs : Arguments
    {
        public float Level = 1;
        public int StackCount = 1;
        public GameplayAbilitySystem Source;
    }

    [Serializable]
    public abstract class GameplayEffectDurationMagnitude
    {
        public abstract float GetDuration(GameplayEffectContextArgs args);
    }

    [Serializable]
    public class GameplayEffectFixedDurationMagnitude : GameplayEffectDurationMagnitude
    {
        public float Duration;

        public override float GetDuration(GameplayEffectContextArgs args)
        {
            return Duration;
        }
    }

    [Serializable]
    public class GameplayEffectScaledDurationMagnitude : GameplayEffectDurationMagnitude
    {
        public AnimationCurve ScalingCurve;

        public override float GetDuration(GameplayEffectContextArgs args)
        {
            return ScalingCurve.Evaluate(args.Level);
        }
    }

    [Serializable]
    public class GameplayEffectAttributeBasedDurationMagnitude : GameplayEffectDurationMagnitude
    {
        public GameplayAttributeData AttributeData;
        public float Coefficient = 1f;
        public float Constant = 0f;

        public override float GetDuration(GameplayEffectContextArgs args)
        {
            if (args.Source == null)
            {
                Logger.Warn("GameplayEffectAttributeBasedDurationMagnitude: Source is null, returning constant duration.");
                return Constant;
            }

            float attributeValue = args.Source.GetAttributeValue(AttributeData);
            return (attributeValue * Coefficient) + Constant;
        }
    }
}