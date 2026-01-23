using System;
using UnityEngine;

public class MageIdleState : AICharacterIdleState
{
    public MageIdleState(Mage character)
        : base(character)
    {
    }

    protected override void ChangeToChaseState()
    {
        stateMachine.ChangeState<MageChaseState>();
    }

    protected override void ChangeToMoveState()
    {
        stateMachine.ChangeState<MageMoveState>();
    }

    protected override void ChangeToAttackState()
    {
        stateMachine.ChangeState<MageAttackState>();
    }
}

public class MageMoveState : AICharacterMoveState
{
    public MageMoveState(Mage character)
        : base(character)
    {
    }

    protected override void ChangeToIdleState()
    {
        stateMachine.ChangeState<MageIdleState>();
    }

    protected override void ChangeToChaseState()
    {
        stateMachine.ChangeState<MageChaseState>();
    }

    protected override void ChangeToAttackState()
    {
        stateMachine.ChangeState<MageAttackState>();
    }
}

public class MageJumpState : AICharacterJumpState
{
    public MageJumpState(Mage character)
        : base(character)
    {
    }

    protected override void ChangeToIdleState()
    {
        stateMachine.ChangeState<MageIdleState>();
    }
}

public class MageFallState : AICharacterFallState
{
    public MageFallState(Mage character)
        : base(character)
    {
    }

    protected override void ChangeToIdleState()
    {
        stateMachine.ChangeState<MageIdleState>();
    }
}

public class MageChaseState : AICharacterChaseState
{
    public MageChaseState(Mage character)
        : base(character)
    {
    }

    protected override void ChangeToIdleState()
    {
        stateMachine.ChangeState<MageIdleState>();
    }

    protected override void ChangeToAttackState()
    {
        stateMachine.ChangeState<MageAttackState>();
    }
}

public class MageRetreatState : AICharacterState
{
    protected new Mage character => base.character as Mage;

    private Retreat skill;
    private Action nextStateAction;

    public MageRetreatState(Mage character)
        : base(character)
    {
    }

    public void SetNextState<T>() where T : AICharacterState
    {
        nextStateAction = () => stateMachine.ChangeState<T>();
    }

    public override void Enter()
    {
        skill = character.SkillSystem.GetSkill<Retreat>();
        if (skill == null)
        {
            throw new Exception("Retreat skill not found in Mage's SkillSystem.");
        }

        var retreatDirection = new Vector2(-character.FacingDirection, 0);
        skill.Use(character.gameObject, new RetreatParameters(retreatDirection));
    }

    public override void Execute()
    {
        if (skill.IsComplete())
        {
            if (nextStateAction != null)
            {
                nextStateAction.Invoke();
                nextStateAction = null;
            }
            else
            {
                stateMachine.ChangeState<MageIdleState>();
            }
        }
    }

    public override void Exit()
    {
        skill.Cleanup();
    }
}

public class MageMagicBallRainState : AICharacterState
{
    private static readonly int MagicBallRainAnimHash = Animator.StringToHash("MagicBallRain");

    protected new Mage character => base.character as Mage;

    private MagicBallRain skill;

    public MageMagicBallRainState(Mage character)
        : base(character)
    {
    }

    public override void Enter()
    {
        skill = character.SkillSystem.GetSkill<MagicBallRain>();
        if (skill == null)
        {
            throw new Exception("MagicBallRain skill not found in Mage's SkillSystem.");
        }

        character.AnimationEventReciever.OnEventTriggered += skill.HandleEvents;
        character.Animator.Play(MagicBallRainAnimHash);
        skill.Use(character.gameObject, new MagicBallRainParameters(character.Target));
    }

    public override void Execute()
    {
        if (skill.IsComplete())
        {
            stateMachine.ChangeState<MageIdleState>();
        }
    }

    public override void Exit()
    {
        skill.Cleanup();
        character.AnimationEventReciever.OnEventTriggered -= skill.HandleEvents;
    }
}

public class MageAttackState : AICharacterAttackState
{
    private const float StunDuration = 2.0f;
    private const int MaxTryCount = 3;

    protected new Mage character => base.character as Mage;

    private int tryCount = 0;

    public MageAttackState(Mage character)
        : base(character)
    {
    }

    protected override void ChangeToIdleState()
    {
        stateMachine.ChangeState<MageIdleState>();
    }

    protected override void OnHitEvent()
    {
        int hitCount = 0;

        var targets = character.TargetDetector.DetectTargets();
        foreach (var target in targets)
        {
            var entity = target.GetComponent<Entity>();
            if (entity == null || entity is not ICombatable combatable)
            {
                continue;
            }

            if (combatable.CombatSystem.IsDead)
            {
                continue;
            }

            if (combatable.CombatSystem.ActiveCounter)
            {
                character.CombatSystem.Stun(StunDuration);
                return;
            }

            if (combatable.CombatSystem.ChanceToEvasion(combatable.StatSystem.TotalEvasion.FinalValue))
            {
                continue;
            }

            var damage = RPG.CalcDamage(character, combatable);
            var buff = RPG.CalcBuffByDamage(character, combatable, damage);

            combatable.CombatSystem.TakeDamage(damage);
            combatable.BuffSystem.AddBuff(buff);

            if (damage.IsCritical)
            {
                character.VFXSystem.SpawnCriticalHitVFX(entity);
            }
            else
            {
                character.VFXSystem.SpawnHitVFX(entity);
            }

            character.SFXSystem.PlayHitSFX();

            hitCount++;
        }

        if (hitCount == 0)
        {
            character.SFXSystem.PlayMissSFX();
        }

        if (++tryCount >= MaxTryCount)
        {
            if (character.SkillSystem.CanUseSkill<Retreat>(character.gameObject))
            {
                character.LookAt(character.Target.CenterPosition);
                var retreatState = stateMachine.GetState<MageRetreatState>();
                retreatState.SetNextState<MageMagicBallRainState>();
                stateMachine.ChangeState<MageRetreatState>();
            }
            
            tryCount = 0;
        }
    }
}

public class MageKnockbackState : AICharacterKnockbackState
{
    public MageKnockbackState(Mage character)
        : base(character)
    {
    }

    protected override void ChangeToIdleState()
    {
        stateMachine.ChangeState<MageIdleState>();
    }
}

public class MageStunnedState : AICharacterStunnedState
{
    public MageStunnedState(Mage character)
        : base(character)
    {
    }

    protected override void ChangeToIdleState()
    {
        stateMachine.ChangeState<MageIdleState>();
    }
}

public class MageDeadState : AICharacterDeadState
{
    public MageDeadState(Mage character)
        : base(character)
    {
    }
}

