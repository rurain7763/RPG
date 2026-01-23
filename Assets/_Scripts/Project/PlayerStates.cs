using UnityEngine;

public abstract class PlayerState : State
{
    protected static readonly int IdleAnimHash = Animator.StringToHash("Idle");
    protected static readonly int MoveAnimHash = Animator.StringToHash("Move");
    protected static readonly int JumpAndFallAnimHash = Animator.StringToHash("Jump/Fall");
    protected static readonly int WallSlideAnimHash = Animator.StringToHash("WallSlide");
    protected static readonly int DashAnimHash = Animator.StringToHash("Dash");
    protected static readonly int BasicAttack1AnimHash = Animator.StringToHash("BasicAttack_1");
    protected static readonly int BasicAttack2AnimHash = Animator.StringToHash("BasicAttack_2");
    protected static readonly int BasicAttack3AnimHash = Animator.StringToHash("BasicAttack_3");
    protected static readonly int JumpAttakStartAnimHash = Animator.StringToHash("JumpAttack_Start");
    protected static readonly int JumpAttakEndAnimHash = Animator.StringToHash("JumpAttack_End");
    protected static readonly int CounterAttackAnimHash = Animator.StringToHash("CounterAttack");
    protected static readonly int ThrowSwordStartAnimHash = Animator.StringToHash("ThrowSword_Start");
    protected static readonly int ThrowSwordEndAnimHash = Animator.StringToHash("ThrowSword_End");
    protected static readonly int DeadAnimHash = Animator.StringToHash("Dead");

    protected static readonly int MoveAnimSpeedFactorHash = Animator.StringToHash("MoveAnimSpeedFactor");
    protected static readonly int AttackAnimSpeedFactorHash = Animator.StringToHash("AttackAnimSpeedFactor");

    protected Player player;

    public PlayerState(Player player) => this.player = player;

    protected void HandleMovement(float factor)
    {
        float xAxis = player.MovementAxis.x;

        if (xAxis == 0)
        {
            return;
        }

        player.Rigidbody.linearVelocity = new Vector2(xAxis * factor, player.Rigidbody.linearVelocity.y);
        if ((xAxis > 0 && !player.IsFacingRight) || (xAxis < 0 && player.IsFacingRight))
        {
            player.FlipFacing();
        }
    }

    protected void HandleDash()
    {
        if (!player.InputSystem.Player.Dash.triggered || !player.SkillSystem.TryGetSkill(out Dash dash) || !dash.CanUse(player.gameObject))
        {
            return;
        }

        dash.SetToCooldown();
        stateMachine.ChangeState<PlayerDash>();
    }

    protected void HandleThrowSword()
    {
        if (!player.InputSystem.Player.ThrowSword.triggered)
        {
            return;
        }

        if (!player.SkillSystem.TryGetSkill(out ThrowSword throwSword) || !throwSword.CanUse(player.gameObject))
        {
            return;
        }

        throwSword.SetToCooldown();
        stateMachine.ChangeState<PlayerThrowSword>();
    }

    protected void HandleHit(float knockbackForce, float knockbackDuration, float airborneForce, float airborneDuration)
    {
        int hitCount = 0;

        var targets = player.TargetDetector.DetectTargets();
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

            var damage = RPG.CalcDamage(player, combatable);
            var buff = RPG.CalcBuffByDamage(player, combatable, damage);

            combatable.CombatSystem.TakeDamage(damage);
            combatable.BuffSystem.AddBuff(buff);

            if (knockbackForce > 0)
            {
                combatable.CombatSystem.Knockback(player.transform.position, knockbackForce, knockbackDuration);
            }

            if (airborneForce > 0)
            {
                combatable.CombatSystem.Airborne(airborneForce, airborneDuration);
            }

            if (combatable.CombatSystem.IsDead)
            {
                RPG.EventDispatcher.DispatchEvent(new EnemyKilledEvent(entity));
            }

            if (damage.IsCritical)
            {
                player.VFXSystem.SpawnCriticalHitVFX(entity);
            }
            else
            {
                player.VFXSystem.SpawnHitVFX(entity);
            }

            player.SFXSystem.PlayHitSFX();

            hitCount++;
        }

        if (hitCount == 0)
        {
            player.SFXSystem.PlayMissSFX();
        }
    }
}

public class PlayerIdle : PlayerState
{
    public PlayerIdle(Player player) : base(player) { }

    public override void Enter()
    {
        player.Rigidbody.linearVelocity = Vector2.zero;
        player.Animator.Play(IdleAnimHash);
    }

    public override void Execute()
    {
        if (player.MovementAxis.x != 0)
        {
            stateMachine.ChangeState<PlayerMove>();
        }

        HandleDash();

        if (player.InputSystem.Player.Counter.triggered)
        {
            stateMachine.ChangeState<PlayerCounterAttack>();
        }

        HandleThrowSword();

        if (player.InputSystem.Player.Attack.triggered)
        {
            stateMachine.ChangeState<PlayerBasicAttack>();
        }

        if (player.InputSystem.Player.Jump.triggered)
        {
            stateMachine.ChangeState<PlayerJump>();
        }

        if (!player.IsGrounded)
        {
            stateMachine.ChangeState<PlayerFall>();
        }
    }
}

public class PlayerMove : PlayerState
{
    public PlayerMove(Player player) : base(player) { }

    public override void Enter()
    {
        player.Animator.Play(MoveAnimHash);
    }

    public override void Execute()
    {
        player.Animator.SetFloat(MoveAnimSpeedFactorHash, player.MoveSpeed / player.DefaultMoveSpeed);

        HandleMovement(player.MoveSpeed);

        if (player.MovementAxis.x == 0)
        {
            stateMachine.ChangeState<PlayerIdle>();
        }

        HandleDash();
        HandleThrowSword();

        if (player.InputSystem.Player.Attack.triggered)
        {
            stateMachine.ChangeState<PlayerBasicAttack>();
        }

        if (player.InputSystem.Player.Jump.triggered)
        {
            stateMachine.ChangeState<PlayerJump>();
        }
        
        if (!player.IsGrounded)
        {
            stateMachine.ChangeState<PlayerFall>();
        }
    }

    public override void Exit()
    {
        player.Animator.SetFloat(MoveAnimSpeedFactorHash, 1f);
    }
}

public class PlayerJump : PlayerState
{
    public PlayerJump(Player player) : base(player) { }

    public override void Enter()
    {
        player.Animator.Play(JumpAndFallAnimHash);
        player.Rigidbody.linearVelocity = new Vector2(player.Rigidbody.linearVelocity.x, player.JumpForce);
    }

    public override void Execute()
    {
        HandleMovement(player.MoveSpeed * 0.7f);
        HandleDash();

        if (player.InputSystem.Player.Attack.triggered)
        {
            stateMachine.ChangeState<PlayerJumpAttack>();
        }

        if (player.Rigidbody.linearVelocity.y < 0)
        {
            stateMachine.ChangeState<PlayerFall>();
        }
        
        if (player.IsWallInFront())
        {
            stateMachine.ChangeState<PlayerWallSlide>();
        }
    }
}

public class PlayerFall : PlayerState
{
    public PlayerFall(Player player) : base(player) { }

    public override void Enter()
    {
        player.Animator.Play(JumpAndFallAnimHash);
    }

    public override void Execute()
    {
        HandleMovement(player.MoveSpeed * 0.7f);
        HandleDash();

        if (player.InputSystem.Player.Attack.triggered)
        {
            stateMachine.ChangeState<PlayerJumpAttack>();
        }

        if (player.IsGrounded)
        {
            stateMachine.ChangeState<PlayerIdle>();
        }
        
        if (player.IsWallInFront())
        {
            stateMachine.ChangeState<PlayerWallSlide>();
        }
    }
}

public class PlayerWallSlide : PlayerState
{
    public PlayerWallSlide(Player player) : base(player) { }

    public override void Enter()
    {
        player.Animator.Play(WallSlideAnimHash);
    }

    public override void Execute()
    {
        if (player.MovementAxis.y < 0)
        {
            player.Rigidbody.linearVelocity = new Vector2(0, player.Rigidbody.linearVelocity.y);
        }
        else
        {
            player.Rigidbody.linearVelocity = new Vector2(0, player.Rigidbody.linearVelocity.y * 0.3f);
        }

        HandleMovement(player.MoveSpeed);

        if (player.InputSystem.Player.Jump.triggered)
        {
            stateMachine.ChangeState<PlayerWallJump>();
        }

        if (!player.IsWallInFront())
        {
            stateMachine.ChangeState<PlayerFall>();
        }

        if (player.IsGrounded)
        {
            player.FlipFacing();
            stateMachine.ChangeState<PlayerIdle>();
        }
    }
}

public class PlayerWallJump : PlayerState
{
    private float WallJumpHorizontalForce;
    private float WallJumpVerticalForce;

    public PlayerWallJump(Player player) : base(player) 
    { 
        WallJumpHorizontalForce = 10f;
        WallJumpVerticalForce = 12f;
    }

    public override void Enter()
    {
        player.Animator.Play(JumpAndFallAnimHash);
        float horizontalForce = player.IsFacingRight ? -WallJumpHorizontalForce : WallJumpHorizontalForce;
        player.Rigidbody.linearVelocity = new Vector2(horizontalForce, WallJumpVerticalForce);
        player.FlipFacing();
    }

    public override void Execute()
    {
        HandleMovement(player.MoveSpeed * 0.7f);
        HandleDash();

        if (player.InputSystem.Player.Attack.triggered)
        {
            stateMachine.ChangeState<PlayerJumpAttack>();
        }

        if (player.Rigidbody.linearVelocity.y < 0)
        {
            stateMachine.ChangeState<PlayerFall>();
        }

        if (player.IsWallInFront())
        {
            stateMachine.ChangeState<PlayerWallSlide>();
        }
    }
}

public class PlayerDash : PlayerState
{
    private Dash dashSkill;

    public PlayerDash(Player player) : base(player) { }

    public override void Enter()
    {
        dashSkill = player.SkillSystem.GetSkill<Dash>();
        dashSkill.Use(player.gameObject);
    }

    public override void Execute()
    {
        if (dashSkill.IsComplete())
        {
            if (player.IsGrounded)
            {
                stateMachine.ChangeState<PlayerIdle>();
            }
            else
            {
                stateMachine.ChangeState<PlayerFall>();
            }
        }
    }

    public override void Exit()
    {
        dashSkill.Cleanup();
    }
}

public class PlayerBasicAttack : PlayerState
{
    private const float KnockbackForce = 5;

    private const float comboResetTime = 0.35f;

    private static readonly int[] attackAnimHashs = new int[] {
        BasicAttack1AnimHash,
        BasicAttack2AnimHash,
        BasicAttack3AnimHash
    };

    private static readonly Vector2[] feedbackForces = new Vector2[] {
        new Vector2(3f, 1.5f),
        new Vector2(1.0f, 2.5f),
        new Vector2(2.75f, 1.75f)
    };

    private int lastAttackAnimIndex;
    private float lastAttackTime;
    private int currentAttackAnimHash;

    public PlayerBasicAttack(Player player) 
        : base(player) 
    {
        lastAttackAnimIndex = -1;
        lastAttackTime = -comboResetTime;
    }

    public override void Enter()
    {
        player.AnimationEventReciever.RegisterEventHandler("Hit", HandleHitAnimEvent);

        if (Time.time - lastAttackTime > comboResetTime)
        {
            lastAttackAnimIndex = 0;
        }

        currentAttackAnimHash = attackAnimHashs[lastAttackAnimIndex];

        Vector2 feedbackForce = feedbackForces[lastAttackAnimIndex];
        player.Rigidbody.linearVelocity = new Vector2(feedbackForce.x * player.FacingDirection, feedbackForce.y);

        player.Animator.Play(currentAttackAnimHash);
    }

    public override void Execute()
    {
        player.Animator.SetFloat(AttackAnimSpeedFactorHash, player.AttackSpeed / player.DefaultAttackSpeed);

        HandleDash();

        if (Helper.IsAnimationStateFinished(player.Animator, currentAttackAnimHash))
        {
            stateMachine.ChangeState<PlayerIdle>();
        }
    }

    public override void Exit()
    {
        player.AnimationEventReciever.UnregisterEventHandler("Hit", HandleHitAnimEvent);

        player.Animator.SetFloat(AttackAnimSpeedFactorHash, 1f);

        player.Rigidbody.linearVelocity = Vector2.zero;

        lastAttackAnimIndex = (lastAttackAnimIndex + 1) % attackAnimHashs.Length;
        lastAttackTime = Time.time;
    }

    private void HandleHitAnimEvent(IEventData data)
    {
        HandleHit(KnockbackForce, 0.2f, 0f, 0f);
    }
}

public class PlayerJumpAttack : PlayerState
{
    private const float AirborneForce = 10f;

    private bool enterGrounded;

    public PlayerJumpAttack(Player player) : base(player) { }

    public override void Enter()
    {
        player.AnimationEventReciever.RegisterEventHandler("Hit", HandleHitAnimEvent);

        player.Animator.Play(JumpAttakStartAnimHash);
        enterGrounded = false;
    }

    public override void Execute()
    {
        player.Animator.SetFloat(AttackAnimSpeedFactorHash, player.AttackSpeed / player.DefaultAttackSpeed);

        if (!enterGrounded && player.IsGrounded)
        {
            player.Rigidbody.linearVelocity = Vector2.zero;
            player.Animator.Play(JumpAttakEndAnimHash);
            enterGrounded = true;
        }

        if (enterGrounded && Helper.IsAnimationStateFinished(player.Animator, JumpAttakEndAnimHash))
        {
            stateMachine.ChangeState<PlayerIdle>();
        }
    }

    public override void Exit()
    {
        player.AnimationEventReciever.UnregisterEventHandler("Hit", HandleHitAnimEvent);

        player.Animator.SetFloat(AttackAnimSpeedFactorHash, 1f);
    }

    private void HandleHitAnimEvent(IEventData data)
    {
        HandleHit(0f, 0f, AirborneForce, 1.0f);
    }
}

public class PlayerCounterAttack : PlayerState
{
    public PlayerCounterAttack(Player player) : base(player) { }

    public override void Enter()
    {
        player.CombatSystem.ActiveCounter = true;
        player.Animator.Play(CounterAttackAnimHash);
    }

    public override void Execute()
    {
        if (Helper.IsAnimationStateFinished(player.Animator, CounterAttackAnimHash))
        {
            stateMachine.ChangeState<PlayerIdle>();
        }
    }

    public override void Exit()
    {
        player.CombatSystem.ActiveCounter = false;
    }
}

public class PlayerThrowSword : PlayerState
{
    private ThrowSword throwSwordSkill;

    private bool isAiming;
    private float aimRad;

    public PlayerThrowSword(Player player) 
        : base(player) 
    {
        aimRad = Helper.HalfPI * 0.5f;
    }

    private Vector2 GetThrowDirection()
    {
        return new Vector2(player.FacingDirection * Mathf.Cos(aimRad), Mathf.Sin(aimRad));
    }

    public override void Enter()
    {
        isAiming = true;
        throwSwordSkill = player.SkillSystem.GetSkill<ThrowSword>();

        player.AnimationEventReciever.OnEventTriggered += throwSwordSkill.HandleEvents;

        if (throwSwordSkill.IsInSequence())
        {
            throwSwordSkill.Use(player.gameObject);
            stateMachine.ChangeState<PlayerIdle>();
            return;
        }

        var parameters = new ThrowSwordParameters
        {
            ThrowDirection = GetThrowDirection()
        };

        throwSwordSkill.Use(player.gameObject, parameters);

        player.Rigidbody.linearVelocity = new Vector2(0f, player.Rigidbody.linearVelocity.y);
        player.Animator.Play(ThrowSwordStartAnimHash);
    }

    public override void Execute()
    {
        if (player.InputSystem.Player.ThrowSword.WasReleasedThisFrame())
        {
            isAiming = false;
            player.Animator.Play(ThrowSwordEndAnimHash);
        }

        if (isAiming)
        {
            Vector2 inputAim = player.InputSystem.Player.Aim.ReadValue<Vector2>();

            if (inputAim.x != 0)
            {
                player.SetFacing(inputAim.x > 0);
            }

            if (inputAim.y != 0)
            {
                aimRad = Mathf.Min(Helper.HalfPI, Mathf.Max(0, aimRad + inputAim.y * Time.deltaTime));
            }

            var parameters = new ThrowSwordParameters
            {
                ThrowDirection = GetThrowDirection()
            };

            throwSwordSkill.UpdateParameters(parameters);
        }
        else if(Helper.IsAnimationStateFinished(player.Animator, ThrowSwordEndAnimHash))
        {
            stateMachine.ChangeState<PlayerIdle>();
        }
    }

    public override void Exit()
    {
        player.AnimationEventReciever.OnEventTriggered -= throwSwordSkill.HandleEvents;
    }
}

public class PlayerDead : PlayerState
{
    public PlayerDead(Player player) : base(player) { }

    public override void Enter()
    {
        player.Rigidbody.linearVelocity = Vector2.zero;
        player.Animator.Play(DeadAnimHash);
    }
}