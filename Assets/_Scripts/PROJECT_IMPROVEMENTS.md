# Unity RPG 프로젝트 개선점

## 1. 버그 수정 (Critical)

### 1.1 EntityStatSystem.cs - 잘못된 StatData 참조
**위치:** `EntityStatSystem.cs:177`

```csharp
// 현재 (버그)
var totalEvasion = new CombinedStat(totalCriticalRateEntry.StatData, Evasion, Agility);

// 수정
var totalEvasion = new CombinedStat(totalEvasionEntry.StatData, Evasion, Agility);
```

**영향:** TotalEvasion 스탯이 TotalCriticalRate와 같은 StatData를 사용하여 스탯 조회/표시 오류 발생 가능

---

### 1.2 EntityCombatSystem.cs - 오타
**위치:** `EntityCombatSystem.cs:65`

```csharp
// 현재 (오타)
private Coroutine nockbackCoroutine;

// 수정
private Coroutine knockbackCoroutine;
```

---

## 2. 아키텍처 개선

### 2.1 RPG.cs - God Class 문제
**문제:** RPG 클래스가 너무 많은 책임을 가지고 있음

**현재 상태:**
- 모든 시스템의 정적 접근점
- 데미지 계산 로직
- 버프 생성 로직
- 레벨 로딩 로직
- 체크포인트 관리
- 대화 생성

**개선 제안:**
```csharp
// DamageCalculator.cs (분리)
public static class DamageCalculator
{
    public static Damage Calculate(ICombatable attacker, ICombatable defender) { ... }
}

// BuffFactory.cs (분리)
public static class BuffFactory
{
    public static RPGBuff CreateFromDamage(ICombatable attacker, ICombatable defender, Damage damage) { ... }
}

// CheckpointManager.cs (분리)
public class CheckpointManager
{
    public Checkpoint GetLastCheckpoint(RPGLevel level, Player player) { ... }
    public void TeleportToCheckpoint(RPGLevel level, Player player) { ... }
}
```

---

### 2.2 원소 시스템 하드코딩 문제
**위치:** `RPG.cs:113-135`, `EntityCombatSystem.cs`

**현재 상태:**
```csharp
// 원소 타입이 하드코딩되어 확장이 어려움
if (attacker.StatSystem.FireDamage.FinalValue > elementalDamage) { ... }
if (attacker.StatSystem.IceDamage.FinalValue > elementalDamage) { ... }
if (attacker.StatSystem.LightningDamage.FinalValue > elementalDamage) { ... }
```

**개선 제안:**
```csharp
// ElementalDamageData.cs
[CreateAssetMenu]
public class ElementalDamageData : ScriptableObject
{
    public ElementType Type;
    public StatData DamageStat;
    public StatData ResistanceStat;
    public Func<float, float, ICombatable, RPGBuff> BuffFactory;
}

// 데이터 주도 원소 시스템
public class ElementalDamageSystem
{
    private List<ElementalDamageData> elementTypes;

    public (ElementType, float, float) GetDominantElement(ICombatable attacker, ICombatable defender)
    {
        return elementTypes
            .Select(e => (e.Type, attacker.StatSystem.GetStat(e.DamageStat), defender.StatSystem.GetStat(e.ResistanceStat)))
            .OrderByDescending(x => x.Item2)
            .First();
    }
}
```

---

### 2.3 Player와 AICharacter 중복 코드
**문제:** 두 클래스에서 거의 동일한 초기화 및 시스템 연결 코드가 반복됨

**중복 코드 예시:**
```csharp
// Player.cs:131-136과 AICharacter.cs:83-88 동일
CombatSystem.MaxHealth = StatSystem.TotalHealth.FinalValue;
CombatSystem.HealthRegeneration = StatSystem.HealthRegeneration.FinalValue;
CombatSystem.SetHealthToMax();
StatSystem.TotalHealth.OnStatChanged += () => CombatSystem.MaxHealth = StatSystem.TotalHealth.FinalValue;
StatSystem.HealthRegeneration.OnStatChanged += () => CombatSystem.HealthRegeneration = StatSystem.HealthRegeneration.FinalValue;
```

**개선 제안:**
```csharp
// Entity 또는 별도 헬퍼 클래스에서 공통 로직 처리
public static class EntitySystemBinder
{
    public static void BindStatToCombat(EntityStatSystem stat, EntityCombatSystem combat)
    {
        combat.MaxHealth = stat.TotalHealth.FinalValue;
        combat.HealthRegeneration = stat.HealthRegeneration.FinalValue;
        combat.SetHealthToMax();

        stat.TotalHealth.OnStatChanged += () => combat.MaxHealth = stat.TotalHealth.FinalValue;
        stat.HealthRegeneration.OnStatChanged += () => combat.HealthRegeneration = stat.HealthRegeneration.FinalValue;
    }
}
```

---

## 3. 성능 개선

### 3.1 GetLastCheckpoint - 매번 컴포넌트 검색
**위치:** `RPG.cs:64`

**현재 상태:**
```csharp
foreach (var checkpoint in level.transform.GetComponentsInChildren<Checkpoint>())
```

**개선 제안:**
```csharp
// RPGLevel에서 체크포인트 캐싱
public class RPGLevel : Level
{
    private Dictionary<string, Checkpoint> checkpointCache;

    protected override void Awake()
    {
        base.Awake();
        CacheCheckpoints();
    }

    private void CacheCheckpoints()
    {
        checkpointCache = GetComponentsInChildren<Checkpoint>()
            .ToDictionary(c => c.CheckpointID);
    }

    public Checkpoint GetCheckpoint(string checkpointID)
    {
        return checkpointCache.TryGetValue(checkpointID, out var cp) ? cp : null;
    }
}
```

---

### 3.2 TeleportPlayerToPortal - FindObjectsByType 사용
**위치:** `RPG.cs:94`

**현재 상태:**
```csharp
var allPortals = GameObject.FindObjectsByType<Portal>(FindObjectsSortMode.None);
```

**개선 제안:**
```csharp
// RPGLevel에서 포탈 등록 관리
public class RPGLevel : Level
{
    private List<Portal> registeredPortals = new();

    public void RegisterPortal(Portal portal) => registeredPortals.Add(portal);
    public void UnregisterPortal(Portal portal) => registeredPortals.Remove(portal);
    public Portal GetFirstPortal() => registeredPortals.FirstOrDefault();
}
```

---

### 3.3 EntityStatSystem.Awake() - 반복 코드 최적화
**위치:** `EntityStatSystem.cs:96-226`

**현재 상태:** 20개 이상의 스탯을 개별적으로 초기화하는 반복 코드

**개선 제안:**
```csharp
// 리플렉션 또는 SerializedField 배열 활용
[Serializable]
struct StatConfig
{
    public StatData StatData;
    public float BaseValue;
}

[SerializeField] private StatConfig[] statConfigs;

private void Awake()
{
    foreach (var config in statConfigs)
    {
        stats[config.StatData] = new Stat(config.StatData, config.BaseValue);
    }

    InitializeCombinedStats();
}
```

---

## 4. 안전성 개선

### 4.1 이벤트 구독 해제 누락
**위치:** `Player.cs:135-136`, `AICharacter.cs:87-88, 96`

**문제:** Begin()에서 이벤트를 구독하지만 End()에서 해제하지 않음

**현재 상태:**
```csharp
// Begin()에서 구독
StatSystem.TotalHealth.OnStatChanged += () => CombatSystem.MaxHealth = StatSystem.TotalHealth.FinalValue;

// End()에서 해제 없음!
```

**개선 제안:**
```csharp
private Action onTotalHealthChanged;
private Action onHealthRegenChanged;

public override void Begin()
{
    onTotalHealthChanged = () => CombatSystem.MaxHealth = StatSystem.TotalHealth.FinalValue;
    onHealthRegenChanged = () => CombatSystem.HealthRegeneration = StatSystem.HealthRegeneration.FinalValue;

    StatSystem.TotalHealth.OnStatChanged += onTotalHealthChanged;
    StatSystem.HealthRegeneration.OnStatChanged += onHealthRegenChanged;
}

public override void End()
{
    StatSystem.TotalHealth.OnStatChanged -= onTotalHealthChanged;
    StatSystem.HealthRegeneration.OnStatChanged -= onHealthRegenChanged;
}
```

---

### 4.2 Null 체크 보강
**위치:** 여러 곳

**개선이 필요한 코드:**
```csharp
// RPG.cs:167-168 - currentLevel이 null일 경우 처리 없음
var currentLevel = LevelSys.CurrentLevel as RPGLevel;
return currentLevel.NearestTownLevelID; // NullReferenceException 가능

// 개선
var currentLevel = LevelSys.CurrentLevel as RPGLevel;
if (currentLevel == null)
{
    Logger.Warn("Current level is not RPGLevel");
    return default;
}
return currentLevel.NearestTownLevelID;
```

---

## 5. 유지보수성 개선

### 5.1 매직 넘버 상수화
**위치:** `EntityStatSystem.cs`, `EntityCombatSystem.cs`

**현재 상태:**
```csharp
float vitalityBonus = statList[1].FinalValue * 10;      // 매직 넘버
float agilityBonus = statList[1].FinalValue * 0.3f;     // 매직 넘버
float strengthBonus = statList[1].FinalValue * 0.5f;    // 매직 넘버
```

**개선 제안:**
```csharp
// StatConstants.cs
public static class StatConstants
{
    public const float VitalityToHealthRatio = 10f;
    public const float AgilityToCritRateRatio = 0.3f;
    public const float StrengthToCritPowerRatio = 0.5f;
    public const float AgilityToEvasionRatio = 0.5f;
    public const float IntelligenceToResistanceRatio = 0.5f;
}

// 또는 ScriptableObject로 설정 가능하게
[CreateAssetMenu]
public class StatScalingConfig : ScriptableObject
{
    public float VitalityToHealthRatio = 10f;
    public float AgilityToCritRateRatio = 0.3f;
    // ...
}
```

---

### 5.2 BuffSystem - ID 기반 접근 개선
**위치:** `BuffSystem.cs`

**문제:** Buff ID가 uint로 되어 있어 어떤 버프인지 파악이 어려움

**개선 제안:**
```csharp
// BuffID enum 정의
public enum BuffID : uint
{
    None = 0,
    Frozen = 1,
    Burning = 2,
    Electrified = 3,
    // ...
}

// 사용 시
public bool HasFrozen() => HasBuff((uint)BuffID.Frozen);
```

---

## 6. 확장성 개선

### 6.1 스킬 입력 처리 일반화
**위치:** `Player.cs:183-213`

**현재 상태:** 각 스킬마다 별도의 핸들러 메서드

```csharp
private void HandlePlantShardInput() { ... }
private void HandleSpawnEchoInput() { ... }
// 스킬 추가 시 새 메서드 필요
```

**개선 제안:**
```csharp
// SkillInputBinding.cs
[Serializable]
public class SkillInputBinding
{
    public InputAction InputAction;
    public string SkillTypeName;
}

// Player.cs
[SerializeField] private SkillInputBinding[] skillBindings;

private void HandleSkillInputs()
{
    foreach (var binding in skillBindings)
    {
        if (binding.InputAction.triggered)
        {
            TryUseSkillByType(binding.SkillTypeName);
        }
    }
}
```

---

### 6.2 CombinedStat 계산 함수 데이터화
**위치:** `EntityStatSystem.cs`

**문제:** 복합 스탯 계산 로직이 코드에 하드코딩됨

**개선 제안:**
```csharp
// CombinedStatData.cs - ScriptableObject
[CreateAssetMenu]
public class CombinedStatData : StatData
{
    public StatData[] SourceStats;
    public string CalculationFormula; // "base + vitality * 10" 형태의 수식
}

// 또는 델리게이트 매핑
public class StatCalculations
{
    public static readonly Dictionary<StatData, Func<IReadOnlyList<Stat>, float>> Formulas = new()
    {
        { StatDataRegistry.TotalHealth, stats => stats[0].FinalValue + stats[1].FinalValue * 10 },
        // ...
    };
}
```

---

## 7. 코드 품질 개선

### 7.1 인터페이스 활용 강화
**문제:** ICombatable 인터페이스가 있지만 일부 기능만 포함

**개선 제안:**
```csharp
public interface ICombatable
{
    EntityCombatSystem CombatSystem { get; }
    EntityStatSystem StatSystem { get; }
    BuffSystem BuffSystem { get; }  // 추가

    void TakeDamage(Damage damage);
    void ApplyBuff(Buff buff);
}

public interface IMovable
{
    float MoveSpeed { get; }
    Vector2 MovementAxis { get; set; }
    int FacingDirection { get; }
    void Move(Vector2 direction);
}
```

---

### 7.2 접근 제한자 정리
**위치:** `AICharacter.cs:7-10`

```csharp
// 현재 - public 필드
public Transform HpBarAnchor;
public float VisionRange = 5f;
public float AttackRange = 1.5f;

// 개선 - SerializeField + 프로퍼티
[SerializeField] private Transform hpBarAnchor;
[SerializeField] private float visionRange = 5f;
[SerializeField] private float attackRange = 1.5f;

public Transform HpBarAnchor => hpBarAnchor;
public float VisionRange => visionRange;
public float AttackRange => attackRange;
```

---

## 8. 우선순위 요약

| 우선순위 | 항목 | 이유 |
|---------|------|------|
| **1 (긴급)** | EntityStatSystem 버그 수정 | 게임 밸런스에 직접 영향 |
| **2 (높음)** | 이벤트 구독 해제 | 메모리 누수 및 예외 발생 가능 |
| **3 (높음)** | Null 체크 보강 | 런타임 크래시 방지 |
| **4 (중간)** | RPG.cs 분리 | 유지보수성 향상 |
| **5 (중간)** | 성능 최적화 (캐싱) | 프레임 드롭 방지 |
| **6 (낮음)** | 매직 넘버 상수화 | 가독성 및 밸런스 조정 용이 |
| **7 (낮음)** | 원소 시스템 데이터화 | 확장성 향상 |

---

*작성일: 2026-01-23*
