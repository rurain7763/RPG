using UnityEngine;

public abstract class ItemEffect
{
    public abstract void ApplyEffect(Entity entity);
    public virtual void RemoveEffect() { }
}

public class SpawnPortalEffect : ItemEffect
{
    public override void ApplyEffect(Entity entity)
    {
        Player player = entity as Player;
        if (player == null || !player.IsLocalPlayer())
        {
            Logger.Warn("SpawnPortalEffect can only be applied to Player entities.");
            return;
        }

        var currentLevel = RPG.LevelSys.CurrentLevel as RPGLevel;
        RPG.UserDataSys.PlayData.Portal.LaunchPortal(currentLevel.LevelID, player.transform.position + new Vector3(0.75f, 0, 0) * player.FacingDirection, !player.IsFacingRight);
    }
}

public class HealItemEffect : ItemEffect
{
    private float healRate;

    public HealItemEffect(float healRate)
    {
        this.healRate = healRate;
    }

    public override void ApplyEffect(Entity entity)
    {
        if (entity is not ICombatable combatable)
        {
            return;
        }

        Healing healing = new Healing(combatable.CombatSystem, combatable.CombatSystem)
        {
            Amount = combatable.StatSystem.TotalHealth.FinalValue * healRate
        };

        combatable.CombatSystem.TakeHeal(healing);
    }
}

public class ApplyBuffItemEffect : ItemEffect
{
    private RPGBuffData buffData;

    public ApplyBuffItemEffect(RPGBuffData buffData)
    {
        this.buffData = buffData;
    }

    public override void ApplyEffect(Entity entity)
    {
        if (entity is not ICombatable combatable)
        {
            return;
        }
        var buff = buffData.CreateBuff(combatable);
        combatable.BuffSystem.AddBuff(buff);
    }
}

public class IceBlastItemEffect : ItemEffect
{
    private readonly float healthPercentageToTrigger;
    private readonly float reflectDamageRate;
    private readonly float cooldown;
    private readonly VFXID vfxID;

    private Entity owner;

    private float lastTriggerTime;

    public IceBlastItemEffect(float healthPercentageToTrigger, float reflectDamageRate, float cooldown, VFXID vfxID)
    {
        this.healthPercentageToTrigger = healthPercentageToTrigger;
        this.reflectDamageRate = reflectDamageRate;
        this.cooldown = cooldown;
        this.vfxID = vfxID;
        lastTriggerTime = -cooldown;
    }

    public override void ApplyEffect(Entity entity)
    {
        if (entity is not ICombatable combatable)
        {
            return;
        }

        combatable.CombatSystem.OnTakeDamage += HandleTakeDamage;

        owner = entity;
    }

    public override void RemoveEffect()
    {
        if (owner is not ICombatable combatable)
        {
            return;
        }

        combatable.CombatSystem.OnTakeDamage -= HandleTakeDamage;
    }

    private void HandleTakeDamage(Damage damage)
    {
        if (lastTriggerTime + cooldown > Time.time)
        {
            return;
        }

        var combatable = owner as ICombatable;

        float currentHealthPercentage = combatable.CombatSystem.CurrentHealth / combatable.CombatSystem.MaxHealth;
        if (currentHealthPercentage > healthPercentageToTrigger)
        {
            return;
        }

        var vfx = RPG.VFXSys.SpawnVFX(Local.GetVFXPath(vfxID));
        vfx.transform.position = owner.CenterPosition;

        if (damage.Source is ICombatable damageSourceCombatable)
        {
            var blastDamage = new Damage(combatable.CombatSystem, damageSourceCombatable.CombatSystem)
            {
                PhysicalAmount = 0,
                ElementalAmount = damage.TotalAmount * reflectDamageRate,
                ElementType = ElementType.Ice,
                IsCritical = false
            };

            damageSourceCombatable.CombatSystem.TakeDamage(blastDamage);
        }

        lastTriggerTime = Time.time;
    }
}

public class DrainDamageItemEffect : ItemEffect
{
    private readonly float drainRate;

    private Entity owner;

    public DrainDamageItemEffect(float drainRate)
    {
        this.drainRate = drainRate;
    }

    public override void ApplyEffect(Entity entity)
    {
        if (entity is not ICombatable combatable)
        {
            return;
        }

        combatable.CombatSystem.OnDealDamage += HandleDealDamage;

        owner = entity;
    }

    public override void RemoveEffect()
    {
        if (owner is not ICombatable combatable)
        {
            return;
        }

        combatable.CombatSystem.OnDealDamage -= HandleDealDamage;
    }

    private void HandleDealDamage(Damage damage)
    {
        if (damage.TotalAmount <= 0)
        {
            return;
        }

        var combatable = owner as ICombatable;        
        var healing = new Healing(combatable.CombatSystem, combatable.CombatSystem)
        {
            Amount = damage.TotalAmount * drainRate
        };

        combatable.CombatSystem.TakeHeal(healing);
    }
}