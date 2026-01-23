using UnityEngine;

public abstract class EchoState : State
{
    protected static readonly int IdleAnimHash = Animator.StringToHash("Idle");
    protected static readonly int JumpAndFallAnimHash = Animator.StringToHash("Jump/Fall");
    protected static readonly int BasicAttack1AnimHash = Animator.StringToHash("BasicAttack_1");
    protected static readonly int BasicAttack2AnimHash = Animator.StringToHash("BasicAttack_2");
    protected static readonly int BasicAttack3AnimHash = Animator.StringToHash("BasicAttack_3");

    protected Echo echo;

    public EchoState(Echo echo)
    {
        this.echo = echo;
    }
}

public class EchoIdle : EchoState
{
    public EchoIdle(Echo echo) : base(echo) { }
    
    public override void Enter()
    {
        echo.Rigidbody.linearVelocity = Vector2.zero;
        echo.Animator.Play(IdleAnimHash);
    }

    public override void Execute()
    {
        if (!echo.IsGrounded)
        {
            stateMachine.ChangeState<EchoFall>();
        }
        else
        {
            stateMachine.ChangeState<EchoAttack>();
        }
    }
}

public class EchoFall : EchoState
{
    public EchoFall(Echo echo) : base(echo) { }

    public override void Enter()
    {
        echo.Animator.Play(JumpAndFallAnimHash);
    }

    public override void Execute()
    {
        if (echo.IsGrounded)
        {
            stateMachine.ChangeState<EchoIdle>();
        }
    }
}

public class EchoAttack : EchoState
{
    private const float KnockbackForce = 5;
    private const float KnockbackDuration = 0.2f;

    private const float ComboResetTime = 0.35f;

    private static readonly int[] AttackAnimHashs = new int[] {
        BasicAttack1AnimHash,
        BasicAttack2AnimHash,
        BasicAttack3AnimHash
    };

    private int lastAttackAnimIndex;
    private float lastAttackTime;
    private int currentAttackAnimHash;

    public EchoAttack(Echo echo) 
        : base(echo) 
    {
        lastAttackAnimIndex = -1;
        lastAttackTime = -ComboResetTime;
    }

    public override void Enter()
    {
        echo.AnimationEventReciever.RegisterEventHandler("Hit", HandleHitEvent);

        if (Time.time - lastAttackTime > ComboResetTime)
        {
            lastAttackAnimIndex = 0;
        }

        currentAttackAnimHash = AttackAnimHashs[lastAttackAnimIndex];

        echo.Rigidbody.linearVelocity = Vector2.zero;
        echo.Animator.Play(currentAttackAnimHash);
    }

    public override void Execute()
    {
        if (Helper.IsAnimationStateFinished(echo.Animator, currentAttackAnimHash))
        {
            stateMachine.ChangeState<EchoIdle>();
        }
    }

    public override void Exit()
    {
        echo.AnimationEventReciever.UnregisterEventHandler("Hit", HandleHitEvent);

        lastAttackAnimIndex = (lastAttackAnimIndex + 1) % AttackAnimHashs.Length;
        lastAttackTime = Time.time;

        echo.RemainAttackCount--;
    }

    private void HandleHitEvent(IEventData data)
    {
        echo.LastHitTargets.Clear();

        var targets = echo.TargetDetector.DetectTargets();
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

            if (combatable.CombatSystem.ChanceToEvasion(combatable.StatSystem.TotalEvasion.FinalValue))
            {
                continue;
            }

            var damage = RPG.CalcDamage(echo, combatable);

            combatable.CombatSystem.TakeDamage(damage);
            combatable.CombatSystem.Knockback(echo.CenterPosition, KnockbackForce, KnockbackDuration);

            echo.LastHitTargets.Add(entity);

            if (damage.IsCritical)
            {
                echo.VFXSystem.SpawnCriticalHitVFX(entity);
            }
            else
            {
                echo.VFXSystem.SpawnHitVFX(entity);
            }
        }
    }
}

public class EchoDead : EchoState
{
    public EchoDead(Echo echo) : base(echo) { }

    public override void Enter()
    {
        echo.VFXSystem.SpawnDeathVFX();
        echo.Expire();
    }
}