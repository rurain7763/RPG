using UnityEngine;

public class ReaperIdleState : AICharacterIdleState
{
    protected new Reaper character => base.character as Reaper;

    public ReaperIdleState(Reaper character)
        : base(character)
    {
    }

    public override void Execute()
    {
        idleTimer += Time.deltaTime;
        if (idleTimer >= IdleDuration)
        {
            ChangeToMoveState();
        }

        if (character.Target == null)
        {
            if (character.TryGetEnemyInVision(out Entity target))
            {
                character.Target = target;
                ChangeToChaseState();
            }
        }
        else
        {
            if (character.SkillSystem.CanUseSkill<Blink>(character.gameObject))
            {
                stateMachine.ChangeState<ReaperBlinkState>();
            }
            else if (character.SkillSystem.CanUseSkill<SpellDeathThunder>(character.gameObject))
            {
                stateMachine.ChangeState<ReaperSpellState>();
            }
            else if (character.IsEnemyInAttackRange(character.Target))
            {
                ChangeToAttackState();
            }
            else
            {
                ChangeToChaseState();
            }
        }
    }

    protected override void ChangeToChaseState()
    {
        stateMachine.ChangeState<ReaperChaseState>();
    }

    protected override void ChangeToMoveState()
    {
        stateMachine.ChangeState<ReaperMoveState>();
    }

    protected override void ChangeToAttackState()
    {
        stateMachine.ChangeState<ReaperAttackState>();
    }
}

public class ReaperMoveState : AICharacterMoveState
{
    public ReaperMoveState(Reaper character)
        : base(character)
    {
    }

    protected override void ChangeToIdleState()
    {
        stateMachine.ChangeState<ReaperIdleState>();
    }

    protected override void ChangeToChaseState()
    {
        stateMachine.ChangeState<ReaperChaseState>();
    }

    protected override void ChangeToAttackState()
    {
        stateMachine.ChangeState<ReaperAttackState>();
    }
}

public class ReaperJumpState : AICharacterJumpState
{
    public ReaperJumpState(Reaper character)
        : base(character)
    {
    }

    protected override void ChangeToIdleState()
    {
        stateMachine.ChangeState<ReaperIdleState>();
    }
}

public class ReaperFallState : AICharacterFallState
{
    public ReaperFallState(Reaper character)
        : base(character)
    {
    }

    protected override void ChangeToIdleState()
    {
        stateMachine.ChangeState<ReaperIdleState>();
    }
}

public class ReaperChaseState : AICharacterChaseState
{
    public ReaperChaseState(Reaper character)
        : base(character)
    {
    }

    protected override void ChangeToIdleState()
    {
        stateMachine.ChangeState<ReaperIdleState>();
    }

    protected override void ChangeToAttackState()
    {
        stateMachine.ChangeState<ReaperAttackState>();
    }
}

public class ReaperBlinkState : AICharacterState
{
    private static readonly int BlinkAnimHash = Animator.StringToHash("Blink");

    protected new Reaper character => base.character as Reaper;

    private Blink skill;

    public ReaperBlinkState(Reaper character)
        : base(character)
    {
    }

    public override void Enter()
    {
        skill = character.SkillSystem.GetSkill<Blink>();
        if (skill == null)
        {
            throw new System.Exception("ReaperBlinkState requires Blink skill.");
        }

        var blinkParams = new BlinkParameters(character.GetAvailableArea(), character.BodySize);

        character.AnimationEventReciever.OnEventTriggered += skill.HandleEvents;

        character.Animator.Play(BlinkAnimHash);
        skill.Use(character.gameObject, blinkParams);
        skill.SetToCooldown();
    }

    public override void Execute()
    {
        if (skill.IsComplete() && Helper.IsAnimationStateFinished(character.Animator, BlinkAnimHash))
        {
            stateMachine.ChangeState<ReaperIdleState>();
        }
    }

    public override void Exit()
    {
        skill.Cleanup();
        character.AnimationEventReciever.OnEventTriggered -= skill.HandleEvents;
    }
}

public class ReaperAttackState : AICharacterAttackState
{
    private const float StunDuration = 2.0f;

    protected new Reaper character => base.character as Reaper;

    public ReaperAttackState(Reaper character)
        : base(character)
    {
    }

    protected override void ChangeToIdleState()
    {
        stateMachine.ChangeState<ReaperIdleState>();
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
    }
}

public class ReaperSpellState : AICharacterState
{
    private static readonly int SpellAnimHash = Animator.StringToHash("Spell");

    protected new Reaper character => base.character as Reaper;

    private SpellDeathThunder skill;

    public ReaperSpellState(Reaper character)
        : base(character)
    {
    }

    public override void Enter()
    {
        skill = character.SkillSystem.GetSkill<SpellDeathThunder>();
        if (skill == null)
        {
            throw new System.Exception("ReaperSpellState requires SpellDeathThunder skill.");
        }

        character.AnimationEventReciever.OnEventTriggered += skill.HandleEvents;
        skill.Use(character.gameObject, new SpellDeathThunderParameters(character.Target));
        skill.SetToCooldown();
        character.Animator.Play(SpellAnimHash);
    }

    public override void Execute()
    {
        character.Animator.SetFloat(ActiveAnimSpeedFactorHash, character.StatSystem.ActiveSpeed.FinalValue);
        if (skill.IsComplete() && Helper.GetAnimationLoopCount(character.Animator, SpellAnimHash) == skill.MaxSpellCount)
        {
            stateMachine.ChangeState<ReaperIdleState>();
        }
    }

    public override void Exit()
    {
        skill.Cleanup();
        character.Animator.SetFloat(ActiveAnimSpeedFactorHash, 1f);
        character.AnimationEventReciever.OnEventTriggered -= skill.HandleEvents;
    }
}

public class ReaperStunnedState : AICharacterStunnedState
{
    public ReaperStunnedState(Reaper character)
        : base(character)
    {
    }

    protected override void ChangeToIdleState()
    {
        stateMachine.ChangeState<ReaperIdleState>();
    }
}

public class ReaperDeadState : AICharacterDeadState
{
    public ReaperDeadState(Reaper character)
        : base(character)
    {
    }

    public override void Enter()
    {
        base.Enter();

        character.IncludedLevel.PlayDefaultBGM();

        var hud = RPG.UISys.GetActiveStatic<HUDUI>();
        hud?.UnregisterHpDisplayer(character);
    }
}

