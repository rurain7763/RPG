using UnityEngine;

public abstract class AICharacterState : State
{
    protected static readonly int IdleAnimHash = Animator.StringToHash("Idle");
    protected static readonly int MoveAnimHash = Animator.StringToHash("Move");
    protected static readonly int JumpAndFallAnimHash = Animator.StringToHash("Jump/Fall");
    protected static readonly int AttackAnimHash = Animator.StringToHash("Attack");
    protected static readonly int StunnedAnimHash = Animator.StringToHash("Stunned");
    protected static readonly int DeadAnimHash = Animator.StringToHash("Dead");

    protected static readonly int MoveAnimSpeedFactorHash = Animator.StringToHash("MoveAnimSpeedFactor");
    protected static readonly int AttackAnimSpeedFactorHash = Animator.StringToHash("AttackAnimSpeedFactor");

    protected AICharacter character;

    public AICharacterState(AICharacter character) => this.character = character;
}

public abstract class AICharacterIdleState : AICharacterState
{
    protected const float IdleDuration = 2f;

    protected float idleTimer = 0f;

    public AICharacterIdleState(AICharacter character) : base(character) { }

    public override void Enter()
    {
        character.CombatSystem.OnTakeDamage += HandleTakeDamage;
        idleTimer = 0f;
        character.Animator.Play(IdleAnimHash);
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
            if (character.IsEnemyInAttackRange(character.Target))
            {
                ChangeToAttackState();
            }
            else
            {
                ChangeToChaseState();
            }
        }
    }

    protected abstract void ChangeToChaseState();
    protected abstract void ChangeToAttackState();
    protected abstract void ChangeToMoveState();

    public override void Exit()
    {
        character.CombatSystem.OnTakeDamage -= HandleTakeDamage;
    }

    private void HandleTakeDamage(Damage damage)
    {
        if (damage.Source == null)
        {
            return;
        }

        character.Target = damage.Source.Owner;
    }
}

public abstract class AICharacterJumpState : AICharacterState
{
    public AICharacterJumpState(AICharacter character) 
        : base(character) 
    {
    }

    public override void Enter()
    {
        character.Animator.Play(JumpAndFallAnimHash);
    }

    protected abstract void ChangeToIdleState();
}

public abstract class AICharacterFallState : AICharacterState
{
    public AICharacterFallState(AICharacter character) 
        : base(character) 
    {
    }

    public override void Enter()
    {
        character.Animator.Play(JumpAndFallAnimHash);
    }

    public override void Exit()
    {
        character.Rigidbody.linearVelocity = Vector2.zero;
    }

    protected abstract void ChangeToIdleState();
}

public abstract class AICharacterMoveState : AICharacterState
{
    private const float MoveDuration = 3f;

    private float moveTimer = 0f;

    public AICharacterMoveState(AICharacter character) : base(character) { }

    public override void Enter()
    {
        character.CombatSystem.OnTakeDamage += HandleTakeDamage;

        moveTimer = 0f;

        if (character.IsCliffInFront())
        {
            character.FlipFacing();
        }
        else
        {
            float rnd = Random.Range(0f, 1f);
            if (rnd < 0.5f)
            {
                character.FlipFacing();
            }
        }

        character.Animator.Play(MoveAnimHash);
    }

    public override void Execute()
    {
        float moveSpeed = character.MoveSpeed;

        character.Animator.SetFloat(MoveAnimSpeedFactorHash, moveSpeed / character.DefaultMoveSpeed);
        character.Rigidbody.linearVelocity = new Vector2(moveSpeed * character.FacingDirection, character.Rigidbody.linearVelocity.y);

        moveTimer += Time.deltaTime;

        if (character.IsWallInFront())
        {
            character.FlipFacing();
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
            if (character.IsEnemyInAttackRange(character.Target))
            {
                ChangeToAttackState();
            }
            else
            {
                ChangeToChaseState();
            }
        }

        if (moveTimer >= MoveDuration || character.IsCliffInFront())
        {
            ChangeToIdleState();
        }
    }

    protected abstract void ChangeToIdleState();
    protected abstract void ChangeToChaseState();
    protected abstract void ChangeToAttackState();

    public override void Exit()
    {
        character.Animator.SetFloat(MoveAnimSpeedFactorHash, 1f);

        character.Rigidbody.linearVelocity = new Vector2(0f, character.Rigidbody.linearVelocity.y);

        character.CombatSystem.OnTakeDamage -= HandleTakeDamage;
    }

    private void HandleTakeDamage(Damage damage)
    {
        if (damage.Source == null)
        {
            return;
        }

        character.Target = damage.Source.Owner;
    }
}

public abstract class AICharacterChaseState : AICharacterState
{
    private const float MoveSpeedFactor = 2.0f;
    private const float NearbyRadius = 2.0f;
    protected virtual float lostTargetTime { get; } = 3.0f;

    private float lostTargetRemainTime;

    public AICharacterChaseState(AICharacter character) : base(character) { }

    public override void Enter()
    {
        character.Animator.Play(MoveAnimHash);
        lostTargetRemainTime = lostTargetTime;
    }

    public override void Execute()
    {
        float moveSpeed = character.MoveSpeed * MoveSpeedFactor;

        if (character.Target == null || (character.Target is ICombatable combatable && combatable.CombatSystem.IsDead))
        {
            character.Target = null;
            ChangeToIdleState();
            return;
        }

        character.LookAt(character.Target.CenterPosition);

        character.Animator.SetFloat(MoveAnimSpeedFactorHash, moveSpeed / character.DefaultMoveSpeed);

        if (character.IsEnemyInAttackRange(character.Target))
        {
            ChangeToAttackState();
        }
        else if (character.IsEnemyNearby(NearbyRadius, character.Target))
        {
            character.Rigidbody.linearVelocity = new Vector2(moveSpeed * character.FacingDirection, character.Rigidbody.linearVelocity.y);
            lostTargetRemainTime = lostTargetTime;
        }
        else if (lostTargetRemainTime > 0)
        {
            character.Rigidbody.linearVelocity = new Vector2(moveSpeed * character.FacingDirection, character.Rigidbody.linearVelocity.y);
            lostTargetRemainTime -= Time.deltaTime;
        }
        else
        {
            character.Target = null;
            ChangeToIdleState();
        }
    }

    protected abstract void ChangeToIdleState();
    protected abstract void ChangeToAttackState();

    public override void Exit()
    {
        character.Animator.SetFloat(MoveAnimSpeedFactorHash, 1f);
    }
}

public abstract class AICharacterAttackState : AICharacterState
{
    public AICharacterAttackState(AICharacter character) 
        : base(character) 
    {
    }

    public override void Enter()
    {
        character.AnimationEventReciever.RegisterEventHandler("Hit", HandleHitEvent);
        character.AnimationEventReciever.RegisterEventHandler("Fire", HandleFireEvent);

        character.LookAt(character.Target.CenterPosition);

        character.Rigidbody.linearVelocity = new Vector2(0f, character.Rigidbody.linearVelocity.y);
        character.Animator.Play(AttackAnimHash);
    }

    public override void Execute()
    {
        character.Animator.SetFloat(AttackAnimSpeedFactorHash, character.AttackSpeed);

        if (character.Animator.GetCurrentAnimatorStateInfo(0).normalizedTime >= 1f)
        {
            ChangeToIdleState();
        }
    }

    protected abstract void ChangeToIdleState();

    public override void Exit()
    {
        character.AnimationEventReciever.UnregisterEventHandler("Fire", HandleFireEvent);
        character.AnimationEventReciever.UnregisterEventHandler("Hit", HandleHitEvent);

        character.Animator.SetFloat(AttackAnimSpeedFactorHash, 1f);
    }

    private void HandleHitEvent(IEventData data)
    {
        // NOTE: maybe use data in the future, for now we just ignore it
        OnHitEvent();
    }

    protected virtual void OnHitEvent()
    {
    }

    private void HandleFireEvent(IEventData data)
    {
        if (data is not StringEventData actual)
        {
            return;
        }

        Transform firePoint = character.transform.Find(actual.Value);
        if (firePoint == null)
        {
            Logger.Warn($"Fire point '{actual.Value}' not found on AICharacter '{character.name}'.");
            return;
        }

        OnFireEvent(firePoint);
    }

    protected virtual void OnFireEvent(Transform firePoint)
    {
    }
}

public abstract class AICharacterKnockbackState : AICharacterState
{
    public AICharacterKnockbackState(AICharacter character) : base(character)
    {
    }

    public override void Enter()
    {
        character.Animator.Play(IdleAnimHash);
    }

    public override void Execute()
    {
        if (!character.CombatSystem.IsKnockbacked)
        {
            ChangeToIdleState();
        }
    }

    protected abstract void ChangeToIdleState();
}

public abstract class AICharacterStunnedState : AICharacterState
{
    public AICharacterStunnedState(AICharacter character) : base(character)
    {
    }

    public override void Enter()
    {
        character.Animator.Play(StunnedAnimHash);
    }

    public override void Execute()
    {
        if (!character.CombatSystem.IsStunned)
        {
            ChangeToIdleState();
        }
    }

    protected abstract void ChangeToIdleState();
}

public abstract class AICharacterDeadState : AICharacterState
{
    public AICharacterDeadState(AICharacter character) : base(character)
    {
    }

    public override void Enter()
    {
        character.Rigidbody.linearVelocity = Vector2.zero;

        character.VFXSystem.SpawnDeathVFX();
        character.Animator.Play(DeadAnimHash);
    }

    public override void Execute()
    {
        if (character.Animator.GetCurrentAnimatorStateInfo(0).normalizedTime >= 1f)
        {
            GameObject.Destroy(character.gameObject);
        }
    }
}

