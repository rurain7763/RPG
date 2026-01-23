using UnityEngine;

public class SlimeIdleState : AICharacterIdleState
{
    public SlimeIdleState(Slime slime) 
        : base(slime)
    {
    }

    protected override void ChangeToChaseState()
    {
        stateMachine.ChangeState<SlimeChaseState>();
    }

    protected override void ChangeToMoveState()
    {
        stateMachine.ChangeState<SlimeMoveState>();
    }

    protected override void ChangeToAttackState()
    {
        stateMachine.ChangeState<SlimeAttackState>();
    }
}

public class SlimeMoveState : AICharacterMoveState
{
    public SlimeMoveState(Slime slime) 
        : base(slime) 
    {
    }

    protected override void ChangeToIdleState()
    {
        stateMachine.ChangeState<SlimeIdleState>();
    }

    protected override void ChangeToChaseState()
    {
        stateMachine.ChangeState<SlimeChaseState>();
    }

    protected override void ChangeToAttackState()
    {
        stateMachine.ChangeState<SlimeAttackState>();
    }
}

public class SlimeJumpState : AICharacterJumpState
{
    public SlimeJumpState(Slime slime) 
        : base(slime) 
    {
    }

    protected override void ChangeToIdleState()
    {
        stateMachine.ChangeState<SlimeIdleState>();
    }
}

public class SlimeFallState : AICharacterFallState
{
    public SlimeFallState(Slime slime) 
        : base(slime) 
    {
    }

    protected override void ChangeToIdleState()
    {
        stateMachine.ChangeState<SlimeIdleState>();
    }
}

public class SlimeChaseState : AICharacterChaseState
{
    public SlimeChaseState(Slime slime) 
        : base(slime) 
    {
    }

    protected override void ChangeToIdleState()
    {
        stateMachine.ChangeState<SlimeIdleState>();
    }

    protected override void ChangeToAttackState()
    {
        stateMachine.ChangeState<SlimeAttackState>();
    }
}

public class SlimeAttackState : AICharacterAttackState
{
    private const float StunDuration = 2.0f;

    protected new Slime character => base.character as Slime;

    public SlimeAttackState(Slime slime) 
        : base(slime) 
    {
    }

    protected override void ChangeToIdleState()
    {
        stateMachine.ChangeState<SlimeIdleState>();
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

public class SlimeKnockbackState : AICharacterKnockbackState
{
    public SlimeKnockbackState(Slime slime) 
        : base(slime)
    {
    }

    protected override void ChangeToIdleState()
    {
        stateMachine.ChangeState<SlimeIdleState>();
    }
}

public class SlimeStunnedState : AICharacterStunnedState
{
    public SlimeStunnedState(Slime slime) 
        : base(slime)
    {
    }

    protected override void ChangeToIdleState()
    {
        stateMachine.ChangeState<SlimeIdleState>();
    }
}

public class SlimeDeadState : AICharacterDeadState
{
    public new Slime character => base.character as Slime;

    public SlimeDeadState(Slime slime) 
        : base(slime)
    {
    }

    public override void Enter()
    {
        base.Enter();

        if (character.RemainSplitCountOnDeath == 0)
        {
            return;
        }

        const float anglePerSlime = 23.0f;
        const float maxArcAngle = 160.0f;
        const float force = 15.0f;

        Helper.EachDirectionsOnArc2D(Vector2.up, anglePerSlime, maxArcAngle, character.SpawnSlimeCountOnSplit, (index, dir) =>
        {
            Slime slime = GameObject.Instantiate<Slime>(character.SpawnSlimeOnDeadPrefab, character.transform.position, Quaternion.identity);
            slime.transform.localScale = character.transform.localScale * character.SplitSizeMultiplier;
            slime.Rigidbody.AddForce(dir.normalized * force, ForceMode2D.Impulse);

            // TODO: adjust stats
            slime.StatSystem.Health.BaseValue = character.StatSystem.Health.BaseValue * character.SplitStatMultiplier;
            slime.StatSystem.Damage.BaseValue = character.StatSystem.Damage.BaseValue * character.SplitStatMultiplier;

            if (character.Target)
            {
                slime.LookAt(character.Target.transform.position);
            }

            slime.RemainSplitCountOnDeath = character.RemainSplitCountOnDeath - 1;

            slime.Begin();
        });
    }
}

