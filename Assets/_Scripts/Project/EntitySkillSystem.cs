using System.Collections.Generic;
using UnityEngine;

public class EntitySkillSystem : MonoBehaviour
{
    [SerializeField] private SkillCoreData[] skills;

    private SkillSystem linkedSkillSystem;
    private List<RPGSkill> skillInstances = new();

    public SkillSystem LinkedSkillSystem
    {
        get => linkedSkillSystem;
        set
        {
            var prev = linkedSkillSystem;
            var next = value;

            if (prev != null)
            {
                prev.OnSkillUpgradeChanged -= HandleSkillUpgradeChanged;
            }

            if (next != null)
            {
                next.OnSkillUpgradeChanged += HandleSkillUpgradeChanged;
            }

            linkedSkillSystem = next;
        }
    }

    public IReadOnlyList<RPGSkill> Skills => skillInstances;

    public void Begin()
    {
        foreach (var skill in skills)
        {
            var instance = skill.CreateSkill() as RPGSkill;
            skillInstances.Add(instance);
        }

        if (LinkedSkillSystem != null)
        {
            foreach (var instance in skillInstances)
            {
                if (LinkedSkillSystem.TryGetSkillUpgrade(instance.Data.ID, out var flags))
                {
                    instance.SetUpgrades(flags);
                }
            }
        }
    }

    private void HandleSkillUpgradeChanged(UUID skillId)
    {
        if (TryGetSkillByID(skillId, out var skill))
        {
            var flags = LinkedSkillSystem.GetSkillUpgrade(skillId);
            skill.SetUpgrades(flags);
        }
    }

    public void Tick(float delta)
    {
        foreach (var instance in skillInstances)
        {
            instance.Tick(delta);
        }
    }

    public void End()
    {
        LinkedSkillSystem = null;
        skillInstances.Clear();
    }

    public RPGSkill GetSkillByIndex(int index)
    {
        if (index < 0 || index >= skillInstances.Count)
        {
            Debug.LogWarning($"Skill index {index} is out of range.");
            return null;
        }

        return skillInstances[index];
    }

    public bool TryGetSkillByIndex(int index, out RPGSkill skill)
    {
        skill = GetSkillByIndex(index);
        return skill != null;
    }

    public bool HasSkill<T>() where T : RPGSkill
    {
        return skillInstances.Exists(skill => skill is T);
    }

    public T GetSkill<T>() where T : RPGSkill
    {
        return skillInstances.Find(skill => skill is T) as T;
    }

    public bool TryGetSkill<T>(out T skill) where T : RPGSkill
    {
        skill = GetSkill<T>();
        return skill != null;
    }

    public bool TryGetSkillByID(UUID skillId, out RPGSkill skill)
    {
        skill = skillInstances.Find(s => s.Data.ID == skillId);
        return skill != null;
    }

    public bool CanUseSkill<T>(GameObject user) where T : RPGSkill
    {
        if (TryGetSkill(out T skill))
        {
            return skill.CanUse(user);
        }
        return false;
    }
}