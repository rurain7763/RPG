public class SkeletonKnightIdleState : AICharacterIdleState
{
    public SkeletonKnightIdleState(SkeletonKnight skeleton) 
        : base(skeleton)
    {
    }

    protected override void ChangeToChaseState()
    {
        stateMachine.ChangeState<SkeletonKnightChaseState>();
    }

    protected override void ChangeToMoveState()
    {
        stateMachine.ChangeState<SkeletonKnightMoveState>();
    }

    protected override void ChangeToAttackState()
    {
        stateMachine.ChangeState<SkeletonKnightAttackState>();
    }
}

public class SkeletonKnightMoveState : AICharacterMoveState
{
    public SkeletonKnightMoveState(SkeletonKnight skeleton) 
        : base(skeleton) 
    {
    }

    protected override void ChangeToIdleState()
    {
        stateMachine.ChangeState<SkeletonKnightIdleState>();
    }

    protected override void ChangeToChaseState()
    {
        stateMachine.ChangeState<SkeletonKnightChaseState>();
    }

    protected override void ChangeToAttackState()
    {
        stateMachine.ChangeState<SkeletonKnightAttackState>();
    }
}

public class SkeletonKnightJumpState : AICharacterJumpState
{
    public SkeletonKnightJumpState(SkeletonKnight skeleton) 
        : base(skeleton) 
    {
    }

    protected override void ChangeToIdleState()
    {
        stateMachine.ChangeState<SkeletonKnightIdleState>();
    }
}

public class SkeletonKnightFallState : AICharacterFallState
{
    public SkeletonKnightFallState(SkeletonKnight skeleton) 
        : base(skeleton) 
    {
    }

    protected override void ChangeToIdleState()
    {
        stateMachine.ChangeState<SkeletonKnightIdleState>();
    }
}

public class SkeletonKnightChaseState : AICharacterChaseState
{
    public SkeletonKnightChaseState(SkeletonKnight skeleton) 
        : base(skeleton) 
    {
    }

    protected override void ChangeToIdleState()
    {
        stateMachine.ChangeState<SkeletonKnightIdleState>();
    }

    protected override void ChangeToAttackState()
    {
        stateMachine.ChangeState<SkeletonKnightAttackState>();
    }
}

public class SkeletonKnightAttackState : AICharacterAttackState
{
    private const float StunDuration = 2.0f;

    protected new SkeletonKnight character => base.character as SkeletonKnight;

    public SkeletonKnightAttackState(SkeletonKnight skeleton) 
        : base(skeleton) 
    {
    }

    protected override void ChangeToIdleState()
    {
        stateMachine.ChangeState<SkeletonKnightIdleState>();
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

public class SkeletonKnightKnockbackState : AICharacterKnockbackState
{
    public SkeletonKnightKnockbackState(SkeletonKnight skeleton) 
        : base(skeleton)
    {
    }

    protected override void ChangeToIdleState()
    {
        stateMachine.ChangeState<SkeletonKnightIdleState>();
    }
}

public class SkeletonKnightStunnedState : AICharacterStunnedState
{
    public SkeletonKnightStunnedState(SkeletonKnight skeleton) 
        : base(skeleton)
    {
    }

    protected override void ChangeToIdleState()
    {
        stateMachine.ChangeState<SkeletonKnightIdleState>();
    }
}

public class SkeletonKnightDeadState : AICharacterDeadState
{
    public SkeletonKnightDeadState(SkeletonKnight skeleton) 
        : base(skeleton)
    {
    }
}

