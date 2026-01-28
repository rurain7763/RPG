using System;
using UnityEngine;

public abstract class RPGBuff : Buff
{
    public new ICombatable Owner { get => base.Owner as ICombatable; set => base.Owner = value; }
    public new ICombatable Source { get => base.Source as ICombatable; set => base.Source = value; }

    public RPGBuff(BuffID id, BuffCategory category, float duration, int stackCount, ICombatable source) 
        : base((uint)id, category, duration, stackCount, source)
    {
    }
}

public class Frozen : RPGBuff
{
    private AddValueModifier activeSpeedModifier;

    public Frozen(float duration, ICombatable source) 
        : base(BuffID.Frozen, BuffCategory.Negative, duration, 1, source)
    {
        activeSpeedModifier = new AddValueModifier(-0.23f);
    }

    public override void OnApply()
    {
        Owner.StatSystem.ActiveSpeed.AddModifier(activeSpeedModifier);
    }

    public override void OnExpire()
    {
        Owner.StatSystem.ActiveSpeed.RemoveModifier(activeSpeedModifier);
    }

    public override bool OnDuplicate(Buff newBuff)
    {
        Duration = Mathf.Max(Duration, newBuff.Duration);

        return true;
    }
}

public class Burning : RPGBuff
{
    private const float DamagePerSecond = 0.5f;

    private float damageTickRate;
    private float lastTimeDamageApplied;

    public Burning(float duration, float damageTickRate, ICombatable source) 
        : base(BuffID.Burning, BuffCategory.Negative, duration, 1, source)
    {
        this.damageTickRate = damageTickRate;
    }

    public override void OnApply()
    {
        lastTimeDamageApplied = Time.time;
    }

    public override void OnTick()
    {
        if (Time.time - lastTimeDamageApplied >= DamagePerSecond)
        {
            var damage = new Damage(Owner.CombatSystem)
            {
                PhysicalAmount = 0,
                IsCritical = false,
                ElementalAmount = Owner.StatSystem.TotalHealth.FinalValue * damageTickRate,
                ElementType = ElementType.Fire,
            };

            Owner.CombatSystem.TakeDamage(damage);
            lastTimeDamageApplied = Time.time;
        }
    }

    public override bool OnDuplicate(Buff newBuff)
    {
        Burning other = newBuff as Burning;

        // NOTE: We take the highest duration and tick rate
        Duration = Mathf.Max(Duration, other.Duration);
        damageTickRate = Mathf.Max(damageTickRate, other.damageTickRate);

        return true;
    }
}

public class Electrified : RPGBuff
{
    const float StunDuration = 1.5f;

    private int maxChargeCount;

    public Electrified(float duration, int maxChargeCount, ICombatable source) 
        : base(BuffID.Electrified, BuffCategory.Negative, duration, 1, source)
    {
        this.maxChargeCount = maxChargeCount;
    }

    public override bool OnDuplicate(Buff newBuff)
    {
        if (StackCount == maxChargeCount)
        {
            return false;
        }

        Electrified other = newBuff as Electrified;

        Duration = Mathf.Max(Duration, other.Duration);
        maxChargeCount = Mathf.Max(maxChargeCount, other.maxChargeCount);

        StackCount += other.StackCount;
        if (StackCount >= maxChargeCount)
        {
            Owner.CombatSystem.Stun(StunDuration);

            if (Owner is Entity entity)
            {
                VFX vfx = RPG.VFXSys.SpawnVFX(Local.GetVFXPath(VFXID.LightningStrike));
                vfx.transform.position = entity.transform.position;
            }

            Duration = 0f;
        }

        return true;
    }
}

public class Slow : RPGBuff
{
    private AddValueModifier moveSpeedModifier;

    private float slowRate;

    public Slow(float duration, float slowRate, ICombatable source) 
        : base(BuffID.Slow, BuffCategory.Negative, duration, 1, source)
    {
        this.slowRate = slowRate;
        moveSpeedModifier = new AddValueModifier(-slowRate);
    }

    public override void OnApply()
    {
        Owner.StatSystem.MoveSpeed.AddModifier(moveSpeedModifier);
    }

    public override void OnExpire()
    {
        if (Owner is not ICombatable combatable)
        {
            return;
        }

        Owner.StatSystem.MoveSpeed.RemoveModifier(moveSpeedModifier);
    }

    public override bool OnDuplicate(Buff newBuff)
    {
        var other = newBuff as Slow;

        Duration = Mathf.Max(Duration, newBuff.Duration);

        if (Owner is not ICombatable combatable)
        {
            return true;
        }

        if (other.slowRate > slowRate)
        {
            combatable.StatSystem.MoveSpeed.RemoveModifier(moveSpeedModifier);
            slowRate = other.slowRate;
            moveSpeedModifier = new AddValueModifier(-slowRate);
            combatable.StatSystem.MoveSpeed.AddModifier(moveSpeedModifier);
        }

        return true;
    }
}

public class IncreaseDamage : RPGBuff
{
    private float damageIncreaseRate;

    private TotalMultValueModifier damageModifier;

    public IncreaseDamage(float duration, float damageIncreaseRate, ICombatable source)
        : base(BuffID.IncreaseDamage, BuffCategory.Positive, duration, 1, source)
    {
        this.damageIncreaseRate = damageIncreaseRate;
    }

    public override void OnApply()
    {
        damageModifier = new TotalMultValueModifier(damageIncreaseRate);
        Owner.StatSystem.Damage.AddModifier(damageModifier);
    }

    public override void OnExpire()
    {
        Owner.StatSystem.Damage.RemoveModifier(damageModifier);
    }

    public override bool OnDuplicate(Buff newBuff)
    {
        if (Owner is not ICombatable combatable)
        {
            return true;
        }

        var other = newBuff as IncreaseDamage;

        Duration = Mathf.Max(Duration, newBuff.Duration);
        if (other.damageIncreaseRate > damageIncreaseRate)
        {
            combatable.StatSystem.Damage.RemoveModifier(damageModifier);
            damageIncreaseRate = other.damageIncreaseRate;
            damageModifier = new TotalMultValueModifier(damageIncreaseRate);
            combatable.StatSystem.Damage.AddModifier(damageModifier);
        }

        return true;
    }
}

public class IncreaseAttackSpeed : RPGBuff
{
    private float attackSpeedIncreaseRate;

    private TotalMultValueModifier attackSpeedModifier;

    public IncreaseAttackSpeed()
        : base(BuffID.IncreaseAttackSpeed, BuffCategory.Positive, -1f, 1, null)
    {
    }

    public IncreaseAttackSpeed(float duration, float attackSpeedIncreaseRate, ICombatable source)
        : base(BuffID.IncreaseAttackSpeed, BuffCategory.Positive, duration, 1, source)
    {
        this.attackSpeedIncreaseRate = attackSpeedIncreaseRate;
    }

    public override void OnApply()
    {
        attackSpeedModifier = new TotalMultValueModifier(attackSpeedIncreaseRate);
        Owner.StatSystem.AttackSpeed.AddModifier(attackSpeedModifier);
    }

    public override void OnExpire()
    {
        Owner.StatSystem.AttackSpeed.RemoveModifier(attackSpeedModifier);
    }

    public override bool OnDuplicate(Buff newBuff)
    {
        var other = newBuff as IncreaseAttackSpeed;
        Duration = Mathf.Max(Duration, newBuff.Duration);
        if (other.attackSpeedIncreaseRate > attackSpeedIncreaseRate)
        {
            Owner.StatSystem.AttackSpeed.RemoveModifier(attackSpeedModifier);
            attackSpeedIncreaseRate = other.attackSpeedIncreaseRate;
            attackSpeedModifier = new TotalMultValueModifier(attackSpeedIncreaseRate);
            Owner.StatSystem.AttackSpeed.AddModifier(attackSpeedModifier);
        }
        return true;
    }
}