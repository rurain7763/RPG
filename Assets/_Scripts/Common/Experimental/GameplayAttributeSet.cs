using System.Collections.Generic;

namespace Experimental
{
    public class GameplayAttribute
    {
        public readonly GameplayAttributeData AttributeData;

        private ModifiableValue mValue;

        public float BaseValue { 
            get => mValue.BaseValue; 
            set => mValue.BaseValue = value;
        }

        public float CurrentValue => mValue.FinalValue;

        public GameplayAttribute(GameplayAttributeData data)
        {
            AttributeData = data;
            mValue = new ModifiableValue(data.DefaultValue);
        }

        public GameplayAttribute(GameplayAttributeData data, float overrideValue)
        {
            AttributeData = data;
            mValue = new ModifiableValue(overrideValue);
        }

        public void AddModifier(ValueModifier modifier)
        {
            mValue.AddModifier(modifier);
        }

        public bool RemoveModifier(ValueModifier modifier)
        {
            return mValue.RemoveModifier(modifier);
        }
    }

    public abstract class GameplayAttributeSet
    {
        public abstract IEnumerable<GameplayAttribute> Attributes { get; }

        public virtual void Init(GameplayAbilitySystem abilitySystem) { }

        public abstract bool HasAttribute(GameplayAttribute attribute);
        public abstract GameplayAttribute GetAttribute(GameplayAttributeData attributeData);
        public virtual void PreAttributeBaseValueChange(GameplayAttribute attribute, ref float newValue) { }
        public virtual void PostAttributeBaseValueChange(GameplayAttribute attribute, float oldValue) { }
        public virtual void PostAttributeCurrentValueChange(GameplayAttribute attribute) { }
        public virtual bool PreGameplayEffectExecute(GameplayEffectData effect, GameplayAbility ability) => true;
        public virtual void PostGameplayEffectExecute(GameplayEffectData effect, GameplayAbility ability) { }
    }
}