using UnityEngine;

public class Reaper : AICharacter
{
    [SerializeField] private BoxCollider2D availableArea;

    private bool hasRegisteredHud = false;

    public TargetDetector2D TargetDetector { get; private set; }
    public EntitySkillSystem SkillSystem { get; private set; }

    public override Entity Target
    {
        get => base.Target;
        set
        {
            Entity prevTarget = base.Target;
            base.Target = value;
            if (prevTarget == null && base.Target != null)
            {
                IncludedLevel.Environment.PlayBGM(BGMID.Reaper);
                
                if (!hasRegisteredHud)
                {
                    var hud = RPG.UISys.GetActiveStatic<HUDUI>();
                    if (hud != null)
                    {
                        hud.RegisterHpDisplayer("Reaper", this);
                        hasRegisteredHud = true;
                    }
                }
            }
            else if (prevTarget != null && base.Target == null)
            {
                IncludedLevel.PlayDefaultBGM();

                if (hasRegisteredHud)
                {
                    var hud = RPG.UISys.GetActiveStatic<HUDUI>();
                    if (hud != null)
                    {
                        hud.UnregisterHpDisplayer(this);
                        hasRegisteredHud = false;
                    }
                }
            }
        }
    }

    protected override void Awake()
    {
        base.Awake();

        TargetDetector = GetComponentInChildren<TargetDetector2D>();

        SkillSystem = GetComponent<EntitySkillSystem>();

        StateMachine.AddState<ReaperIdleState>(this);
        StateMachine.AddState<ReaperMoveState>(this);
        StateMachine.AddState<ReaperChaseState>(this);
        StateMachine.AddState<ReaperAttackState>(this);
        StateMachine.AddState<ReaperStunnedState>(this);
        StateMachine.AddState<ReaperJumpState>(this);
        StateMachine.AddState<ReaperFallState>(this);
        StateMachine.AddState<ReaperBlinkState>(this);
        StateMachine.AddState<ReaperSpellState>(this);
        StateMachine.AddState<ReaperDeadState>(this);
        StateMachine.AddGlobalTransition<ReaperDeadState>(() => CombatSystem.IsDead, 2);
        StateMachine.AddGlobalTransition<ReaperStunnedState>(() => !CombatSystem.IsDead && CombatSystem.IsStunned, 1);
        StateMachine.AddGlobalTransition<ReaperFallState>(() => !CombatSystem.IsDead && Rigidbody.linearVelocityY < 0f, 0);
        StateMachine.AddGlobalTransition<ReaperJumpState>(() => !CombatSystem.IsDead && Rigidbody.linearVelocityY > 0f, 0);
        StateMachine.AddTransition<ReaperStunnedState, ReaperIdleState>(() => !CombatSystem.IsStunned);
        StateMachine.AddTransition<ReaperFallState, ReaperIdleState>(() => Mathf.Approximately(Rigidbody.linearVelocityY, 0));
        StateMachine.AddTransition<ReaperJumpState, ReaperIdleState>(() => Mathf.Approximately(Rigidbody.linearVelocityY, 0));

        StateMachine.SetAsEntryState<ReaperIdleState>();
    }

    public override void Begin()
    {
        base.Begin();

        SkillSystem.Begin();
        foreach (var skill in SkillSystem.Skills)
        {
            skill.AddUpgrade(SkillUpgradeCommonFlag.Default);
        }

        CombatSystem.ActiveKnockbackImmunity = true;
        CombatSystem.ActiveAirborneImmunity = true;
        CombatSystem.ActiveStunImmunity = true;
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

    public Rect GetAvailableArea()
    {
        var bounds = availableArea.bounds;
        return new Rect(bounds.min.x, bounds.min.y, bounds.size.x, bounds.size.y);
    }
}