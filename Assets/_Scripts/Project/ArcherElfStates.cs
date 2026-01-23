using UnityEngine;

public class ArcherElfIdleState : AICharacterIdleState
{
    public ArcherElfIdleState(ArcherElf character)
        : base(character)
    {
    }

    protected override void ChangeToChaseState()
    {
        stateMachine.ChangeState<ArcherElfChaseState>();
    }

    protected override void ChangeToMoveState()
    {
        stateMachine.ChangeState<ArcherElfMoveState>();
    }

    protected override void ChangeToAttackState()
    {
        stateMachine.ChangeState<ArcherElfAttackState>();
    }
}

public class ArcherElfMoveState : AICharacterMoveState
{
    public ArcherElfMoveState(ArcherElf character)
        : base(character)
    {
    }

    protected override void ChangeToIdleState()
    {
        stateMachine.ChangeState<ArcherElfIdleState>();
    }

    protected override void ChangeToChaseState()
    {
        stateMachine.ChangeState<ArcherElfChaseState>();
    }

    protected override void ChangeToAttackState()
    {
        stateMachine.ChangeState<ArcherElfAttackState>();
    }
}

public class ArcherElfJumpState : AICharacterJumpState
{
    public ArcherElfJumpState(ArcherElf character)
        : base(character)
    {
    }

    protected override void ChangeToIdleState()
    {
        stateMachine.ChangeState<ArcherElfIdleState>();
    }
}

public class ArcherElfFallState : AICharacterFallState
{
    public ArcherElfFallState(ArcherElf character)
        : base(character)
    {
    }

    protected override void ChangeToIdleState()
    {
        stateMachine.ChangeState<ArcherElfIdleState>();
    }
}

public class ArcherElfChaseState : AICharacterChaseState
{
    public ArcherElfChaseState(ArcherElf character)
        : base(character)
    {
    }

    protected override void ChangeToIdleState()
    {
        stateMachine.ChangeState<ArcherElfIdleState>();
    }

    protected override void ChangeToAttackState()
    {
        stateMachine.ChangeState<ArcherElfAttackState>();
    }
}

public class ArcherElfAttackState : AICharacterAttackState
{
    private const float ArrowSpeed = 10f;

    protected new ArcherElf character => base.character as ArcherElf;

    public ArcherElfAttackState(ArcherElf character)
        : base(character)
    {
    }

    protected override void ChangeToIdleState()
    {
        stateMachine.ChangeState<ArcherElfIdleState>();
    }

    protected override void OnFireEvent(Transform firePoint)
    {
        ArcherElfArrow arrow = Object.Instantiate(character.ArrowPrefab, firePoint.position, firePoint.rotation);
        arrow.Setup(character);
        arrow.Fire(new Vector2(character.FacingDirection, 0), ArrowSpeed);
    }
}

public class ArcherElfKnockbackState : AICharacterKnockbackState
{
    public ArcherElfKnockbackState(ArcherElf character)
        : base(character)
    {
    }

    protected override void ChangeToIdleState()
    {
        stateMachine.ChangeState<ArcherElfIdleState>();
    }
}

public class ArcherElfStunnedState : AICharacterStunnedState
{
    public ArcherElfStunnedState(ArcherElf character)
        : base(character)
    {
    }

    protected override void ChangeToIdleState()
    {
        stateMachine.ChangeState<ArcherElfIdleState>();
    }
}

public class ArcherElfDeadState : AICharacterDeadState
{
    public ArcherElfDeadState(ArcherElf character)
        : base(character)
    {
    }
}

