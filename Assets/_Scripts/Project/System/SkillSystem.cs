using System;
using System.Collections.Generic;
using Unity.Collections.LowLevel.Unsafe;
using UnityEngine;

public class SkillSystem
{
    private Dictionary<UUID, UIntFlagContainer32> skillUpgradeFlags;

    public int SkillPoints { get; set; }
    public int UsedSkillPoints { get; private set; }
    public int AvailableSkillPoints => Math.Max(0, SkillPoints - UsedSkillPoints);

    public event Action<UUID> OnSkillUpgradeChanged;

    public SkillSystem()
    {
    }

    public SkillSystem(SkillSystemDTO dto)
    {
        SkillPoints = dto.skillPoints;
        skillUpgradeFlags = new();
        foreach (var pair in dto.skillUpgradeFlags)
        {
            skillUpgradeFlags[pair.Key] = pair.Value;
        }
        CalcUsedSkillPoints();
    }

    public void RegisterSkill(UUID skillId)
    {
        skillUpgradeFlags[skillId] = default;
    }

    public void UnregisterSkill(UUID skillId)
    {
        skillUpgradeFlags.Remove(skillId);
    }

    public bool IsSkillRegistered(UUID skillId)
    {
        return skillUpgradeFlags.ContainsKey(skillId);
    }

    private void CalcUsedSkillPoints()
    {
        UsedSkillPoints = 0;
        foreach (var pair in skillUpgradeFlags)
        {
            UsedSkillPoints += pair.Value.Count();
        }
    }

    public bool TryGetSkillUpgrade(UUID skillId, out UIntFlagContainer32 flags)
    {
        return skillUpgradeFlags.TryGetValue(skillId, out flags);
    }

    public UIntFlagContainer32 GetSkillUpgrade(UUID skillId)
    {
        if (!skillUpgradeFlags.TryGetValue(skillId, out UIntFlagContainer32 flags))
        {
            throw new KeyNotFoundException($"Skill upgrade flags for skill ID {skillId} not found.");
        }
        return flags;
    }

    public void AddUpgrade<T>(UUID skillId, T flag) where T : unmanaged, Enum
    {
        uint v = UnsafeUtility.As<T, uint>(ref flag);
        AddUpgrade(skillId, v);
    }

    public void AddUpgrade(UUID skillId, uint flagValue)
    {
        if (skillUpgradeFlags.TryGetValue(skillId, out var flags) == false)
        {
            throw new KeyNotFoundException($"Skill upgrade flags for skill ID {skillId} not found.");
        }

        flags.Add(flagValue);
        skillUpgradeFlags[skillId] = flags;
        CalcUsedSkillPoints();

        OnSkillUpgradeChanged?.Invoke(skillId);
    }

    public void RemoveUpgrade<T>(UUID skillId, T flag) where T : unmanaged, Enum
    {
        uint v = UnsafeUtility.As<T, uint>(ref flag);
        RemoveUpgrade(skillId, v);
    }

    public void RemoveUpgrade(UUID skillId, uint flagValue)
    {
        if (skillUpgradeFlags.TryGetValue(skillId, out var flags))
        {
            if (!flags.Has(flagValue))
            {
                return;
            }

            flags.Remove(flagValue);
            skillUpgradeFlags[skillId] = flags;
            CalcUsedSkillPoints();
            OnSkillUpgradeChanged?.Invoke(skillId);
        }
    }

    public SkillSystemDTO CaptureDTO()
    {
        var dto = new SkillSystemDTO
        {
            skillPoints = SkillPoints,
            skillUpgradeFlags = new SerializedDictionary<UUID, UIntFlagContainer32>()
        };

        foreach (var pair in skillUpgradeFlags)
        {
            dto.skillUpgradeFlags[pair.Key] = pair.Value;
        }

        return dto;
    }
}

[Serializable]
public class SkillSystemDTO
{
    public int skillPoints;
    public SerializedDictionary<UUID, UIntFlagContainer32> skillUpgradeFlags = new();
}