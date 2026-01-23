using UnityEngine;

public class Slime : AICharacter
{
    public TargetDetector2D TargetDetector { get; private set; }

    public Slime SpawnSlimeOnDeadPrefab;
    public int RemainSplitCountOnDeath = 2;
    public int SpawnSlimeCountOnSplit = 2;
    public float SplitSizeMultiplier = 0.8f;
    public float SplitStatMultiplier = 0.5f;

    protected override void Awake()
    {
        base.Awake();

        TargetDetector = GetComponentInChildren<TargetDetector2D>();

        StateMachine.AddState<SlimeIdleState>(this);
        StateMachine.AddState<SlimeMoveState>(this);
        StateMachine.AddState<SlimeChaseState>(this);
        StateMachine.AddState<SlimeAttackState>(this);
        StateMachine.AddState<SlimeKnockbackState>(this);
        StateMachine.AddState<SlimeStunnedState>(this);
        StateMachine.AddState<SlimeFallState>(this);
        StateMachine.AddState<SlimeJumpState>(this);
        StateMachine.AddState<SlimeDeadState>(this);
        StateMachine.AddGlobalTransition<SlimeDeadState>(() => CombatSystem.IsDead, 2);
        StateMachine.AddGlobalTransition<SlimeKnockbackState>(() => !CombatSystem.IsDead && CombatSystem.IsKnockbacked, 1);
        StateMachine.AddGlobalTransition<SlimeStunnedState>(() => !CombatSystem.IsDead && CombatSystem.IsStunned, 1);
        StateMachine.AddGlobalTransition<SlimeJumpState>(() => !CombatSystem.IsDead && Rigidbody.linearVelocityY > 0, 0);
        StateMachine.AddGlobalTransition<SlimeFallState>(() => !CombatSystem.IsDead && Rigidbody.linearVelocityY < 0, 0);
        StateMachine.AddTransition<SlimeKnockbackState, SlimeIdleState>(() => !CombatSystem.IsKnockbacked);
        StateMachine.AddTransition<SlimeStunnedState, SlimeIdleState>(() => !CombatSystem.IsStunned);
        StateMachine.AddTransition<SlimeJumpState, SlimeIdleState>(() => Mathf.Approximately(Rigidbody.linearVelocityY, 0));
        StateMachine.AddTransition<SlimeFallState, SlimeIdleState>(() => Mathf.Approximately(Rigidbody.linearVelocityY, 0));

        StateMachine.SetAsEntryState<SlimeIdleState>();
    }
}