# Feel 에셋 분석 (2026-04-08)

Feel (More Mountains) 에셋 구조와 활용법 분석. RevengeToMegacop 탑다운 액션 게임 기준.

---

## 1. MMFeedbacks — 게임 피드백/연출 시스템

### 아키텍처

```
MMF_Player (MonoBehaviour)        ← GameObject에 부착하는 컨테이너
  └─ List<MMF_Feedback>           ← 피드백 목록 (SerializeReference)
       ├─ MMF_CameraShake
       ├─ MMF_FreezeFrame
       ├─ MMF_Sound
       └─ ...

Shaker (MonoBehaviour)            ← 씬 오브젝트에 부착 (카메라, 라이트 등)
  └─ Channel                      ← 어떤 Player의 이벤트를 받을지 라우팅
```

- **MMF_Player** (신규): SerializeReference 기반, 컴포넌트 없이 순수 C# 객체로 피드백 관리
- **MMFeedbacks** (레거시): MonoBehaviour 기반. 여전히 동작하며 API 동일

### 핵심 API

```csharp
// 필드 선언
public MMFeedbacks HitFeedback;      // 레거시 (여전히 사용 가능)
public MMF_Player HitFeedback;       // 신규 권장

// 재생
HitFeedback?.PlayFeedbacks();
HitFeedback?.PlayFeedbacks(position, feedbacksIntensity);  // 위치 + 강도(0~1+)
yield return HitFeedback?.PlayFeedbacksCoroutine(position, intensity);

// 역방향 재생
HitFeedback?.PlayFeedbacksInReverse();

// 정지/리셋
HitFeedback?.StopFeedbacks();
HitFeedback?.ResetFeedbacks();
HitFeedback?.RestoreInitialValues();  // 초기 상태로 복원

// 새로 생성된 오브젝트 (풀링 시 중요)
HitFeedback?.Initialization();  // 첫 PlayFeedbacks() 전 반드시 호출

// 상태 확인
bool isPlaying = HitFeedback.IsPlaying;  // 재생 중 여부 (게이팅에 활용)

// 런타임 추가
myPlayer.AddFeedback(new MMF_Scale());
MMF_Feedback fb = myPlayer.AddFeedback(typeof(MMF_CameraShake));
```

### 피드백 타입 목록

#### Camera (화면 연출)
| 타입 | 설명 | 활용 |
|------|------|------|
| `MMF_CameraShake` | 카메라 흔들림 | 피격, 폭발 |
| `MMF_CameraZoom` | FOV 변경 | 처형 줌인 |
| `MMF_Flash` | 화면 플래시 | 패리, 피격 |
| `MMF_Fade` | 화면 페이드 | 씬 전환 |
| `MMF_CameraFieldOfView` | FOV 직접 제어 | 달리기 |
| `MMF_CinemachineImpulse` | Cinemachine 임펄스 | 카메라 충격 |

#### Time (시간 조작)
| 타입 | 설명 | 활용 |
|------|------|------|
| `MMF_FreezeFrame` | 히트스톱 (수 프레임 정지) | 처형, 강타 |
| `MMF_TimescaleModifier` | 타임스케일 변경 + 자동복원 | 슬로모션 |

#### Transform (오브젝트 변형)
| 타입 | 설명 | 활용 |
|------|------|------|
| `MMF_PositionShake` | 위치 흔들림 | 총 반동 |
| `MMF_RotationShake` | 회전 흔들림 | 피격 |
| `MMF_ScaleShake` | 크기 흔들림 | 피격 임팩트 |
| `MMF_Scale` | 크기 보간 | 방패 스프링 |
| `MMF_Position` | 위치 보간 | 넉백 |
| `MMF_SquashAndStretch` | 스쿼시 앤 스트레치 | 점프/착지 |
| `MMF_PositionSpring` | Spring 기반 위치 | 부드러운 반응 |

#### PostProcess (후처리)
| 타입 | 설명 | 활용 |
|------|------|------|
| `MMF_Bloom_URP` | 블룸 강도 | 처형 연출 |
| `MMF_Vignette_URP` | 비네트 | 피격 UI |
| `MMF_ChromaticAberration_URP` | 색수차 | 강한 충격 |
| `MMF_ColorAdjustments_URP` | 색조 보정 | 슬로모션 연출 |
| `MMF_LensDistortion_URP` | 렌즈 왜곡 | 폭발 |

#### Renderer (머티리얼/렌더러)
| 타입 | 설명 | 활용 |
|------|------|------|
| `MMF_Flicker` | 렌더러 깜빡임 | 피격 무적 |
| `MMF_MaterialSetProperty` | 머티리얼 프로퍼티 | 아웃라인, 히트 |
| `MMF_Light` | 라이트 강도/색상 | 머즐 플래시 |

#### Audio
| 타입 | 설명 |
|------|------|
| `MMF_Sound` | 단순 AudioClip 재생 |
| `MMF_MMSoundManagerSound` | MMSoundManager 통한 재생 (풀링, 트랙 관리) |

#### Particles/VFX
| 타입 | 설명 |
|------|------|
| `MMF_Particles` | ParticleSystem Play/Stop |
| `MMF_ParticlesInstantiation` | 파티클 인스턴스 생성 |

#### Animation
| 타입 | 설명 |
|------|------|
| `MMF_Animation` | Animator 파라미터 설정 |
| `MMF_AnimationCrossfade` | CrossFade 트리거 |

#### GameObject
| 타입 | 설명 |
|------|------|
| `MMF_SetActive` | 오브젝트 활성화/비활성화 |
| `MMF_InstantiateObject` | 프리팹 인스턴스 생성 |

#### Events / Flow
| 타입 | 설명 |
|------|------|
| `MMF_Events` | UnityEvent 호출 |
| `MMF_MMGameEvent` | MMEventManager 이벤트 브로드캐스트 |
| `MMF_Pause` | 시퀀스 일시정지 (ms) |
| `MMF_Looper` | 피드백 루프 |
| `MMF_Feedbacks` | 다른 MMF_Player 재생 |

#### UI
| 타입 | 설명 |
|------|------|
| `MMF_FloatingText` | 데미지 숫자 팝업 |
| `MMF_Image` | Image 색상/알파 보간 |
| `MMF_CanvasGroup` | CanvasGroup 알파 |

#### Haptics (모바일)
| 타입 | 설명 |
|------|------|
| `MMF_HapticPreset` | 내장 프리셋 진동 |
| `MMF_HapticEmphasis` | 커스텀 강도 진동 |
| `MMF_HapticClip` | 커스텀 .haptic 파일 |

### Shaker 설정 방법

Camera Shake, Flash, Fade 등 Shaker 기반 피드백은:
1. 카메라(또는 대상 오브젝트)에 Shaker 컴포넌트 부착 (예: `MMCameraShaker`)
2. Feedback과 Shaker의 **Channel**을 동일하게 맞춤
3. 피드백 재생 시 이벤트가 Shaker로 전달됨

---

## 2. MMTools — 범용 유틸리티

### MMEventManager (이벤트 버스)

```csharp
// 이벤트 정의 (struct)
public struct PlayerDiedEvent { public int score; }

// 발행
PlayerDiedEvent.Trigger(score: 1000);

// 구독 (OnEnable/OnDisable에서)
void OnEnable()  => this.MMEventStartListening<PlayerDiedEvent>();
void OnDisable() => this.MMEventStopListening<PlayerDiedEvent>();
void OnMMEvent(PlayerDiedEvent e) { /* 처리 */ }
```

GC 없는 struct 기반. 인스펙터용: `MMGameEventListener` 컴포넌트.

### MMObjectPooler (오브젝트 풀링)

```csharp
// MMSimpleObjectPooler — 단일 프리팹
var bullet = pooler.GetPooledGameObject();
bullet.SetActive(true);
// 반환: bullet.SetActive(false) 또는 MMPoolableObject.LifeTime 자동 반환

// MMMultipleObjectPooler — 다수 프리팹
var obj = multiPooler.GetPooledGameObject();

// MMPoolableObject 컴포넌트 — pooled object에 부착
// LifeTime 설정 시 자동 비활성화
```

### AI Brain (적 AI)

```
AIBrain
  └─ AIState[] States
       ├─ AIAction[] Actions      ← 매 프레임 실행 (이동, 사격, 대기...)
       └─ AITransition[] Transitions
            ├─ AIDecision Decision ← true/false 반환 (거리 감지, 타이머...)
            ├─ TrueState (string)
            └─ FalseState (string)
```

```csharp
// 커스텀 Action
public class AIActionChasePlayer : AIAction {
    public override void PerformAction() {
        // _brain.Target 추적 이동
    }
}

// 커스텀 Decision
public class AIDecisionPlayerInRange : AIDecision {
    public float Range = 10f;
    public override bool Decide() {
        return Vector3.Distance(transform.position, _brain.Target.position) < Range;
    }
}
```

### MMStateMachine

```csharp
public enum CharacterStates { Idle, Moving, Attacking, Stunned, Dead }

public MMStateMachine<CharacterStates> MovementState;

void Start() {
    MovementState = new MMStateMachine<CharacterStates>(gameObject, triggerEvents: true);
}

void Update() {
    MovementState.ChangeState(CharacterStates.Moving);
    if (MovementState.CurrentState == CharacterStates.Dead) { ... }
}
```

### MMTimeManager (히트스톱)

```csharp
// 이벤트 방식
MMTimeScaleEvent.Trigger(MMTimeScaleMethods.For, 0.05f, 0.1f, true, 1f, false);
// (method, timeScale, duration, lerp, lerpSpeed, unscaled)

MMFreezeFrameEvent.Trigger(0.05f);  // 0.05초 프리즈
```

### MMHealthBar (월드스페이스 체력바)

컴포넌트를 적에게 부착, Inspector에서 설정:
- `ForegroundBar`, `DelayedBar` 연결
- `BumpScaleOnChange`: 피격 시 바 펄스 효과
- `HideBar`: 자동 숨김 타이머

```csharp
_healthBar.UpdateBar(currentHealth, 0, maxHealth, bump: true);
```

### MMCooldown

```csharp
[SerializeField] private MMCooldown _dashCooldown;  // Inspector에서 설정

void TryDash() {
    if (!_dashCooldown.Ready()) return;
    _dashCooldown.Start();
    // 대시 실행
}
```

### MMObservable

```csharp
public MMObservable<float> Health = new MMObservable<float>();

void Start() {
    Health.OnValueChanged += (newVal) => UpdateHealthUI(newVal);
}

void TakeDamage(float dmg) {
    Health.Value -= dmg;  // 자동으로 OnValueChanged 발행
}
```

### MMLootTable

```csharp
var lootTable = GetComponent<MMLootTableGameObject>();
lootTable.ComputeWeights();
GameObject drop = lootTable.GetLoot().Loot;
Instantiate(drop, transform.position, Quaternion.identity);
```

### MMConeOfVision2D (적 탐지)

컴포넌트 부착, Inspector 설정:
- `VisionRadius`, `VisionAngle`
- `ObstacleMask`, `TargetMask`

```csharp
// 매 프레임 또는 일정 주기로 호출
_coneOfVision.ScanForTargets();
if (_coneOfVision.VisibleTargets.Count > 0) {
    _brain.Target = _coneOfVision.VisibleTargets[0].transform;
}
```

### MMMaths 주요 함수

```csharp
MMMaths.Remap(value, fromMin, fromMax, toMin, toMax);  // 범위 재매핑
MMMaths.Chance(criticalPercent);                        // 확률 판정 (bool)
MMMaths.Spring(currentValue, targetValue, ref velocity, damping, frequency, deltaTime);
MMMaths.DirectionFromAngle2D(angle, angleOffset);       // 각도 → 방향벡터
```

### MMFollowTarget (카메라 추적)

컴포넌트를 카메라에 부착:
- `Target`: 추적 대상
- `InterpolationType`: RegularLerp / MMLerp / MMSpring
- `FollowPositionX/Y/Z`: 축별 추적 on/off
- `Offset`: 오프셋 포지션

### MMSingleton

```csharp
public class GameManager : MMSingleton<GameManager> {
    public int Score;
}

// 어디서든
GameManager.Instance.Score += 100;
```

---

## 3. NiceVibrations (모바일 햅틱)

```csharp
// 씬에 HapticReceiver 컴포넌트 1개 필수

// 프리셋
HapticPatterns.PlayPreset(HapticPatterns.PresetType.HeavyImpact);
// Selection, Success, Warning, Failure, LightImpact, MediumImpact, HeavyImpact, RigidImpact, SoftImpact

// 커스텀 단발 (amplitude: 0~1, frequency: 0~1)
HapticPatterns.PlayEmphasis(0.8f, 0.5f);

// 연속 진동
HapticPatterns.PlayConstant(amplitude, frequency, duration);

// 실시간 변조
HapticController.clipLevel = newAmplitude;
HapticController.clipFrequencyShift = newFrequency;
HapticController.Stop();

// 마스터 토글
HapticController.hapticsEnabled = false;
```

무기별 .haptic 샘플: `Assets/Feel/NiceVibrations/HapticSamples/Weapons/`
- PistolLoad1-3, ShotgunFire1-2, ShotgunLoad1-3, HeavyMachineGunFire1-2 등

---

## 4. RevengeToMegacop 적용 시나리오

### 타격감 구성 (피드백 조합)

```
피격:
  MMF_CameraShake        (흔들림)
  MMF_FreezeFrame        (히트스톱 2~4프레임)
  MMF_Flicker            (피격 깜빡임)
  MMF_Vignette_URP       (비네트 순간 강화)
  MMF_Sound              (피격음)
  MMF_HapticEmphasis     (모바일 진동)

처형:
  MMF_TimescaleModifier  (0.2x 슬로모션 0.5초)
  MMF_CameraZoom         (줌인 + 복원)
  MMF_ChromaticAberration_URP
  MMF_Bloom_URP
  MMF_Sound

총 발사:
  MMF_PositionShake      (반동)
  MMF_Particles          (머즐 플래시)
  MMF_Light              (머즐 라이트)
  MMF_Sound

패리/가드 성공:
  MMF_FreezeFrame        (강한 히트스톱)
  MMF_Flash              (화면 플래시)
  MMF_Scale              (방패 스프링 반응)
  MMF_Sound              (패리음)

대시:
  MMF_PositionShake
  MMF_Flicker
  MMF_Sound
```

### 시스템 적용

| 기능 | 사용 시스템 |
|------|------------|
| 총알/이펙트 풀링 | `MMSimpleObjectPooler` + `MMPoolableObject` |
| 적 AI (패트롤→추격→공격) | `AIBrain` + `AIAction` + `AIDecision` |
| 플레이어 감지 | `MMConeOfVision2D` |
| 적 HP바 | `MMHealthBar` |
| 스킬 쿨타임 | `MMCooldown` |
| HP/탄약 UI 반응 | `MMObservable<float>` |
| 적 드롭 | `MMLootTableGameObject` |
| 게임 전역 이벤트 | `MMEventManager` (struct 이벤트) |
| 씬 전환 | `MMFader` + `MMLoadScene` |
| 세이브/로드 | `MMSaveLoadManager` |

### 패트롤 AI 구성 예시

```
AIBrain
  States:
    "Patrol"
      Actions: [AIActionPatrol (MMPath 기반)]
      Transitions:
        AIDecisionPlayerInRange(10f) → True: "Chase"
    "Chase"
      Actions: [AIActionMoveTowardsTarget]
      Transitions:
        AIDecisionPlayerInRange(3f)  → True: "Attack"
        AIDecisionPlayerInRange(10f) → False: "Patrol"
    "Attack"
      Actions: [AIActionShoot]
      Transitions:
        AIDecisionPlayerInRange(3f)  → False: "Chase"
```
