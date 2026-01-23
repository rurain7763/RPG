using UnityEngine;

public class ArcherElf : AICharacter
{
    public ArcherElfArrow ArrowPrefab;

    protected override void Awake()
    {
        base.Awake();

        StateMachine.AddState<ArcherElfIdleState>(this);
        StateMachine.AddState<ArcherElfMoveState>(this);
        StateMachine.AddState<ArcherElfChaseState>(this);
        StateMachine.AddState<ArcherElfAttackState>(this);
        StateMachine.AddState<ArcherElfKnockbackState>(this);
        StateMachine.AddState<ArcherElfStunnedState>(this);
        StateMachine.AddState<ArcherElfFallState>(this);
        StateMachine.AddState<ArcherElfJumpState>(this);
        StateMachine.AddState<ArcherElfDeadState>(this);
        StateMachine.AddGlobalTransition<ArcherElfDeadState>(() => CombatSystem.IsDead, 2);
        StateMachine.AddGlobalTransition<ArcherElfKnockbackState>(() => !CombatSystem.IsDead && CombatSystem.IsKnockbacked, 1);
        StateMachine.AddGlobalTransition<ArcherElfStunnedState>(() => !CombatSystem.IsDead && CombatSystem.IsStunned, 1);
        StateMachine.AddGlobalTransition<ArcherElfJumpState>(() => !CombatSystem.IsDead && Rigidbody.linearVelocityY > 0, 0);
        StateMachine.AddGlobalTransition<ArcherElfFallState>(() => !CombatSystem.IsDead && Rigidbody.linearVelocityY < 0, 0);
        StateMachine.AddTransition<ArcherElfKnockbackState, ArcherElfIdleState>(() => !CombatSystem.IsKnockbacked);
        StateMachine.AddTransition<ArcherElfStunnedState, ArcherElfIdleState>(() => !CombatSystem.IsStunned);
        StateMachine.AddTransition<ArcherElfJumpState, ArcherElfIdleState>(() => Mathf.Approximately(Rigidbody.linearVelocityY, 0));
        StateMachine.AddTransition<ArcherElfFallState, ArcherElfIdleState>(() => Mathf.Approximately(Rigidbody.linearVelocityY, 0));

        StateMachine.SetAsEntryState<ArcherElfIdleState>();
    }
}