using System;
using System.Collections.Generic;
using UnityEngine;

public class EntityStatSystem : MonoBehaviour
{
    [Serializable]
    struct StatEntry
    {
        public StatData StatData;
        public float BaseValue;
    }

    [Serializable]
    struct CombinedStatEntry
    {
        public StatData StatData;
    }

    [SerializeField] private StatEntry healthEntry;
    [SerializeField] private StatEntry healthRegenEntry;
    [SerializeField] private StatEntry strengthEntry;
    [SerializeField] private StatEntry agilityEntry;
    [SerializeField] private StatEntry intelligenceEntry;
    [SerializeField] private StatEntry vitalityEntry;

    [SerializeField] private StatEntry damageEntry;
    [SerializeField] private StatEntry critPowerEntry;
    [SerializeField] private StatEntry critRateEntry;
    [SerializeField] private StatEntry armorReductionEntry;

    [SerializeField] private StatEntry fireDamageEntry;
    [SerializeField] private StatEntry iceDamageEntry;
    [SerializeField] private StatEntry lightningDamageEntry;

    [SerializeField] private StatEntry armorEntry;
    [SerializeField] private StatEntry evasionEntry;

    [SerializeField] private StatEntry fireResistanceEntry;
    [SerializeField] private StatEntry iceResistanceEntry;
    [SerializeField] private StatEntry lightningResistanceEntry;

    [SerializeField] private StatEntry moveSpeedEntry;
    [SerializeField] private StatEntry attackSpeedEntry;

    [SerializeField] private CombinedStatEntry totalHealthEntry;
    [SerializeField] private CombinedStatEntry physicalDamageEntry;
    [SerializeField] private CombinedStatEntry elementalDamageEntry;
    [SerializeField] private CombinedStatEntry totalCriticalRateEntry;
    [SerializeField] private CombinedStatEntry totalCriticalPowerEntry;
    [SerializeField] private CombinedStatEntry totalEvasionEntry;
    [SerializeField] private CombinedStatEntry totalArmorEntry;
    [SerializeField] private CombinedStatEntry totalIceResistanceEntry;
    [SerializeField] private CombinedStatEntry totalFireResistanceEntry;
    [SerializeField] private CombinedStatEntry totalLightningResistanceEntry;

    private Dictionary<StatData, IStat> stats = new();

    public Stat Health => stats[healthEntry.StatData] as Stat;
    public Stat HealthRegeneration => stats[healthRegenEntry.StatData] as Stat;
    public Stat Strength => stats[strengthEntry.StatData] as Stat;
    public Stat Agility => stats[agilityEntry.StatData] as Stat;
    public Stat Intelligence => stats[intelligenceEntry.StatData] as Stat;
    public Stat Vitality => stats[vitalityEntry.StatData] as Stat;

    public Stat Damage => stats[damageEntry.StatData] as Stat;
    public Stat CritPower => stats[critPowerEntry.StatData] as Stat;
    public Stat CritRate => stats[critRateEntry.StatData] as Stat;
    public Stat ArmorReduction => stats[armorReductionEntry.StatData] as Stat;

    public Stat FireDamage => stats[fireDamageEntry.StatData] as Stat;
    public Stat IceDamage => stats[iceDamageEntry.StatData] as Stat;
    public Stat LightningDamage => stats[lightningDamageEntry.StatData] as Stat;

    public Stat Armor => stats[armorEntry.StatData] as Stat;
    public Stat Evasion => stats[evasionEntry.StatData] as Stat;

    public Stat FireResistance => stats[fireResistanceEntry.StatData] as Stat;
    public Stat IceResistance => stats[iceResistanceEntry.StatData] as Stat;
    public Stat LightningResistance => stats[lightningResistanceEntry.StatData] as Stat;

    public Stat MoveSpeed => stats[moveSpeedEntry.StatData] as Stat;
    public Stat AttackSpeed => stats[attackSpeedEntry.StatData] as Stat;

    public CombinedStat TotalHealth => stats[totalHealthEntry.StatData] as CombinedStat;
    public CombinedStat PhysicalDamage => stats[physicalDamageEntry.StatData] as CombinedStat;
    public CombinedStat ElementalDamage => stats[elementalDamageEntry.StatData] as CombinedStat;
    public CombinedStat TotalCriticalRate => stats[totalCriticalRateEntry.StatData] as CombinedStat;
    public CombinedStat TotalCriticalPower => stats[totalCriticalPowerEntry.StatData] as CombinedStat;
    public CombinedStat TotalEvasion => stats[totalEvasionEntry.StatData] as CombinedStat;
    public CombinedStat TotalArmor => stats[totalArmorEntry.StatData] as CombinedStat;
    public CombinedStat TotalIceResistance => stats[totalIceResistanceEntry.StatData] as CombinedStat;
    public CombinedStat TotalFireResistance => stats[totalFireResistanceEntry.StatData] as CombinedStat;
    public CombinedStat TotalLightningResistance => stats[totalLightningResistanceEntry.StatData] as CombinedStat;

    private void Awake()
    {
        stats[healthEntry.StatData] = new Stat(healthEntry.StatData, healthEntry.BaseValue);
        stats[healthRegenEntry.StatData] = new Stat(healthRegenEntry.StatData, healthRegenEntry.BaseValue);
        stats[strengthEntry.StatData] = new Stat(strengthEntry.StatData, strengthEntry.BaseValue);
        stats[agilityEntry.StatData] = new Stat(agilityEntry.StatData, agilityEntry.BaseValue);
        stats[intelligenceEntry.StatData] = new Stat(intelligenceEntry.StatData, intelligenceEntry.BaseValue);
        stats[vitalityEntry.StatData] = new Stat(vitalityEntry.StatData, vitalityEntry.BaseValue);
        stats[damageEntry.StatData] = new Stat(damageEntry.StatData, damageEntry.BaseValue);
        stats[critPowerEntry.StatData] = new Stat(critPowerEntry.StatData, critPowerEntry.BaseValue);
        stats[critRateEntry.StatData] = new Stat(critRateEntry.StatData, critRateEntry.BaseValue);
        stats[armorReductionEntry.StatData] = new Stat(armorReductionEntry.StatData, armorReductionEntry.BaseValue);
        stats[fireDamageEntry.StatData] = new Stat(fireDamageEntry.StatData, fireDamageEntry.BaseValue);
        stats[iceDamageEntry.StatData] = new Stat(iceDamageEntry.StatData, iceDamageEntry.BaseValue);
        stats[lightningDamageEntry.StatData] = new Stat(lightningDamageEntry.StatData, lightningDamageEntry.BaseValue);
        stats[armorEntry.StatData] = new Stat(armorEntry.StatData, armorEntry.BaseValue);
        stats[evasionEntry.StatData] = new Stat(evasionEntry.StatData, evasionEntry.BaseValue);
        stats[fireResistanceEntry.StatData] = new Stat(fireResistanceEntry.StatData, fireResistanceEntry.BaseValue);
        stats[iceResistanceEntry.StatData] = new Stat(iceResistanceEntry.StatData, iceResistanceEntry.BaseValue);
        stats[lightningResistanceEntry.StatData] = new Stat(lightningResistanceEntry.StatData, lightningResistanceEntry.BaseValue);
        stats[moveSpeedEntry.StatData] = new Stat(moveSpeedEntry.StatData, moveSpeedEntry.BaseValue);
        stats[attackSpeedEntry.StatData] = new Stat(attackSpeedEntry.StatData, attackSpeedEntry.BaseValue);
        
        var totalHealth = new CombinedStat(totalHealthEntry.StatData, Health, Vitality);
        totalHealth.SetCalculationFunc((statList) =>
        {
            float baseMaxHealth = statList[0].FinalValue;
            float vitalityBonus = statList[1].FinalValue * 10;
            return baseMaxHealth + vitalityBonus;
        });

        stats[totalHealthEntry.StatData] = totalHealth;

        var physicalDamage = new CombinedStat(physicalDamageEntry.StatData, Damage, Strength);
        physicalDamage.SetCalculationFunc((statList) =>
        {
            float baseDamage = statList[0].FinalValue;
            float strengthBonus = statList[1].FinalValue;
            return baseDamage + strengthBonus;
        });

        stats[physicalDamageEntry.StatData] = physicalDamage;

        var elementalDamage = new CombinedStat(elementalDamageEntry.StatData, FireDamage, IceDamage, LightningDamage, Intelligence);
        elementalDamage.SetCalculationFunc((statList) =>
        {
            float fireDamage = statList[0].FinalValue;
            float iceDamage = statList[1].FinalValue;
            float lightningDamage = statList[2].FinalValue;
            float intelligence = statList[3].FinalValue;
            float maxElementalDamage = Mathf.Max(fireDamage, Mathf.Max(iceDamage, lightningDamage));
            if (maxElementalDamage <= 0f)
            {
                return 0f;
            }
            maxElementalDamage += (maxElementalDamage * 0.5f) + intelligence;
            return maxElementalDamage;
        });

        stats[elementalDamageEntry.StatData] = elementalDamage;

        var totalCriticalRate = new CombinedStat(totalCriticalRateEntry.StatData, CritRate, Agility);
        totalCriticalRate.SetCalculationFunc((statList) =>
        {
            float baseCritRate = statList[0].FinalValue;
            float agilityBonus = statList[1].FinalValue * 0.3f;
            return baseCritRate + agilityBonus;
        });

        stats[totalCriticalRateEntry.StatData] = totalCriticalRate;

        var totalCriticalPower = new CombinedStat(totalCriticalPowerEntry.StatData, CritPower, Strength);
        totalCriticalPower.SetCalculationFunc((statList) =>
        {
            float baseCritPower = statList[0].FinalValue;
            float strengthBonus = statList[1].FinalValue * 0.5f;
            return baseCritPower + strengthBonus;
        });

        stats[totalCriticalPowerEntry.StatData] = totalCriticalPower;

        var totalEvasion = new CombinedStat(totalCriticalRateEntry.StatData, Evasion, Agility);
        totalEvasion.SetCalculationFunc((statList) =>
        {
            float baseEvasion = statList[0].FinalValue;
            float agilityBonus = statList[1].FinalValue * 0.5f;
            return baseEvasion + agilityBonus;
        });

        stats[totalEvasionEntry.StatData] = totalEvasion;

        var totalArmor = new CombinedStat(totalArmorEntry.StatData, Armor, Vitality);
        totalArmor.SetCalculationFunc((statList) =>
        {
            float baseArmor = statList[0].FinalValue;
            float vitalityBonus = statList[1].FinalValue;
            return baseArmor + vitalityBonus;
        });

        stats[totalArmorEntry.StatData] = totalArmor;

        var totalIceResistance = new CombinedStat(totalIceResistanceEntry.StatData, IceResistance, Intelligence);
        totalIceResistance.SetCalculationFunc((statList) =>
        {
            float baseResistance = statList[0].FinalValue;
            float intelligenceBonus = statList[1].FinalValue * 0.5f;
            return baseResistance + intelligenceBonus;
        });

        stats[totalIceResistanceEntry.StatData] = totalIceResistance;

        var totalFireResistance = new CombinedStat(totalFireResistanceEntry.StatData, FireResistance, Intelligence);
        totalFireResistance.SetCalculationFunc((statList) =>
        {
            float baseResistance = statList[0].FinalValue;
            float intelligenceBonus = statList[1].FinalValue * 0.5f;
            return baseResistance + intelligenceBonus;
        });

        stats[totalFireResistanceEntry.StatData] = totalFireResistance;

        var totalLightningResistance = new CombinedStat(totalLightningResistanceEntry.StatData, LightningResistance, Intelligence);
        totalLightningResistance.SetCalculationFunc((statList) =>
        {
            float baseResistance = statList[0].FinalValue;
            float intelligenceBonus = statList[1].FinalValue * 0.5f;
            return baseResistance + intelligenceBonus;
        });

        stats[totalLightningResistanceEntry.StatData] = totalLightningResistance;
    }

    public T GetStat<T>(StatData statData) where T : class, IStat
    {
        if (stats.TryGetValue(statData, out IStat stat))
        {
            return stat as T;
        }

        return null;
    }
}