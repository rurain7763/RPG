using UnityEngine;
using UnityEngine.InputSystem;

public class Player : Entity, ICombatable, ISkillUser, IHasInventory
{
    private static readonly int YVelocityAnimHash = Animator.StringToHash("yVelocity");

    [SerializeField] private LayerMask interactableLayer;

    public AnimationEventReciever AnimationEventReciever { get; private set; }
    public Rigidbody2D Rigidbody { get; private set; }
    public PlayerInputSystem InputSystem { get; private set; }
    public TargetDetector2D TargetDetector { get; private set; }
    public EntityCombatSystem CombatSystem { get; private set; }
    public EntityStatSystem StatSystem { get; private set; }
    public EntityVFXSystem VFXSystem { get; private set; }
    public EntitySFXSystem SFXSystem { get; private set; }
    public BuffSystem BuffSystem { get; private set; }
    public QuestSystem QuestSystem { get; private set; }
    public EntitySkillSystem SkillSystem { get; private set; }
    public InventorySystem InventorySystem { get; private set; }
    public EquipmentSystem EquipmentSystem { get; private set; }
    public EntityQuickItemSystem QuickItemSystem { get; private set; }
    public PlayLevelSystem PlayLevelSystem { get; private set; }

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

    public bool BlockInput
    {
        get => blockInput;
        set
        {
            blockInput = value;
            if (blockInput)
            {
                MovementAxis = Vector2.zero;
                InputSystem.Disable();
            }
            else
            {
                InputSystem.Enable();
            }
        }
    }

    public ulong UserID { get; private set; }

    private bool blockInput;
    private StateMachine stateMachine;

    protected override void Awake()
    {
        base.Awake();

        AnimationEventReciever = GetComponentInChildren<AnimationEventReciever>();

        Rigidbody = GetComponent<Rigidbody2D>();

        InputSystem = new PlayerInputSystem();

        TargetDetector = GetComponentInChildren<TargetDetector2D>();

        CombatSystem = GetComponent<EntityCombatSystem>();

        StatSystem = GetComponent<EntityStatSystem>();

        VFXSystem = GetComponent<EntityVFXSystem>();

        SFXSystem = GetComponent<EntitySFXSystem>();

        BuffSystem = new BuffSystem(this);

        SkillSystem = GetComponent<EntitySkillSystem>();

        stateMachine = new StateMachine();
        stateMachine.AddState<PlayerIdle>(this);
        stateMachine.AddState<PlayerMove>(this);
        stateMachine.AddState<PlayerJump>(this);
        stateMachine.AddState<PlayerFall>(this);
        stateMachine.AddState<PlayerWallSlide>(this);
        stateMachine.AddState<PlayerWallJump>(this);
        stateMachine.AddState<PlayerDash>(this);
        stateMachine.AddState<PlayerBasicAttack>(this);
        stateMachine.AddState<PlayerJumpAttack>(this);
        stateMachine.AddState<PlayerCounterAttack>(this);
        stateMachine.AddState<PlayerThrowSword>(this);
        stateMachine.AddState<PlayerDead>(this);
        stateMachine.AddGlobalTransition<PlayerDead>(() => CombatSystem.IsDead, 0);
        stateMachine.SetAsEntryState<PlayerIdle>();
    }

    private void OnEnable()
    {
        InputSystem.Enable();

        InputSystem.Player.Movement.performed += ctx => MovementAxis = ctx.ReadValue<Vector2>();
        InputSystem.Player.Movement.canceled += ctx => MovementAxis = Vector2.zero;
    }

    private void OnDisable()
    {
        InputSystem.Disable();
    }

    public override void Begin()
    {
        base.Begin();

        PlayLevelSystem.OnLevelChanged += HandleLevelChanged;

        SkillSystem.Begin();

        StatSystem.TotalHealth.OnStatChanged += () => CombatSystem.MaxHealth = StatSystem.TotalHealth.FinalValue;
        StatSystem.HealthRegeneration.OnStatChanged += () => CombatSystem.HealthRegeneration = StatSystem.HealthRegeneration.FinalValue;

        stateMachine.Begin();
    }

    public override void Tick(float delta)
    {
        base.Tick(delta);

        if (!BlockInput)
        {
            HandleInteractInput();
            HandleSkillInput<TimeShard>(InputSystem.Player.PlantShard);
            HandleSkillInput<TimeEcho>(InputSystem.Player.SpawnEcho);
            HandleSkillInput<DomainExpansion>(InputSystem.Player.DomainExpansion);
            HandleQuickItemSlotInput();
        }

        BuffSystem.Tick(delta);
        CombatSystem.Tick(delta);
        SkillSystem.Tick(delta);
        stateMachine.Tick();

        Animator.SetFloat(YVelocityAnimHash, Rigidbody.linearVelocity.y);
    }

    public override void End()
    {
        base.End();

        SkillSystem.End();

        PlayLevelSystem.OnLevelChanged -= HandleLevelChanged;
    }

    private void HandleLevelChanged()
    {
        if (SkillSystem.LinkedSkillSystem != null)
        {
            SkillSystem.LinkedSkillSystem.SkillPoints += 1;
        }

        RPG.VFXSys.SpawnVFX(Local.GetVFXPath(VFXID.LevelUp), transform);
        RPG.AudioSys.PlaySFX(Local.GetSFXPath(SFXID.LevelUp), transform.position);
    }

    private void HandleInteractInput()
    {
        if (!InputSystem.Player.Interact.triggered)
        {
            return;
        }

        var hit = Physics2D.OverlapCircle(CenterPosition, 1.0f, interactableLayer);
        if (hit != null)
        {
            var interactable = hit.GetComponent<IInteractable>();
            interactable?.Interact(this);
        }
    }

    private void HandleSkillInput<T>(InputAction inputAction) where T : RPGSkill
    {
        if (!inputAction.triggered)
        {
            return;
        }

        if (!SkillSystem.TryGetSkill<T>(out var skill) || !skill.CanUse(gameObject))
        {
            return;
        }

        skill.SetToCooldown();
        skill.Use(gameObject);
    }

    private void HandleQuickItemSlotInput()
    {
        if (InputSystem.Player.QuickItemSlot_1.triggered)
        {
            QuickItemSystem.TryUseQuickItem(1);
        }
        if (InputSystem.Player.QuickItemSlot_2.triggered)
        {
            QuickItemSystem.TryUseQuickItem(2);
        }
    }

    public void Possess(ulong userID)
    {
        var playData = RPG.UserDataSys.GetTable<UserPlayDataTable>(userID);

        PlayLevelSystem = playData.PlayLevelSys;

        CombatSystem.MaxHealth = StatSystem.TotalHealth.FinalValue;
        CombatSystem.CurrentHealth = playData.Progress.RemainHealth;
        CombatSystem.HealthRegeneration = StatSystem.HealthRegeneration.FinalValue;

        InventorySystem = playData.Inventory;

        EquipmentSystem = playData.Equipment;
        EquipmentSystem.Begin(this);

        QuestSystem = playData.QuestSystem;
        QuestSystem.Begin();

        QuickItemSystem = new EntityQuickItemSystem(this);

        SkillSystem.LinkedSkillSystem = playData.SkillSys;

        UserID = userID;
    }

    public void Unpossess()
    {
        SkillSystem.LinkedSkillSystem = null;

        QuestSystem.End();
        EquipmentSystem.End();
        SkillSystem.End();

        UserID = 0;
    }

    public bool IsLocalPlayer()
    {
        return RPG.UserDataSys.CurrentUUID == UserID;
    }
}