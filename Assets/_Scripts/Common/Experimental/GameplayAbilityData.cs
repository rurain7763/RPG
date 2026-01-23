using UnityEngine;

namespace Experimental
{
    public abstract class GameplayAbilityData : ScriptableObject
    {
        public GameplayTagContainer AbilityTags;
        public GameplayTag CancelAbilityWithTag;
        public GameplayTag BlockAbilityWithTag;
        public GameplayTagContainer ActivationOwnedTags;
        public GameplayTagContainer ActivationRequiredTags;
        public GameplayTagContainer ActivationBlockedTags;

        public GameplayEffectData CostEffect;
        public GameplayEffectData CooldownEffect;

        public GameplayAbilityInstancePolicy InstancePolicy;

        public abstract GameplayAbility CreateAbility(int level);
    }
}