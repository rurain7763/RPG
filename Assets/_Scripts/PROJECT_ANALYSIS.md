# Unity RPG 프로젝트 분석서

## 개요

- **총 파일 수:** 220개 C# 파일
- **구조:** Common(공통 시스템) + Project(게임 전용 구현)

---

## 1. 프로젝트 구조

```
_Scripts/
├── Common/                 (83개 파일 - 재사용 가능한 시스템 및 유틸리티)
│   ├── Experimental/       (13개 파일 - GAS 영감 어빌리티 시스템)
│   └── [코어 시스템들]
└── Project/                (137개 파일 - 게임 전용 구현)
    ├── Controller/         (2개 파일)
    ├── Data/               (13개 파일 - 게임 데이터 정의)
    ├── Level/              (5개 파일 - 레벨 관리)
    ├── Quest/              (2개 파일 - 퀘스트 로직)
    ├── Skill/              (25개 파일 - 스킬 구현)
    ├── System/             (2개 파일 - 프로젝트 전용 시스템)
    ├── UI/                 (34개 파일 - UI 컴포넌트)
    └── [캐릭터/적/NPC 클래스들]
```

---

## 2. 핵심 시스템

### 2.1 중앙 관리 시스템 (RPG.cs)

`RPG` 클래스가 `AppManager`를 상속하여 모든 시스템에 대한 정적 접근을 제공합니다.

| 시스템 | 역할 |
|--------|------|
| UISystem | UI 관리 및 팝업 처리 |
| ScreenEffectSystem | 화면 전환 및 이펙트 |
| ResourcesSystem | 에셋 로딩 |
| VFXSystem | 시각 효과 |
| AudioSystem | 사운드 관리 |
| LevelSystem | 레벨 로딩 및 관리 |
| DialogueSystem | 대화 및 내러티브 |
| AppDataSystem | 게임 데이터 관리 |
| UserDataSystem | 유저 데이터 저장/로드 |
| EventDispatcher | 이벤트 버스 (디커플링) |

### 2.2 캐릭터 및 전투 시스템

#### Entity (Entity.cs)
모든 캐릭터의 기반 클래스:
- 물리 감지 (지면, 벽, 절벽)
- 이동 및 방향 전환
- 애니메이션 및 스프라이트 렌더링

#### Player (Player.cs)
Entity를 확장한 플레이어 클래스:

**11가지 상태:**
- Idle, Move, Jump, Fall, WallSlide, WallJump
- Dash, BasicAttack, JumpAttack, CounterAttack
- ThrowSword, Dead

**하위 시스템:**
- EntityCombatSystem (전투)
- EntityStatSystem (스탯)
- EntityVFXSystem / EntitySFXSystem (효과)
- BuffSystem (버프/디버프)
- InventorySystem (인벤토리)
- EquipmentSystem (장비)
- EntitySkillSystem (스킬)
- EntityQuickItemSystem (퀵슬롯)

#### AICharacter (AICharacter.cs)
Entity를 확장한 AI 캐릭터:
- 시야 및 공격 범위 감지
- StateMachine 기반 AI 행동
- 적 유형별 상태 구현 (Slime, SkeletonKnight, ArcherElf, Reaper, Mage)

### 2.3 전투 시스템 (EntityCombatSystem.cs)

**데미지 계산:**
```
물리 데미지 = 기본 데미지 * (1 - 방어력/(방어력+100))
원소 데미지 = 기본 데미지 * (1 - 저항력)
크리티컬 = 데미지 * 크리티컬 배율
```

**원소 타입:** Fire, Ice, Lightning

**군중 제어:** Knockback, Airborne, Stun (면역 플래그 지원)

### 2.4 스탯 시스템 (EntityStatSystem.cs, ModifiableValue.cs)

#### 스탯 카테고리

| 분류 | 스탯 |
|------|------|
| 기본 | Health, HealthRegen, Strength, Agility, Intelligence, Vitality |
| 공격 | PhysicalDamage, CriticalRate, CriticalPower, ArmorReduction |
| 원소 | FireDamage, IceDamage, LightningDamage |
| 방어 | Armor, Evasion, FireResistance, IceResistance, LightningResistance |
| 기타 | MoveSpeed, AttackSpeed |

#### 수정자 파이프라인 (우선순위)

```
Override (100) → Clamp (200) → Multiply (300) →
Category Total Multiply (400) → Total Multiply (500) →
Max Multiply (600) → Min Multiply (700) → Add (800)
```

### 2.5 인벤토리 & 장비 시스템

#### InventorySystem
- 슬롯 기반, 스택 가능
- 트랜잭션 범위 (배치 작업)
- 최대: 플레이어 200슬롯, 창고 500슬롯

#### EquipmentSystem (4슬롯)
| 슬롯 | 타입 |
|------|------|
| Weapon | 무기 |
| Armor | 방어구 |
| FirstTrinket | 악세서리 1 |
| SecondTrinket | 악세서리 2 |

### 2.6 스킬 시스템

#### 스킬 기본 구조
- 쿨다운 관리
- 시퀀스 기반 실행 (다단계 스킬)
- 32비트 플래그 업그레이드 시스템

#### 구현된 스킬
| 스킬 | 설명 |
|------|------|
| Dash | 이동 스킬 + 업그레이드 |
| Blink | 순간이동 |
| Retreat | 방어적 이동 |
| ThrowSword | 원거리 공격 + 모듈 |
| TimeShard | 설치형 트랩/유틸리티 |
| TimeEcho | 분신 메커닉 |
| MagicBallRain | 범위 마법 |
| SpellDeathThunder | 번개 공격 |

### 2.7 버프 시스템 (BuffSystem.cs, RPGBuffs.cs)

**버프 속성:**
- 지속시간 (무한 또는 시간제)
- 스택 카운트 관리
- 라이프사이클: OnApply → OnTick → OnExpire

**구현된 버프:**
| 버프 | 효과 |
|------|------|
| Frozen | 이동 불가 (얼음 저항으로 감소) |
| Burning | 도트 데미지 (화염 저항으로 감소) |
| Electrified | 충전 메커닉 (번개 저항 연동) |

### 2.8 퀘스트 시스템 (QuestSystem.cs)

- 단계 기반 진행
- 퀘스트 정책: Unique (일회성) / Repeatable (반복)
- DTO 직렬화로 저장/로드 지원

### 2.9 레벨 시스템 (RPGLevel.cs)

- 엔티티 등록/해제
- 플레이어 빙의 시스템
- 체크포인트 관리
- 스폰 정책 적용

### 2.10 UI 시스템 (UISystem.cs)

**2계층 아키텍처:**
- StaticUI: 지속 UI 요소 (HUD, 메뉴)
- PopupUI: 모달 다이얼로그

**주요 UI 컴포넌트 (34개 파일):**
- PlayerUI, SkillTreeUI, QuestUI
- BlacksmithUI, MerchantUI, DialogueUI
- OptionUI, DeathUI, ItemTooltip
- CombatText 등

---

## 3. 클래스 관계도

```
┌─────────────────────────────────────────────────────────────┐
│                        RPG (AppManager)                      │
│  ┌─ UISystem          ┌─ LevelSystem      ┌─ EventDispatcher │
│  ├─ AudioSystem       ├─ DialogueSystem   ├─ ResourcesSystem │
│  ├─ VFXSystem         └─ UserDataSystem   └─ AppDataSystem   │
│  └─ ScreenEffectSys                                          │
└─────────────────────────────────────────────────────────────┘
              │
       ┌──────┴──────┐
       │             │
    Player        AICharacter
       │             │
       └──────┬──────┘
              │
         Entity (Base)
            ├─ CombatSystem
            ├─ StatSystem
            ├─ BuffSystem
            ├─ VFXSystem / SFXSystem
            ├─ EntitySkillSystem
            ├─ InventorySystem (Player 전용)
            ├─ EquipmentSystem (Player 전용)
            └─ QuickItemSystem (Player 전용)
```

---

## 4. 사용된 디자인 패턴

### 아키텍처 패턴
| 패턴 | 적용 위치 |
|------|----------|
| Manager/Singleton | RPG 클래스 |
| Strategy | SpawnPolicy 인터페이스 |
| Observer | EventDispatcher |
| State | StateMachine (Player/AI 행동) |

### 데이터 패턴
| 패턴 | 적용 위치 |
|------|----------|
| DTO | QuestSystemDTO, SkillSystemDTO |
| ScriptableObject | ItemData, QuestData, StatData |
| Table | UserPlayDataTable, RPGDataTable |

### 행동 패턴
| 패턴 | 적용 위치 |
|------|----------|
| Sequence | 다단계 스킬 실행 |
| Composite | CombinedStat |
| Decorator | ValueModifier 시스템 |
| Chain of Responsibility | 스탯 수정자 체인 |
| Transaction | InventorySystem.BeginTransaction() |

### 성능 패턴
| 패턴 | 적용 위치 |
|------|----------|
| Object Pool | HpBarPool |
| Bitfield | UIntFlagContainer32 (스킬 업그레이드) |
| Lazy Evaluation | ModifiableValue |
| LinkedList | BuffSystem |

---

## 5. 시스템 간 의존성

### 직접 의존성
```
Player
 ├─→ EntityCombatSystem
 ├─→ EntityStatSystem
 ├─→ BuffSystem
 ├─→ EquipmentSystem → StatModifiers
 ├─→ InventorySystem
 └─→ EntitySkillSystem → LinkedSkillSystem

EntityCombatSystem ──→ StatSystem (데미지 계산)
UISystem ──→ ResourcesSystem (프리팹 로딩)
RPGLevel ──→ Player, LevelEnvironment
```

### 이벤트 기반 의존성 (EventDispatcher 통해)
```
RPGLevel ←── HealthChanged, Die (from CombatSystem)
UISystem ←── OnInventoryChanged (from InventorySystem)
UISystem ←── OnEquipmentChanged (from EquipmentSystem)
UISystem ←── OnStatChanged (from StatSystem)
```

### 스탯 계산 의존성
```
TotalHealth ← Health + Vitality
PhysicalDamage ← Damage + Strength
ElementalDamage ← FireDamage + IceDamage + LightningDamage + Intelligence
TotalCriticalRate ← CritRate + Agility
TotalEvasion ← Evasion + Agility
TotalArmor ← Armor + Vitality
```

---

## 6. 주요 인터페이스

```csharp
public interface ICombatable
{
    EntityCombatSystem CombatSystem { get; }
    EntityStatSystem StatSystem { get; }
}

public interface ISkillUser
{
    EntitySkillSystem SkillSystem { get; }
}

public interface IHasInventory
{
    InventorySystem InventorySystem { get; }
}

public interface IInteractable
{
    void Interact(Entity entity);
}
```

---

## 7. 데이터 흐름 예시

### 데미지 계산 흐름
```
1. Attacker → RPG.CalcDamage(attacker, defender)
2. attacker.StatSystem에서 물리 데미지 추출
3. 크리티컬 확률 체크 → 크리티컬 배율 적용
4. 원소 타입 결정 (Fire/Ice/Lightning)
5. 방어력 감소 적용: damage * (1 - armor/(armor+100))
6. 원소 저항 적용: damage * (1 - resistance)
7. Damage 객체 반환
8. Defender가 CombatSystem.TakeDamage()로 데미지 수신
```

### 장비 변경 흐름
```
1. UI 또는 코드에서 아이템 장착 요청
2. EquipmentSystem.EquipWeapon(item) 호출
3. 기존 장비 해제 (스탯 수정자 제거)
4. 새 장비 장착 (스탯 수정자 추가)
5. ModifiableValue가 최종 스탯 재계산
6. OnEquipmentChanged 이벤트 발생
7. UI가 스탯 변경 반영
```

---

## 8. 주요 상수 및 설정

| 항목 | 값 | 설명 |
|------|-----|------|
| 방어력 상수 | 100f | 데미지 계산 밸런스용 |
| 플레이어 인벤토리 | 200 슬롯 | 최대 보유 가능 |
| 창고 슬롯 | 500 슬롯 | 최대 보관 가능 |
| 스킬 업그레이드 플래그 | 32비트 | 스킬당 최대 32개 업그레이드 |

---

## 9. 아키텍처 특징

1. **계층 분리:** Common(재사용 가능) vs Project(게임 전용)
2. **이벤트 기반 통신:** EventDispatcher로 시스템 간 디커플링
3. **컴포넌트 기반 설계:** Entity에 시스템들을 조합
4. **데이터 주도 설계:** ScriptableObject와 CSV 기반 데이터 정의
5. **확장 가능한 스킬/버프:** 모듈 시스템과 비트플래그 업그레이드

---

*분석일: 2026-01-23*
