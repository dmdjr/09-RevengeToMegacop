# RevengeToMegacop 프로젝트 안내서 (GEMINI.md)

이 파일은 **RevengeToMegacop** 프로젝트의 구조, 개발 규칙 및 주요 시스템에 대한 정보를 제공하여 Gemini CLI 에이전트가 효율적으로 작업을 수행할 수 있도록 돕습니다.

---

## 1. 프로젝트 개요

**RevengeToMegacop**은 Unity 6 기반의 3D 탑다운 액션 게임입니다. 플레이어는 총, 투척 검, 텔레포트 수리검 등 다양한 무기를 사용하여 적들을 처치하며, 패링, 가드, 처형(Execution) 등의 핵심 메카닉을 포함하고 있습니다.

- **Unity 버전**: 6000.0.67f1 (Unity 6)
- **렌더링 파이프라인**: URP (Universal Render Pipeline)
- **주요 기술**: Unity Input System, C# (Global Namespace), Mesh Slicing
- **핵심 메카닉**: 탑다운 전투, 무기 교체, 패링/가드, 약화된 적 처형 및 슬라이싱 연출

---

## 2. 개발 및 빌드 가이드

### 빌드 및 실행
- **Unity Editor**: Unity Hub에서 프로젝트를 열고 `Assets/_Project/Scenes/SampleScene.unity` (또는 각 보스 테스트 씬)을 엽니다.
- **빌드**: `File > Build Settings`에서 타겟 플랫폼(Windows/Mac)을 선택하고 빌드합니다.

### 주요 폴더 구조
- `Assets/_Project/Scripts/Player`: 플레이어 이동, 상태, 전투 컨트롤러
- `Assets/_Project/Scripts/Enemy`: 일반 적, 엘리트 몹, 보스 시스템 및 스폰 로직
- `Assets/_Project/Scripts/Weapon`: 총, 수리검, 검 등 무기 시스템
- `Assets/_Project/Scripts/Skill`: 스킬 트리 및 성장 시스템
- `Assets/_Project/Scripts/Util`: 메쉬 슬라이싱, 카메라 쉐이크 등 유틸리티 로직

---

## 3. 개발 규칙 및 컨벤션

### 코드 스타일
- **네임스페이스**: 커스텀 네임스페이스를 사용하지 않고 **글로벌 네임스페이스**를 유지합니다.
- **언어**: C#을 사용하며, Unity 6의 최신 기능을 활용할 수 있습니다.
- **주석**: 코드의 의도와 복잡한 로직에 대해 명확한 주석을 작성합니다.

### 작업 단위 (Work Units) 및 충돌 방지
`WORK_UNITS.md` 파일에 정의된 대로 프로젝트는 독립적인 작업 단위(WU)로 분리되어 관리됩니다.
- **공유 허브 파일 수정 주의**: `PlayerController.cs`, `PlayerStateController.cs`, `IDamageable.cs` 등 여러 시스템이 의존하는 파일은 수정 시 컨플릭트 위험이 높으므로 주의가 필요합니다.
- **신규 기능 구현**: 기존 파일을 수정하기보다 **새 파일을 생성**하여 기능을 구현하고, 마지막에 연결하는 방식을 권장합니다.
- **이벤트 기반 연동**: 시스템 간 결합도를 낮추기 위해 C# 이벤트를 적극적으로 활용합니다.

---

## 4. 주요 시스템 상세

### 보스 프레임워크 (Boss Framework)
- `BossEnemy.cs`: 모든 보스의 기본 추상 클래스로 페이즈 전환, 패턴 순환, UI 연동을 관리합니다.
- `BossPattern.cs`: 개별 공격 패턴을 정의하는 기반 클래스입니다.
- `BossPhaseData.cs`: ScriptableObject를 통해 페이즈별 설정을 관리합니다.

### 엘리트 몹 (Elite Enemies)
- `EliteEnemy.cs`를 상속받아 고유한 패턴을 가진 엘리트 적들(`AgileRifleman`, `Disruptor`, `ShieldCharger` 등)이 구현되어 있습니다.

### 처형 및 슬라이싱 (Execution & Slicing)
- `MeshSlicer.cs`와 `ExecutionSliceEffect.cs`를 통해 처형 시 적의 모델을 물리적으로 절단하는 고품질 연출을 제공합니다.

---

## 5. 작업 시 참고 사항
- 작업 전 반드시 `CLAUDE.md`와 `WORK_UNITS.md`를 읽고 현재 진행 중인 작업 컨텍스트를 확인하세요.
- 새로운 시스템을 추가할 때는 기존의 `BossEnemy`나 `EliteEnemy`와 같은 프레임워크를 따르는지 검토하세요.
- `IDamageable` 인터페이스를 통해 모든 공격 및 피해 로직이 통합되어 있으므로, 새로운 무기나 적을 추가할 때 이를 준수해야 합니다.
