using System;
using System.Collections.Generic;
using UnityEngine;

namespace Experimental
{
    public class GameplayAbilitySystem : MonoBehaviour
    {
        [SerializeField] private List<GameplayAbilityData> abilityDatas;
        [SerializeField] private GameplayTagContainer tags;

        private List<GameplayAttributeSet> attributeSets;

        private Dictionary<GameplayTag, int> tagReferenceCounts;
        private FastList<GameplayAbility> activeAbilities;
        private FastList<GameplayEffect> activeEffects;

        public GameObject Owner { get; private set; }
        public GameplayTagContainer Tags => tags;

        public event Action OnTagsChanged;

        public void Init(GameObject owner)
        {
            Owner = owner;
            tags = new GameplayTagContainer();
            attributeSets = new();
            tagReferenceCounts = new();
            activeAbilities = new();
            activeEffects = new();
        }

        public void Tick(float delta)
        {
            HandleAcitiveAbilities(delta);
            HandleActiveEffects(delta);
        }

        private void HandleAcitiveAbilities(float delta)
        {
            GameplayTagContainer removedTags = new();
            for (int i = activeAbilities.Count - 1; i >= 0; i--)
            {
                var ability = activeAbilities[i];
                ability.Execute(delta);
                if (ability.IsActive == false)
                {
                    activeAbilities.RemoveAt(i);
                    removedTags.Add(ability.AbilityData.ActivationOwnedTags);
                }
            }
            RemoveGameplayTags(removedTags);
        }

        private void HandleActiveEffects(float delta)
        {
            for (int i = activeEffects.Count - 1; i >= 0; i--)
            {
                var effect = activeEffects[i];
                HandlePeriodricEffect(delta, effect);
                HandleTimedEffect(delta, effect);
            }
        }

        private void HandlePeriodricEffect(float delta, GameplayEffect effect)
        {
            if (!effect.EffectData.HasPeriod)
            {
                return;
            }

            effect.RemainingPeriodDuration -= delta;
            if (effect.RemainingPeriodDuration > 0f)
            {
                return;
            }

            effect.RemainingPeriodDuration += effect.EffectData.Period;

            ExecuteEffectPeriodicLogic(effect);
        }

        private void HandleTimedEffect(float delta, GameplayEffect effect)
        {
            if (effect.EffectData.DurationPolicy != GameplayEffectDurationPolicy.Timed)
            {
                return;
            }

            effect.RemainingDuration -= delta;
            if (effect.RemainingDuration > 0f)
            {
                return;
            }

            if (effect.EffectData.IsStackable && effect.StackCount > 1)
            {
                if (effect.EffectData.StackExpirationPolicy == GameplayEffectStackExpirationPolicy.ClearEntireEffect)
                {
                    RemoveActiveGameplayEffect(effect);
                }
                else if (effect.EffectData.StackExpirationPolicy == GameplayEffectStackExpirationPolicy.RemoveSingleStack)
                {
                    effect.StackCount--;
                    effect.RemainingDuration += effect.Duration;
                    UpdateEffectModifiers(effect);
                }
            }
            else
            {
                RemoveActiveGameplayEffect(effect);
            }
        }

        public void GiveAbility(GameplayAbilityData abilityData)
        {
            abilityDatas.Add(abilityData);
        }

        private bool CheckAbilityActivationRequirements(GameplayAbilityData abilityData)
        {
            if (!tags.All(abilityData.ActivationRequiredTags))
            {
                return false;
            }

            return true;
        }

        private bool CheckAbilityActivationBlockers(GameplayAbilityData abilityData)
        {
            if (tags.Any(abilityData.ActivationBlockedTags))
            {
                return false;
            }

            foreach (var ability in activeAbilities)
            {
                if (ability.AbilityData.BlockAbilityWithTag != null && abilityData.AbilityTags.Has(ability.AbilityData.BlockAbilityWithTag))
                {
                    return false;
                }
            }

            return true;
        }

        private bool CheckAbilityCost(GameplayAbilityData abilityData, int level)
        {
            if (abilityData.CostEffect == null)
            {
                return true;
            }

            var args = new GameplayEffectContextArgs
            {
                Level = level,
                Source = this
            };

            foreach (var modifierData in abilityData.CostEffect.ModifierDatas)
            {
                GetAttribute(modifierData.AttributeData, out var attributeSet, out var attribute);
                if (attributeSet == null || attribute == null)
                {
                    continue;
                }

                var modifier = modifierData.CreateModifier(args);

                float oldValue = attribute.CurrentValue;
                float newValue = oldValue;
                modifier.Calculate(ref newValue);

                if (newValue < oldValue && newValue < 0f)
                {
                    return false;
                }
            }

            return true;
        }

        private bool CheckAbilityCooldown(GameplayAbilityData abilityData)
        {
            if (abilityData.CooldownEffect != null)
            {
                if (tags.Any(abilityData.CooldownEffect.GrantedTags))
                {
                    return false;
                }
            }

            return true;
        }

        public bool TryActivateAbility(GameplayAbilityData abilityData, int level = 1)
        {
            if (!abilityDatas.Contains(abilityData))
            {
                return false;
            }

            if (!CheckAbilityActivationRequirements(abilityData))
            {
                return false;
            }

            if (!CheckAbilityActivationBlockers(abilityData))
            {
                return false;
            }

            if (!CheckAbilityCost(abilityData, level))
            {
                return false;
            }

            if (!CheckAbilityCooldown(abilityData))
            {
                return false;
            }

            if (abilityData.InstancePolicy == GameplayAbilityInstancePolicy.InstancedPerOwner)
            {
                if (activeAbilities.Any(a => a.AbilityData == abilityData))
                {
                    return false;
                }
            }

            if (abilityData.CancelAbilityWithTag != null)
            {
                CancelAbilitiesWithTag(abilityData.CancelAbilityWithTag);
            }

            var ability = abilityData.CreateAbility(level);
            ability.abilitySystem = this;
            ability.Activate();

            activeAbilities.Add(ability);
            AddGameplayTags(abilityData.ActivationOwnedTags);

            return true;
        }

        public int TryActivateAbilityByTags(GameplayTagContainer tags, int level = 1, bool matchAll = true)
        {
            int count = 0;
            foreach (var abilityData in abilityDatas)
            {
                if (matchAll ? abilityData.AbilityTags.All(tags) : abilityData.AbilityTags.Any(tags))
                {
                    if (TryActivateAbility(abilityData, level))
                    {
                        count++;
                    }
                }
            }

            return count;
        }

        public bool TryActivateAbilityByIndex(int index, int level = 1)
        {
            if (index < 0 || index >= abilityDatas.Count)
            {
                return false;
            }

            return TryActivateAbility(abilityDatas[index], level);
        }

        public void CancelAbility(GameplayAbilityData abilityData)
        {
            var ability = activeAbilities.FirstOrDefault(a => a.AbilityData == abilityData);
            if (ability != null)
            {
                ability.Cancel();
                activeAbilities.Remove(ability);
                RemoveGameplayTags(abilityData.ActivationOwnedTags);
            }
        }

        public void CancelAbility(GameplayAbility ability)
        {
            int index = activeAbilities.IndexOf(ability);
            if (index >= 0)
            {
                ability.Cancel();
                activeAbilities.RemoveAt(index);
                RemoveGameplayTags(ability.AbilityData.ActivationOwnedTags);
            }
        }

        public void CancelAbilitiesWithTag(GameplayTag tag)
        {
            GameplayTagContainer removedTags = new();
            for (int i = activeAbilities.Count - 1; i >= 0; i--)
            {
                var ability = activeAbilities[i];
                if (ability.AbilityData.AbilityTags.Has(tag))
                {
                    ability.Cancel();
                    activeAbilities.RemoveAt(i);
                    removedTags.Add(ability.AbilityData.ActivationOwnedTags);
                }
            }
            RemoveGameplayTags(removedTags);
        }

        public void AddAttributeSet(GameplayAttributeSet attributeSet)
        {
            attributeSet.Init(this);
            attributeSets.Add(attributeSet);
        }

        public void GetAttribute(GameplayAttributeData attributeData, out GameplayAttributeSet outAttributeSet, out GameplayAttribute outAttribute)
        {
            outAttribute = null;
            outAttributeSet = null;
            foreach (var attributeSet in attributeSets)
            {
                var attribute = attributeSet.GetAttribute(attributeData);
                if (attribute != null)
                {
                    outAttribute = attribute;
                    outAttributeSet = attributeSet;
                    return;
                }
            }
        }

        public float GetAttributeValue(GameplayAttributeData attributeData)
        {
            GetAttribute(attributeData, out var attributeSet, out var attribute);
            if (attributeSet == null || attribute == null)
            {
                return 0f;
            }
            return attribute.CurrentValue;
        }

        public void ApplyInstantAttributeChange(GameplayAttributeData attributeData, float newValue)
        {
            GetAttribute(attributeData, out var attributeSet, out var attribute);
            if (attributeSet == null || attribute == null)
            {
                return;
            }

            ApplyInstantAttributeChange(attributeSet, attribute, newValue);
        }
        
        public void ApplyInstantAttributeChange(GameplayAttributeSet attributeSet, GameplayAttribute attribute, float newValue)
        {
            float oldValue = attribute.BaseValue;
            attributeSet.PreAttributeBaseValueChange(attribute, ref newValue);
            attribute.BaseValue = newValue;
            attributeSet.PostAttributeBaseValueChange(attribute, oldValue);
        }

        public GameplayEffect ApplyGameplayEffectToSelf(GameplayEffectData effectData, int level = 1)
        {
            if (effectData.IsStackable)
            {
                var existingEffect = activeEffects.FirstOrDefault(e => e.EffectData == effectData);
                if (existingEffect != null)
                {
                    if (!existingEffect.IsMaxStacked)
                    {
                        existingEffect.StackCount++;
                        UpdateEffectModifiers(existingEffect);
                    }

                    if (effectData.RefreshDurationOnStack)
                    {
                        existingEffect.RemainingDuration = existingEffect.Duration;
                    }

                    return existingEffect;
                }
            }

            var effect = new GameplayEffect(effectData, level);
            effect.abilitySystem = this;

            var modifierArgs = effect.GetContextArgs();
            foreach (var modifierData in effect.EffectData.ModifierDatas)
            {
                GetAttribute(modifierData.AttributeData, out var attributeSet, out var attribute);
                if (attributeSet == null || attribute == null)
                {
                    continue;
                }

                var effectModifier = new GameplayEffectModifier
                {
                    ModifierData = modifierData,
                    AttributeSet = attributeSet,
                    Attribute = attribute,
                    Modifier = modifierData.CreateModifier(modifierArgs)
                };

                if (modifierData.ModifierType == GameplayEffectModifierType.PeriodicExecution)
                {
                    effect.Modifiers.Add(effectModifier);
                }
                else if (effect.EffectData.DurationPolicy == GameplayEffectDurationPolicy.Instant)
                {
                    float oldValue = attribute.BaseValue;
                    float newValue = attribute.BaseValue;

                    effectModifier.Modifier.Calculate(ref newValue);

                    attributeSet.PreAttributeBaseValueChange(attribute, ref newValue);
                    attribute.BaseValue = newValue;
                    attributeSet.PostAttributeBaseValueChange(attribute, oldValue);
                }
                else
                {
                    attribute.AddModifier(effectModifier.Modifier);
                    attributeSet.PostAttributeCurrentValueChange(attribute);
                    effect.Modifiers.Add(effectModifier);
                }
            }

            if (effect.EffectData.DurationPolicy == GameplayEffectDurationPolicy.Infinite)
            {
                AddGameplayTags(effect.EffectData.GrantedTags);
                effect.RemainingDuration = 0;
                activeEffects.Add(effect);
            }
            else if (effect.EffectData.DurationPolicy == GameplayEffectDurationPolicy.Timed)
            {
                AddGameplayTags(effect.EffectData.GrantedTags);
                effect.RemainingDuration = effect.Duration;
                activeEffects.Add(effect);
            }

            if (effect.EffectData.ExecutePeriodicEffectOnApplied)
            {
                ExecuteEffectPeriodicLogic(effect);
            }

            return effect;
        }

        public void RemoveActiveGameplayEffect(GameplayEffect effect)
        {
            if (effect.abilitySystem != this)
            {
                return;
            }

            foreach (var modifier in effect.Modifiers)
            {
                modifier.Attribute.RemoveModifier(modifier.Modifier);
                modifier.AttributeSet.PostAttributeCurrentValueChange(modifier.Attribute);
            }

            activeEffects.Remove(effect);

            RemoveGameplayTags(effect.EffectData.GrantedTags);
        }

        public void UpdateEffectModifiers(GameplayEffect effect)
        {
            var modiferArgs = effect.GetContextArgs();
            foreach (var modifier in effect.Modifiers)
            {
                if (modifier.ModifierData.ModifierType != GameplayEffectModifierType.AttributeModifier)
                {
                    continue;
                }

                modifier.Attribute.RemoveModifier(modifier.Modifier);
                modifier.Modifier = modifier.ModifierData.CreateModifier(modiferArgs);
                modifier.Attribute.AddModifier(modifier.Modifier);
                modifier.AttributeSet.PostAttributeCurrentValueChange(modifier.Attribute);
            }
        }

        public void ExecuteEffectPeriodicLogic(GameplayEffect effect)
        {
            if (tags.Any(effect.EffectData.IgnorePeriodicEffectTags))
            {
                return;
            }

            var modifierArgs = effect.GetContextArgs();
            foreach (var modifier in effect.Modifiers)
            {
                if (modifier.ModifierData.ModifierType != GameplayEffectModifierType.PeriodicExecution)
                {
                    continue;
                }

                modifier.Modifier = modifier.ModifierData.CreateModifier(modifierArgs);

                float oldValue = modifier.Attribute.BaseValue;
                float newValue = oldValue;
                modifier.Modifier.Calculate(ref newValue);

                ApplyInstantAttributeChange(modifier.AttributeSet, modifier.Attribute, newValue);
            }
        }

        public void EachGameplayEffectsByTags(GameplayTagContainer targetTags, Action<GameplayEffect> callback)
        {
            foreach (var effect in activeEffects)
            {
                if (effect.EffectData.GrantedTags.Any(targetTags))
                {
                    callback?.Invoke(effect);
                }
            }
        }

        public void AddGameplayTags(GameplayTagContainer tags)
        {
            foreach (var tag in tags)
            {
                if (!tagReferenceCounts.ContainsKey(tag))
                {
                    tagReferenceCounts[tag] = 0;
                }

                tagReferenceCounts[tag]++;
                this.tags.Add(tag);
            }

            OnTagsChanged?.Invoke();
        }

        public void RemoveGameplayTags(GameplayTagContainer tags)
        {
            foreach (var tag in tags)
            {
                if (!tagReferenceCounts.TryGetValue(tag, out int count))
                {
                    continue;
                }

                count--;
                if (count <= 0)
                {
                    this.tags.Remove(tag);
                    tagReferenceCounts.Remove(tag);
                }
                else
                {
                    tagReferenceCounts[tag] = count;
                }
            }

            OnTagsChanged?.Invoke();
        }

        public bool HasAllTags(GameplayTagContainer tags)
        {
            return this.tags.All(tags);
        }

        public bool HasAnyTags(GameplayTagContainer tags)
        {
            return this.tags.Any(tags);
        }
    }
}
