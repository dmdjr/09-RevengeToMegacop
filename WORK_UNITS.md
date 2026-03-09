# 팀 개발을 위한 작업 단위 분리 계획

## Context
팀을 모아 RevengeToMegacop을 계속 개발하기 위해, Git 컨플릭트를 최소화하면서 병렬 작업이 가능하도록 TODO 항목들을 독립적인 작업 단위(Work Unit)로 분리한다.

## 컨플릭트 최소화 전략

### 공유 허브 파일 (수정 시 주의 필요)
아래 파일들은 여러 시스템이 의존하므로, 수정 시 컨플릭트가 발생하기 쉽다:
- `IDamageable.cs` — 데미지 인터페이스 (현재 `Hit(Bullet)` 하나)
- `PlayerController.cs` — 플레이어 서브컨트롤러 코디네이터
- `PlayerStateController.cs` — HP/스태미나/게이지 상태 + 이벤트

**원칙**: 기존 파일 수정을 최소화하고, 새 기능은 **새 파일**에 구현한다. 기존 파일에 연결하는 코드는 작업 마지막에 한 번만 추가한다.

---

## 작업 단위 (Work Units)

### WU-1. 엘리트몹 구현
**담당 인원**: 1명
**신규 파일**:
- `EliteEnemy.cs` (Enemy 상속, 더 높은 HP, 특수 패턴)

**수정 파일**:
- `EnemySpawner.cs` — 엘리트몹 스폰 로직 추가 (단, WU-3과 병합 시점 조율 필요)

**의존**: `Enemy.cs` (상속, 읽기만)
**충돌 위험**: ★★☆☆☆ (EnemySpawner 수정 시 WU-3과 조율)

---

### WU-2A. 보스 프레임워크 (선행 작업)
**담당 인원**: 1명
**설명**: 모든 보스가 공유하는 기반 시스템을 구축한다. 페이즈 전환, 패턴 순환, 보스 UI 등 공통 로직을 추상 클래스로 제공하여 각 보스 담당자가 고유 로직만 구현하도록 한다.

**신규 파일**:
- `BossEnemy.cs` — 모든 보스의 추상 기반 클래스 (Enemy 상속, 페이즈 전환, 패턴 순환 루프)
- `BossPattern.cs` — 개별 공격 패턴의 추상 기반 클래스 (`Execute()`, `CanExecute()`)
- `BossPhaseData.cs` — ScriptableObject, 페이즈별 설정 (HP 임계값, 속도 배율 등)
- `BossUI.cs` — 보스 HP바, 이름 표시 등 공통 UI
- 범용 패턴 예시: `CircularBulletPattern.cs`, `DashPattern.cs` 등 여러 보스가 재사용할 수 있는 공통 패턴

**수정 파일**: 없음

**의존**: `Enemy.cs` (상속, 읽기만), `Weapon.cs` 계열
**충돌 위험**: ★☆☆☆☆ (신규 파일만 생성)

**BossEnemy가 제공하는 공통 기능**:
- HP 비율 기반 자동 페이즈 전환 (`OnDamaged` → `OnPhaseChanged`)
- 패턴 큐 관리 및 실행 루프 (`ExecuteNextPattern`)
- 등장/사망 연출 훅 (`OnBossIntro`, `OnBossDeath`)
- BossUI 자동 연동

**각 보스가 구현할 추상/가상 메서드**:
- `OnPhaseChanged(int phaseIndex)` — 페이즈 전환 시 고유 연출
- `GetPatternsForPhase(int phaseIndex)` — 페이즈별 패턴 목록 반환
- `OnBossIntro()` — 등장 연출 (선택)
- `OnBossDeath()` — 사망 연출 (선택)

---

### WU-2B. 1스테이지 보스
**담당 인원**: 1명
**신규 파일**:
- `Stage1Boss.cs` (BossEnemy 상속, 1스테이지 보스 고유 로직)
- `Stage1Boss*Pattern.cs` — 이 보스만의 고유 패턴들

**수정 파일**: 없음
**의존**: `BossEnemy.cs` (WU-2A, 상속)
**충돌 위험**: ★☆☆☆☆ (신규 파일만 생성, 다른 보스 WU와 파일 겹침 없음)
**선행 조건**: WU-2A 완료

---

### WU-2C. 2스테이지 보스
**담당 인원**: 1명
**신규 파일**:
- `Stage2Boss.cs` (BossEnemy 상속, 2스테이지 보스 고유 로직)
- `Stage2Boss*Pattern.cs` — 이 보스만의 고유 패턴들

**수정 파일**: 없음
**의존**: `BossEnemy.cs` (WU-2A, 상속)
**충돌 위험**: ★☆☆☆☆ (신규 파일만 생성, 다른 보스 WU와 파일 겹침 없음)
**선행 조건**: WU-2A 완료

---

### WU-2D. 3스테이지 보스
**담당 인원**: 1명
**신규 파일**:
- `Stage3Boss.cs` (BossEnemy 상속, 3스테이지 보스 고유 로직)
- `Stage3Boss*Pattern.cs` — 이 보스만의 고유 패턴들

**수정 파일**: 없음
**의존**: `BossEnemy.cs` (WU-2A, 상속)
**충돌 위험**: ★☆☆☆☆ (신규 파일만 생성, 다른 보스 WU와 파일 겹침 없음)
**선행 조건**: WU-2A 완료

> **보스 WU 운영 규칙**: 각 보스 담당자가 공통 기능이 필요하면 직접 구현하지 말고 WU-2A 담당자에게 프레임워크 확장을 요청(PR)한다. 이를 통해 코드 중복을 방지한다.

---

### WU-3. 스테이지 시스템 + 페이즈 기반 스폰
**담당 인원**: 1명
**신규 파일**:
- `GameManager.cs` — 전체 게임 흐름 관리 (싱글톤)
- `StageManager.cs` — 스테이지 진행, 페이즈 전환 관리
- `StageData.cs` — ScriptableObject, 스테이지별 설정 (적 구성, 페이즈 정의)

**수정 파일**:
- `EnemySpawner.cs` — StageManager와 연동하도록 스폰 로직 리팩터링

**의존**: `Enemy.cs`, `EnemySpawner.cs` (기존 스폰 로직 이해 필요)
**충돌 위험**: ★★☆☆☆ (EnemySpawner 수정 시 WU-1과 조율)

> **WU-1 ↔ WU-3 조율 방안**: WU-3이 EnemySpawner 리팩터링을 먼저 완료한 뒤 WU-1이 엘리트몹 스폰 로직을 추가하거나, EnemySpawner를 분리하여 `SpawnConfig` 등의 데이터 구조를 먼저 합의한다.

---

### WU-4. 스킬 트리 / 성장 시스템
**담당 인원**: 1명
**신규 파일**:
- `SkillTreeManager.cs` — 스킬 해금/적용 관리
- `SkillData.cs` — ScriptableObject, 스킬 정의
- `CurrencyManager.cs` — 재화 관리
- `SkillTreeUI.cs` — 스킬 트리 UI

**수정 파일**:
- `PlayerStateController.cs` — 적 처치 시 회복 스킬 효과 적용 (이벤트 리스너 추가, 최소 수정)
- `Enemy.cs` — 사망 시 재화 드롭 이벤트 발행 (OnDeath 이벤트 활용, 최소 수정)

**의존**: `PlayerStateController.cs` (이벤트 구독), `Enemy.cs` (OnDeath 이벤트 구독)
**충돌 위험**: ★★☆☆☆ (대부분 신규 파일, 기존 파일은 이벤트 구독만)

---

### WU-5. 게임 플로우 UI (타이틀/게임오버/클리어)
**담당 인원**: 1명
**신규 파일**:
- `TitleScreenUI.cs`
- `GameOverUI.cs`
- `StageClearUI.cs`
- `SceneTransitionManager.cs`

**수정 파일**: 없음 (GameManager와 연동하지만, 이벤트 기반으로 느슨하게 연결)

**의존**: `GameManager.cs` (WU-3에서 생성, 이벤트 구독)
**충돌 위험**: ★☆☆☆☆ (완전히 독립적인 신규 파일들)
**선행 조건**: WU-3의 GameManager 인터페이스가 먼저 정의되어야 함

---

### WU-6. 사운드 매니저
**담당 인원**: 1명
**신규 파일**:
- `AudioManager.cs` — BGM/SFX 재생 싱글톤
- `SoundData.cs` — ScriptableObject, 사운드 클립 매핑

**수정 파일**: 없음 (각 시스템에서 AudioManager.Instance.Play() 호출만 추가 — 이것은 각 WU 담당자가 자기 코드에서 추가)

**충돌 위험**: ★☆☆☆☆ (완전 독립)

---

### WU-7. 처형 퍼센트 데미지 + 이펙트 강화
**담당 인원**: 1명
**신규 파일**:
- `ExecutionEffect.cs` — 처형 시 시각 이펙트

**수정 파일**:
- `PlayerExecutionController.cs` — 엘리트몹 대상 퍼센트 데미지 분기 추가

**의존**: `EliteEnemy.cs` (WU-1), `Enemy.cs`
**충돌 위험**: ★☆☆☆☆ (PlayerExecutionController만 수정, 다른 WU와 겹치지 않음)
**선행 조건**: WU-1 (엘리트몹)이 먼저 완료되어야 의미 있음

---

### WU-8. 세이브/로드 시스템
**담당 인원**: 1명
**신규 파일**:
- `SaveManager.cs` — JSON 기반 저장/로드
- `SaveData.cs` — 저장 데이터 구조체

**수정 파일**: 없음 (다른 매니저들이 SaveManager를 호출)

**충돌 위험**: ★☆☆☆☆ (완전 독립)

---

## 작업 순서 및 의존관계 맵

```
Phase 1 (병렬 가능 — 서로 완전 독립):
  WU-2A 보스 프레임워크
  WU-6  사운드 매니저
  WU-8  세이브/로드

Phase 2 (Phase 1 완료 후 or 병렬 가능하되 조율 필요):
  WU-2B 1스테이지 보스  ┐
  WU-2C 2스테이지 보스  ├── WU-2A 이후, 서로 병렬 (컨플릭트 0)
  WU-2D 3스테이지 보스  ┘
  WU-3  스테이지 + 스폰 시스템 (EnemySpawner 수정)
  WU-4  스킬 트리

Phase 3 (선행 WU 필요):
  WU-1  엘리트몹        ← WU-3 이후 (EnemySpawner 안정화 후)
  WU-5  게임 플로우 UI   ← WU-3 이후 (GameManager 필요)
  WU-7  처형 퍼센트 데미지 ← WU-1 이후 (엘리트몹 필요)
```

## 컨플릭트 가능 파일 요약

| 파일 | 수정하는 WU | 조율 방법 |
|------|------------|----------|
| `EnemySpawner.cs` | WU-1, WU-3 | WU-3 먼저 → WU-1 후속 |
| `BossEnemy.cs` | WU-2A (소유), WU-2B~2D (읽기만) | WU-2A 담당자만 수정, 나머지는 상속만 |
| `PlayerExecutionController.cs` | WU-7 | 단독 수정 |
| `PlayerStateController.cs` | WU-4 | 단독 수정 (이벤트 추가) |

**결론**: 대부분의 WU가 서로 다른 파일을 수정하므로 컨플릭트 위험이 매우 낮다. `EnemySpawner.cs`는 작업 순서로 해결하고, `BossEnemy.cs`는 WU-2A 담당자가 단독 소유하므로 보스 간 컨플릭트가 발생하지 않는다.
