using System;
using System.Runtime.CompilerServices;
using UnityEngine;

[Serializable]
public abstract class RPGSkill : Skill
{
    private UIntFlagContainer32 upgradeFlags;

    protected Entity entity;

    public event Action OnUpgradeChanged;

    public RPGSkill(SkillCoreData data)
        : base(data)
    {
    }

    protected override void StartUse(GameObject user, Arguments parameters = null)
    {
        entity = user.GetComponentInChildren<Entity>();
    }

    public override bool CanUse(GameObject user)
    {
        return base.CanUse(user) && IsUnlocked() && Helper.HasComponentInChildren<Entity>(user);
    }

    public bool IsUnlocked()
    {
        return HasUpgrade(SkillUpgradeCommonFlag.Default);
    }

    public virtual void SetUpgrades(UIntFlagContainer32 upgrades)
    {
        upgradeFlags = upgrades;
        OnUpgradeChanged?.Invoke();
    }

    public void AddUpgrade<T>(T upgrade) where T : unmanaged, Enum
    {
        uint v = Unsafe.As<T, uint>(ref upgrade);
        if (upgradeFlags.Has(v))
        {
            return;
        }

        upgradeFlags.Add(v);
        OnUpgradeChanged?.Invoke();
    }

    public void RemoveUpgrade<T>(T upgrade) where T : unmanaged, Enum
    {
        uint v = Unsafe.As<T, uint>(ref upgrade);
        if (!upgradeFlags.Has(v))
        {
            return;
        }

        upgradeFlags.Remove(v);
        OnUpgradeChanged?.Invoke();
    }

    public bool HasUpgrade<T>(T upgrade) where T : unmanaged, Enum
    {
        return upgradeFlags.Has(Unsafe.As<T, uint>(ref upgrade));
    }
}

public interface ICharagable
{
    int MaxCharge { get; }
    int CurrentCharge { get; }
    float ChargeProgress { get; }

    event Action OnChargeChanged;
}