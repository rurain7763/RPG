using System;
using System.Collections.Generic;
using UnityEngine;

public class Echo : Entity, ICombatable
{
    [SerializeField] private LayerMask enemyLayer;
    [SerializeField] private float duration = 5f;
    [SerializeField] private int maxAttackCount = 3;

    private StateMachine stateMachine;

    public int MaxAttackCount
    {
        get => maxAttackCount;
        set
        {
            maxAttackCount = value;
            RemainAttackCount = maxAttackCount;
        }
    }

    public int RemainAttackCount { get; set; }

    public float Duration
    {
        get => duration;
        set
        {
            duration = value;
            RemainTime = duration;
        }
    }

    public float RemainTime { get; set; }

    public List<Entity> LastHitTargets { get; set; } = new();

    public AnimationEventReciever AnimationEventReciever { get; private set; }
    public Rigidbody2D Rigidbody { get; private set; }
    public TargetDetector2D TargetDetector { get; private set; }
    public EntityCombatSystem CombatSystem { get; private set; }
    public EntityStatSystem StatSystem { get; private set; }
    public BuffSystem BuffSystem { get; private set; }
    public EntityVFXSystem VFXSystem { get; private set; }

    public event Action OnExpired;

    protected override void Awake()
    {
        base.Awake();

        AnimationEventReciever = GetComponentInChildren<AnimationEventReciever>();
        Rigidbody = GetComponent<Rigidbody2D>();
        TargetDetector = GetComponentInChildren<TargetDetector2D>();
        CombatSystem = GetComponent<EntityCombatSystem>();
        StatSystem = GetComponent<EntityStatSystem>();
        BuffSystem = new BuffSystem(this);
        VFXSystem = GetComponent<EntityVFXSystem>();

        stateMachine = new StateMachine();
        stateMachine.AddState<EchoIdle>(this);
        stateMachine.AddState<EchoFall>(this);
        stateMachine.AddState<EchoAttack>(this);
        stateMachine.AddState<EchoDead>(this);
        stateMachine.AddGlobalTransition<EchoDead>(() => CombatSystem.IsDead || RemainTime <= 0 || (MaxAttackCount != 0 && RemainAttackCount <= 0), 0);
        stateMachine.SetAsEntryState<EchoIdle>();

        RemainTime = duration;
        RemainAttackCount = maxAttackCount;
    }

    public override void Begin()
    {
        base.Begin();

        CombatSystem.MaxHealth = StatSystem.TotalHealth.FinalValue;
        CombatSystem.HealthRegeneration = StatSystem.HealthRegeneration.FinalValue;
        CombatSystem.SetHealthToMax();

        StatSystem.TotalHealth.OnStatChanged += () => CombatSystem.MaxHealth = StatSystem.TotalHealth.FinalValue;
        StatSystem.HealthRegeneration.OnStatChanged += () => CombatSystem.HealthRegeneration = StatSystem.HealthRegeneration.FinalValue;

        stateMachine.Begin();
    }

    public override void Tick(float delta)
    {
        base.Tick(delta);

        RemainTime -= delta;

        CombatSystem.Tick(delta);
        BuffSystem.Tick(delta);

        stateMachine.Tick();

        Animator.SetFloat("yVelocity", Rigidbody.linearVelocity.y);
    }

    public void Expire()
    {
        OnExpired?.Invoke();
        Destroy(gameObject);
    }
}
