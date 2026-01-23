using System;

public class PlayerSkillCertificateArguments : Arguments
{
    public readonly Player Player;

    public PlayerSkillCertificateArguments(Player player)
    {
        Player = player;
    }
}

[Serializable]
public class PlayerSkillCertificate : ISkillCertificate
{
    private SkillSystem GetSkillSystemFromArguments(PlayerSkillCertificateArguments args)
    {
        var table = RPG.UserDataSys.GetTable<UserPlayDataTable>();
        if (table == null)
        {
            throw new InvalidOperationException("UserPlayDataTable not found");
        }
        return table.SkillSys;
    }

    public bool IsUnlocked(SkillTreeNode node, Arguments args)
    {
        if (args is not PlayerSkillCertificateArguments actual)
        {
            throw new ArgumentException("Invalid arguments", nameof(args));
        }

        var skillSystem = GetSkillSystemFromArguments(actual);

        if (node.SkillData is SkillCoreData core)
        {
            return skillSystem.IsSkillRegistered(core.ID);
        }
        else if (node.SkillData is RPGSkillUpgradeModuleData upgrade)
        {
            if (skillSystem.TryGetSkillUpgrade(upgrade.Core.ID, out UIntFlagContainer32 flags))
            {
                return flags.Has(upgrade.FlagValue);
            }
            return false;
        }

        throw new InvalidOperationException("Unknown skill data type");
    }

    public bool CanUnlock(SkillTreeNode node, Arguments args)
    {
        if (args is not PlayerSkillCertificateArguments actual)
        {
            throw new ArgumentException("Invalid arguments", nameof(args));
        }

        foreach (var parent in node.Parents)
        {
            // NOTE: check that parent is unlocked
            if (!IsUnlocked(parent, args))
            {
                return false;
            }

            // NOTE: check that only this node is unlocked among siblings
            foreach (var child in parent.Children)
            {
                if (child == node)
                {
                    continue;
                }

                if (IsUnlocked(child, args))
                {
                    return false;
                }
            }
        }

        return true;
    }

    public void Unlock(SkillTreeNode node, Arguments args)
    {
        if (args is not PlayerSkillCertificateArguments actual)
        {
            throw new ArgumentException("Invalid arguments", nameof(args));
        }

        var skillSystem = GetSkillSystemFromArguments(actual);

        if (node.SkillData is SkillCoreData core)
        {
            skillSystem.RegisterSkill(core.ID);
            skillSystem.AddUpgrade(core.ID, SkillUpgradeCommonFlag.Default);
        }
        else if (node.SkillData is RPGSkillUpgradeModuleData upgrade)
        {
            skillSystem.AddUpgrade(upgrade.Core.ID, upgrade.FlagValue);
        }
    }
}