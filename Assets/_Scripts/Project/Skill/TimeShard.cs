using System;
using UnityEngine;

[Serializable]
public class TimeShard : RPGSkill, ICharagable
{
    class TeleportSequence : SkillSequence
    {
        const float MaxRemainTime = 5f;

        private TimeShard skill;
        private Shard shard;
        private float remainingTime;

        public TeleportSequence(TimeShard skill, Shard shard)
        {
            this.skill = skill;
            this.shard = shard;
        }

        public override void Use(GameObject user, Arguments parameters = null)
        {
            if (user != skill.entity.gameObject)
            {
                throw new InvalidOperationException("Different user when first use something is wrong.");
            }

            Vector2 shardPosition = shard.GroundPosition;
            Vector2 userPosition = skill.entity.CenterPosition;

            skill.entity.transform.position = shardPosition;
            shard.transform.position = userPosition;

            if (skill.HasUpgrade(SkillUpgradeFlag.TimeShard_TeleportHpRewind))
            {
                if (skill.entity is ICombatable combatable)
                {
                    var healing = new Healing(combatable.CombatSystem, combatable.CombatSystem)
                    {
                        Amount = combatable.StatSystem.TotalHealth.FinalValue * 0.3f
                    };

                    combatable.CombatSystem.TakeHeal(healing);
                }
            }

            shard.ExplodeManually(1);
            skill.NextSequence();
        }

        public override void Begin()
        {
            remainingTime = MaxRemainTime;
        }

        public override void Tick(float delta)
        {
            remainingTime -= delta;
            if (remainingTime <= 0)
            {
                shard.ExplodeManually();
                skill.NextSequence();
            }
        }
    }

    const int UpgradeMuticastCount = 3;
    const float ChargeInterval = 5.0f;

    public new TimeShardData Data => base.Data as TimeShardData;

    private float chargeTimer;

    public int MaxCharge { get; private set; }
    public int CurrentCharge { get; private set; }
    public float ChargeProgress => CurrentCharge < MaxCharge ? Mathf.Clamp01(1.0f - chargeTimer / ChargeInterval) : 1.0f;

    public event Action OnChargeChanged;

    public TimeShard(TimeShardData data) 
        : base(data)
    {
        MaxCharge = Data.BaseChargeCount;

        CurrentCharge = MaxCharge;
        chargeTimer = ChargeInterval;

        OnUpgradeChanged += () =>
        {
            if (HasUpgrade(SkillUpgradeFlag.TimeShard_Multicast))
            {
                MaxCharge = UpgradeMuticastCount;
                CurrentCharge = UpgradeMuticastCount;
                OnChargeChanged?.Invoke();
            }

            if (HasUpgrade(SkillUpgradeFlag.TimeShard_Teleport))
            {
                MaxCharge = 1;
                CurrentCharge = 1;
                OnChargeChanged?.Invoke();
            }
        };
    }

    protected override void StartUse(GameObject user, Arguments parameters)
    {
        base.StartUse(user, parameters);

        var shard = GameObject.Instantiate(Data.ShardPrefab, entity.CenterPosition, Quaternion.identity);
        shard.Owner = entity;

        if (HasUpgrade(SkillUpgradeFlag.TimeShard_MoveToEnemey))
        {
            shard.MoveToClosestEnemy = true;
        }

        if (HasUpgrade(SkillUpgradeFlag.TimeShard_Teleport))
        {
            shard.AutoExplode = false;
            AddSequence(new TeleportSequence(this, shard));
            StartSequences();
        }

        --CurrentCharge;

        OnChargeChanged?.Invoke();
    }

    public override void Tick(float delta)
    {
        base.Tick(delta);

        HandleCharge(delta);
    }

    private void HandleCharge(float delta)
    {
        if (IsInSequence())
        {
            return;
        }

        if (CurrentCharge >= MaxCharge)
        {
            return;
        }

        chargeTimer -= delta;
        if (chargeTimer < 0)
        {
            ++CurrentCharge;
            chargeTimer += ChargeInterval;
        }

        OnChargeChanged?.Invoke();
    }

    public override bool CanUse(GameObject user)
    {
        if (IsInSequence())
        {
            return true;
        }

        if (IsOnCooldown || CurrentCharge <= 0)
        {
            return false;
        }

        return true;
    }
}