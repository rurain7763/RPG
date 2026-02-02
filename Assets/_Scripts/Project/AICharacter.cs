using UnityEngine;

public abstract class AICharacter : Entity, ICombatable
{
    private static readonly int YVelocityAnimHash = Animator.StringToHash("yVelocity");

    [SerializeField] private AICharacterData data;

    public Transform HpBarAnchor;
    public int Level = 1;
    public float VisionRange = 5f;
    public float AttackRange = 1.5f;

    public AnimationEventReciever AnimationEventReciever { get; private set; }
    public Rigidbody2D Rigidbody { get; private set; }
    public EntityCombatSystem CombatSystem { get; private set; }
    public EntityStatSystem StatSystem { get; private set; }
    public BuffSystem BuffSystem { get; private set; }
    public EntityVFXSystem VFXSystem { get; private set; }
    public EntitySFXSystem SFXSystem { get; private set; }
    public StateMachine StateMachine { get; private set; }

    public float MoveSpeed
    {
        get
        {
            return StatSystem.ActiveSpeed.FinalValue * StatSystem.MoveSpeed.FinalValue;
        }
    }

    public float AttackSpeed
    {
        get
        {
            return StatSystem.ActiveSpeed.FinalValue * StatSystem.AttackSpeed.FinalValue;
        }
    }

    protected Entity target;
    private HpBarPool hpBarPool;
    private HpBar hpBar;

    public AICharacterData Data => data;

    public virtual Entity Target
    {
        get => target;
        set => target = value;
    }

    protected override void Awake()
    {
        base.Awake();

        AnimationEventReciever = GetComponentInChildren<AnimationEventReciever>();

        Rigidbody = GetComponent<Rigidbody2D>();

        CombatSystem = GetComponent<EntityCombatSystem>();

        StatSystem = GetComponent<EntityStatSystem>();

        BuffSystem = new BuffSystem(this);

        VFXSystem = GetComponent<EntityVFXSystem>();

        SFXSystem = GetComponent<EntitySFXSystem>();

        StateMachine = new StateMachine();
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();

        CombatSystem.OnDie -= HandleOnDie;

        if (hpBar != null)
        {
            hpBarPool.ReleaseObject(hpBar);
            hpBar = null;
        }
    }

    public override void Begin()
    {
        base.Begin();

        CombatSystem.MaxHealth = StatSystem.TotalHealth.FinalValue;
        CombatSystem.HealthRegeneration = StatSystem.HealthRegeneration.FinalValue;
        CombatSystem.SetHealthToMax();

        StatSystem.TotalHealth.OnStatChanged += () => CombatSystem.MaxHealth = StatSystem.TotalHealth.FinalValue;
        StatSystem.HealthRegeneration.OnStatChanged += () => CombatSystem.HealthRegeneration = StatSystem.HealthRegeneration.FinalValue;

        StateMachine.Begin();

        hpBarPool = IncludedLevel.Environment.HpBarPool;

        hpBar = hpBarPool.GetObject();
        hpBar.SetHp(CombatSystem.CurrentHealth, CombatSystem.MaxHealth);
        CombatSystem.OnHealthChanged += () => hpBar.SetHp(CombatSystem.CurrentHealth, CombatSystem.MaxHealth);
        hpBar.GetComponent<WorldToUIFollower>().SetAnchor(HpBarAnchor);

        CombatSystem.OnDie += HandleOnDie;
    }

    private void HandleOnDie(Damage damage)
    {
        if (hpBar != null)
        {
            hpBarPool.ReleaseObject(hpBar);
            hpBar = null;
        }

        if (damage.Source.Owner is Player player)
        {
            int expReward = data.GetExp(Level);
            player.PlayLevelSystem.AddExp(expReward);
        }
    }

    public sealed override void Tick(float delta)
    {
        base.Tick(delta);

        TickSystems(delta);
        StateMachine.Tick();

        Animator.SetFloat(YVelocityAnimHash, Rigidbody.linearVelocity.y);
    }

    protected virtual void TickSystems(float delta)
    {
        BuffSystem.Tick(delta);
        CombatSystem.Tick(delta);
    }

    public override void End()
    {
        base.End();

        CombatSystem.OnDie -= HandleOnDie;

        if (hpBar != null)
        {
            hpBarPool.ReleaseObject(hpBar);
            hpBar = null;
        }
    }

    public bool TryGetEnemyInVision(out Entity enemy)
    {
        enemy = null;

        int obstacleLayers = wallLayer | groundLayer;

        var hit = Physics2D.Raycast(CenterPosition, Vector2.right * FacingDirection, VisionRange, CombatSystem.EnemyMask | obstacleLayers);
        if (hit.collider == null)
        {
            return false;
        }

        if (((1 << hit.collider.gameObject.layer) & obstacleLayers) != 0)
        {
            return false;
        }

        enemy = hit.collider.GetComponent<Entity>();

        return enemy != null;
    }

    public bool IsEnemyNearby(float detectionRadius, Entity checkEnemy)
    {
        int obstacleLayers = wallLayer | groundLayer;

        Vector2 dirToEnemy = (checkEnemy.CenterPosition - CenterPosition).normalized;

        var hit = Physics2D.Raycast(CenterPosition, dirToEnemy, detectionRadius, CombatSystem.EnemyMask);
        if (hit.collider == null || hit.collider.gameObject != checkEnemy.gameObject)
        {
            return false;
        }

        return true;
    }

    public bool IsEnemyTooHight(Entity checkEnemy)
    {
        float bodyHeight = BodySize.y;

        if (checkEnemy.transform.position.y > transform.position.y)
        {
            return checkEnemy.transform.position.y - transform.position.y > bodyHeight;
        }

        return false;
    }

    public bool IsEnemyTooLow(Entity checkEnemy)
    {
        float enemeyHeight = checkEnemy.BodySize.y;

        if (checkEnemy.transform.position.y < transform.position.y)
        {
            return transform.position.y - checkEnemy.transform.position.y > enemeyHeight;
        }

        return false;
    }

    public bool IsEnemyInAttackRange(Entity enemy)
    {
        float distanceToEnemy = Vector2.SqrMagnitude(CenterPosition - enemy.CenterPosition);
        return distanceToEnemy <= AttackRange * AttackRange;
    }

    protected override void OnDrawGizmos()
    {
        base.OnDrawGizmos();

        if (centerAnchor == null)
        {
            return;
        }

        Vector2 offset = new Vector2(0, 0.1f);
        Gizmos.color = Color.red;
        Gizmos.DrawLine(CenterPosition + offset, CenterPosition + offset + Vector2.right * FacingDirection * VisionRange);

        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(CenterPosition + offset, CenterPosition + offset + Vector2.right * FacingDirection * AttackRange);
    }
}