using System.Collections.Generic;

namespace Experimental
{
    public enum GameplayEffectDurationPolicy
    {
        Instant,
        Timed,
        Infinite
    }

    public enum GameplayEffectStackExpirationPolicy
    {
        ClearEntireEffect,
        RemoveSingleStack,
    }

    public enum GameplayEffectModifierType
    {
        AttributeModifier,
        PeriodicExecution
    }

    public class GameplayEffectModifier
    {
        public GameplayAttributeModifierData ModifierData;
        public GameplayAttributeSet AttributeSet;
        public GameplayAttribute Attribute;
        public ValueModifier Modifier;
    }

    public class GameplayEffect
    {
        public readonly GameplayEffectData EffectData;
        public List<GameplayEffectModifier> Modifiers = new();
        public float RemainingDuration;
        public float RemainingPeriodDuration;

        internal GameplayAbilitySystem abilitySystem;

        private int level;
        private int stackCount;

        public int Level
        {
            get => level;
            private set
            {
                if (level != value)
                {
                    level = value;
                    CalcDuration();
                }
            }
        }

        public int StackCount
        {
            get => stackCount;
            set
            {
                if (stackCount != value)
                {
                    stackCount = value;
                    CalcDuration();
                }
            }
        }

        public float Duration { get; private set; }
        public bool IsMaxStacked => StackCount >= EffectData.MaxStacks;

        public GameplayEffect(GameplayEffectData data, int level)
        {
            EffectData = data;
            Level = level;
            StackCount = 1;
            CalcDuration();
        }

        public GameplayEffectContextArgs GetContextArgs()
        {
            return new GameplayEffectContextArgs
            {
                Level = Level,
                StackCount = StackCount,
                Source = abilitySystem
            };
        }

        private void CalcDuration()
        {
            var contextArgs = GetContextArgs();
            Duration = EffectData.DurationMagnitude.GetDuration(contextArgs);
        }
    }
}