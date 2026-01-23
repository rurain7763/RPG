using UnityEngine;

public class SkeletonKnight : AICharacter
{
    public TargetDetector2D TargetDetector { get; private set; }

    protected override void Awake()
    {
        base.Awake();

        TargetDetector = GetComponentInChildren<TargetDetector2D>();

        StateMachine.AddState<SkeletonKnightIdleState>(this);
        StateMachine.AddState<SkeletonKnightMoveState>(this);
        StateMachine.AddState<SkeletonKnightChaseState>(this);
        StateMachine.AddState<SkeletonKnightAttackState>(this);
        StateMachine.AddState<SkeletonKnightKnockbackState>(this);
        StateMachine.AddState<SkeletonKnightStunnedState>(this);
        StateMachine.AddState<SkeletonKnightJumpState>(this);
        StateMachine.AddState<SkeletonKnightFallState>(this);
        StateMachine.AddState<SkeletonKnightDeadState>(this);
        StateMachine.AddGlobalTransition<SkeletonKnightDeadState>(() => CombatSystem.IsDead, 2);
        StateMachine.AddGlobalTransition<SkeletonKnightKnockbackState>(() => !CombatSystem.IsDead && CombatSystem.IsKnockbacked, 1);
        StateMachine.AddGlobalTransition<SkeletonKnightStunnedState>(() => !CombatSystem.IsDead && CombatSystem.IsStunned, 1);
        StateMachine.AddGlobalTransition<SkeletonKnightFallState>(() => !CombatSystem.IsDead && Rigidbody.linearVelocityY < 0f, 0);
        StateMachine.AddGlobalTransition<SkeletonKnightJumpState>(() => !CombatSystem.IsDead && Rigidbody.linearVelocityY > 0f, 0);
        StateMachine.AddTransition<SkeletonKnightKnockbackState, SkeletonKnightIdleState>(() => !CombatSystem.IsKnockbacked);
        StateMachine.AddTransition<SkeletonKnightStunnedState, SkeletonKnightIdleState>(() => !CombatSystem.IsStunned);
        StateMachine.AddTransition<SkeletonKnightFallState, SkeletonKnightIdleState>(() => Mathf.Approximately(Rigidbody.linearVelocityY, 0));
        StateMachine.AddTransition<SkeletonKnightJumpState, SkeletonKnightIdleState>(() => Mathf.Approximately(Rigidbody.linearVelocityY, 0));

        StateMachine.SetAsEntryState<SkeletonKnightIdleState>();
    }
}