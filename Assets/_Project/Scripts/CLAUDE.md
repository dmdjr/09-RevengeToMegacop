# Unity C# 코드 컨벤션

이 파일을 새 Unity 프로젝트의 `Assets/_Project/Scripts/CLAUDE.md`에 복사하여 사용한다.

---

## 네이밍

- `private` 필드: `camelCase`
- public 메서드/타입: `PascalCase`
- Inspector 노출 필드: `[SerializeField] private` — `public` 필드 금지
- **변수명 축약 금지** — 모든 변수(루프, 임시, 로컬 포함)에 단어 전체 표기
  - `sc` → `skillController`, `e` → `enemy`, `go` → `gameObject`
- 한 파일에 한 클래스, 파일명 = 클래스명

## Unity 메시지 메서드

- `Awake`, `Start`, `Update`, `OnDestroy` 등 Unity 예약 함수에는 `private` 접근 제한자를 **붙이지 않는다** (일반 private 메서드와 시각적 구별 목적)

## using 정리

- `System` usings → `UnityEngine` usings 순서
- 사용하지 않는 `using`은 반드시 제거

## 상속 설계

- `new` 키워드로 부모 메서드를 숨기지 말 것 → 부모를 `virtual`로 수정하고 자식에서 `override`
- 부모 필드/프로퍼티를 자식에서 중복 정의 금지 → 부모의 멤버를 `protected`로 재사용
- 명시적 인터페이스 재구현(`void IFoo.Method()`) 금지 → 부모가 이미 구현했으면 `override` 사용

## 이벤트 수명 관리

이벤트 구독 해제를 기계적으로 추가하지 말 것. 먼저 수명 관계를 판단한다:
- 이벤트 소스가 구독자보다 먼저 파괴 → 해제 불필요
- 구독자가 소스보다 먼저 파괴될 수 있음 → 해제 필요
- 둘이 항상 함께 파괴 (같은 씬, 같은 GameObject) → 해제 불필요

## try-finally

"반드시 실행"이 요구되는 코드(콜백, 상태 복원, 리소스 해제)는 `try` 블록이 아닌 **`finally` 블록**에 넣는다. `try` 내부 마지막 줄은 예외 시 실행이 보장되지 않는다.

## Unity 오브젝트 null 체크

- `?.`, `??`, `??=`, `is null` 패턴 금지 — `UnityEngine.Object`의 커스텀 `==` 연산자를 우회하여 null 체크 실패 (UNT0007/0008)
- 반드시 `if (obj != null)` 또는 `if (obj == null)` 사용

## 코드 청결

- 사용하지 않는 변수, 프로퍼티 등 dead code는 전부 제거
- 매직 넘버 지양 → `[SerializeField]` 또는 `const`로 추출

## 문서화

- 베이스/프레임워크 코드에는 XML doc comment(`/// <summary>`) 추가
- 한국어, API 사용 관점, `[필수 구현]` vs `[선택 override]` 명시

## 작업 방식

- 한 번에 너무 많은 기능을 구현하지 말 것 — 점진적이고 통제된 단위로 작업
- 작업 단위를 명확히 분리 — Git 충돌 최소화를 위해 기존 공유 파일 수정보다 새 파일 생성 선호

## 오답노트

### `GetComponentInChildren` vs `TryGetComponent` 혼동 금지

- `TryGetComponent<T>()` — **자기 자신 GameObject**에서만 탐색 (`GetComponent`와 동일 범위, 할당 없음)
- `GetComponentInChildren<T>()` — 자기 자신 + **모든 자식**에서 탐색
- 리뷰 체크리스트의 "GetComponent → TryGetComponent" 권장은 **자식 탐색이 불필요한 경우**에만 적용한다.
  Renderer 등 컴포넌트가 자식 오브젝트에 붙는 경우가 흔하므로, 대체 전에 반드시 탐색 범위를 확인할 것.
