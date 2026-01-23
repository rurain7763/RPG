using System;
using UnityEngine;

namespace Experimental
{
    [CreateAssetMenu(fileName = "NewGameplayAttributeData", menuName = "Common/Experimental/Gameplay Attribute Data")]
    public class GameplayAttributeData : ScriptableObject
    {
        public float DefaultValue;
        [TextArea] public string Comment;
    }

    [Serializable]
    public abstract class GameplayAttributeModifierData : ValueModifierData
    {
        public GameplayEffectModifierType ModifierType;
        public GameplayAttributeData AttributeData;

        public override ValueModifier CreateModifier(Arguments args = null)
        {
            if (args == null)
            {
                args = new GameplayEffectContextArgs();
            }

            return CreateModifier(args as GameplayEffectContextArgs);
        }

        public abstract ValueModifier CreateModifier(GameplayEffectContextArgs args);
    }

    public class GameplayAttributeStaticModifierData<T> : GameplayAttributeModifierData where T : ValueModifier, new()
    {
        public float Value;

        public override ValueModifier CreateModifier(GameplayEffectContextArgs args)
        {
            var mod = new T();
            mod.Value = Value;

            return mod;
        }
    }

    public class GameplayAttributeScaledModifierData<T> : GameplayAttributeModifierData where T : ValueModifier, new()
    {
        public AnimationCurve ScalingCurve;

        public override ValueModifier CreateModifier(GameplayEffectContextArgs args)
        {
            var mod = new T();
            mod.Value = ScalingCurve.Evaluate(args.Level);
            return mod;
        }
    }

    public class GameplayAttributeAttributeBasedModifierData<T> : GameplayAttributeModifierData where T : ValueModifier, new()
    {
        public GameplayAttributeData SourceAttributeData;
        public float Coefficient = 1f;
        public float Constant = 0f;

        public override ValueModifier CreateModifier(GameplayEffectContextArgs args)
        {
            var mod = new T();

            var currentValue = args.Source.GetAttributeValue(SourceAttributeData);
            mod.Value = currentValue * Coefficient + Constant;

            return mod;
        }
    }

    [Serializable] public class GameplayAttributeAddModifierData : GameplayAttributeStaticModifierData<AddValueModifier> { }
    [Serializable] public class GameplayAttributeScaledAddModifierData : GameplayAttributeScaledModifierData<AddValueModifier> { }
    [Serializable] public class GameplayAttributeAttributeBasedAddModifierData : GameplayAttributeAttributeBasedModifierData<AddValueModifier> { }

    // You can create similar classes for other types of ValueModifiers as needed.
}