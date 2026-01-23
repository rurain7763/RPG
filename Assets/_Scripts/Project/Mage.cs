using UnityEngine;

public class Mage : AICharacter, ISkillUser
{
    public TargetDetector2D TargetDetector { get; private set; }
    public EntitySkillSystem SkillSystem { get; private set; }

    protected override void Awake()
    {
        base.Awake();

        TargetDetector = GetComponentInChildren<TargetDetector2D>();

        SkillSystem = GetComponent<EntitySkillSystem>();

        StateMachine.AddState<MageIdleState>(this);
        StateMachine.AddState<MageMoveState>(this);
        StateMachine.AddState<MageChaseState>(this);
        StateMachine.AddState<MageAttackState>(this);
        StateMachine.AddState<MageKnockbackState>(this);
        StateMachine.AddState<MageStunnedState>(this);
        StateMachine.AddState<MageFallState>(this);
        StateMachine.AddState<MageJumpState>(this);
        StateMachine.AddState<MageRetreatState>(this);
        StateMachine.AddState<MageMagicBallRainState>(this);
        StateMachine.AddState<MageDeadState>(this);
        StateMachine.AddGlobalTransition<MageDeadState>(() => CombatSystem.IsDead, 2);
        StateMachine.AddGlobalTransition<MageKnockbackState>(() => !CombatSystem.IsDead && CombatSystem.IsKnockbacked, 1);
        StateMachine.AddGlobalTransition<MageStunnedState>(() => !CombatSystem.IsDead && CombatSystem.IsStunned, 1);
        StateMachine.AddGlobalTransition<MageJumpState>(() => !CombatSystem.IsDead && Rigidbody.linearVelocityY > 0, 0);
        StateMachine.AddGlobalTransition<MageFallState>(() => !CombatSystem.IsDead && Rigidbody.linearVelocityY < 0, 0);
        StateMachine.AddTransition<MageKnockbackState, MageIdleState>(() => !CombatSystem.IsKnockbacked);
        StateMachine.AddTransition<MageStunnedState, MageIdleState>(() => !CombatSystem.IsStunned);
        StateMachine.AddTransition<MageJumpState, MageIdleState>(() => Mathf.Approximately(Rigidbody.linearVelocityY, 0));
        StateMachine.AddTransition<MageFallState, MageIdleState>(() => Mathf.Approximately(Rigidbody.linearVelocityY, 0));

        StateMachine.SetAsEntryState<MageIdleState>();
    }

    public override void Begin()
    {
        base.Begin();

        SkillSystem.Begin();
        foreach (var skill in SkillSystem.Skills)
        {
            skill.AddUpgrade(SkillUpgradeFlag.Default);
        }
    }

    protected override void TickSystems(float delta)
    {
        base.TickSystems(delta);
        SkillSystem.Tick(delta);
    }

    public override void End()
    {
        base.End();
        SkillSystem.End();
    }
}