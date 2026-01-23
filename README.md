# Unity RPG Project

2D 액션 RPG 게임 프로젝트입니다.

## 개발 환경

- **Unity Version:** 6000.0.62f1 (Unity 6)
- **Render Pipeline:** Universal Render Pipeline (URP)
- **Platform:** Windows

## 프로젝트 구조

```
Assets/
├── _Scripts/
│   ├── Common/          # 재사용 가능한 공통 시스템 (83개 파일)
│   └── Project/         # 게임 전용 구현 (137개 파일)
├── _Scenes/             # 게임 씬 (Title, Lobby, InGame)
├── _Prefabs/            # 프리팹
├── _Graphics/           # 그래픽 에셋
├── _Animations/         # 애니메이션
├── _ScriptableObject/   # 데이터 에셋
└── _TilePalette/        # 타일맵 팔레트
```

## 핵심 시스템

### 게임 매니저 (RPG.cs)
| 시스템 | 역할 |
|--------|------|
| UISystem | UI 관리 및 팝업 처리 |
| LevelSystem | 레벨 로딩 및 관리 |
| DialogueSystem | 대화 및 내러티브 |
| AudioSystem | 사운드 관리 |
| VFXSystem | 시각 효과 |
| UserDataSystem | 유저 데이터 저장/로드 |

### 캐릭터 시스템
- **Entity** - 모든 캐릭터의 기반 클래스
- **Player** - 11가지 상태를 가진 플레이어 (Idle, Move, Jump, Dash, Attack 등)
- **AICharacter** - StateMachine 기반 AI 행동

### 전투 시스템
- 물리/원소(Fire, Ice, Lightning) 데미지 계산
- 크리티컬, 방어력, 저항력 시스템
- 군중 제어 (Knockback, Airborne, Stun)

### 스탯 시스템
- 기본 스탯: Health, Strength, Agility, Intelligence, Vitality
- 공격 스탯: PhysicalDamage, CriticalRate, CriticalPower
- 원소 스탯: FireDamage, IceDamage, LightningDamage
- 방어 스탯: Armor, Evasion, 원소 저항

### 스킬 시스템
| 스킬 | 설명 |
|------|------|
| Dash | 이동 스킬 |
| Blink | 순간이동 |
| ThrowSword | 원거리 공격 |
| TimeShard | 설치형 트랩 |
| TimeEcho | 분신 소환 |
| MagicBallRain | 범위 마법 |

### 기타 시스템
- **인벤토리** - 슬롯 기반, 스택 가능 (최대 200슬롯)
- **장비** - 무기, 방어구, 악세서리 2개
- **버프** - Frozen, Burning, Electrified 등
- **퀘스트** - 단계 기반 진행

## 적 종류

- Slime
- Skeleton Knight
- Archer Elf
- Reaper
- Mage

## 사용된 패키지

- UniTask
- DOTween
- Cinemachine
- Input System
- Ink (대화 시스템)
- TextMesh Pro
- SerializeReference Extensions

## 디자인 패턴

- **State Pattern** - 플레이어/AI 행동 관리
- **Observer Pattern** - EventDispatcher를 통한 시스템 간 통신
- **Strategy Pattern** - SpawnPolicy 등
- **Object Pool** - HP바, 이펙트 등

## 라이선스

Private Project
