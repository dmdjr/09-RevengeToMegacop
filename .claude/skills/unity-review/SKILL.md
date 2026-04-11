# /unity-review — Unity C# 변경분 코드 리뷰

변경된 파일만 대상으로 Unity C# 코드를 리뷰한다. 전체 프로젝트 리뷰가 필요하면 `unity-code-architecture-reviewer` 에이전트를 사용할 것.

## 동적 컨텍스트

최근 커밋 변경 파일 목록:
!`git diff --name-only HEAD~1 -- '*.cs'`

Staged 변경 파일 목록:
!`git diff --cached --name-only -- '*.cs'`

Unstaged 변경 파일 목록:
!`git diff --name-only -- '*.cs'`

## 리뷰 대상 결정

1. `$ARGUMENTS`가 지정된 경우 → 해당 파일만 리뷰
2. `$ARGUMENTS`가 비어 있는 경우 → 위 동적 컨텍스트에서 `.cs` 파일 목록을 합산하여 리뷰 대상으로 사용
3. 변경 파일이 없으면 "리뷰할 변경 사항이 없습니다." 출력 후 종료

## 리뷰 절차

1. 대상 파일을 Read 도구로 읽는다.
2. 변경 파일이 의존하는 다른 파일이 있으면 읽되, 리뷰 대상에는 포함하지 않는다.
3. 아래 체크리스트 기준으로 리뷰한다.
4. 결과를 **메인 대화에 인라인으로 출력**한다 (파일 저장 없음).

## 리뷰 수정 시 검증 절차

리뷰에서 감지된 항목을 수정할 때는 반드시 아래 절차를 따른다:

1. 특정 패턴(예: `GetComponent` → `TryGetComponent`)을 수정할 때, 해당 패턴이 존재하는 **모든 파일**을 grep으로 먼저 목록화
2. 각 파일에서 해당 패턴의 **모든 위치**를 확인하고 함께 수정
3. 커밋 전에 grep으로 프로젝트 전체에 해당 패턴이 남아있지 않은지 최종 검증

## 체크리스트

### Critical (버그·크래시·심각한 성능 이슈)
- MonoBehaviour 라이프사이클 오용 (Awake/Start/Update/OnDestroy)
- Update 내 매 프레임 할당 (string concat, LINQ, new WaitForSeconds, GetComponent 미캐싱)
- Camera.main 매 프레임 호출
- 이벤트 구독 후 OnDestroy에서 미해제 → 메모리 누수
- Null reference 위험 (Inspector 미할당 가능성)
- IDamageable / Hit 흐름에서 데미지 미적용 등 로직 누락
- Unity 오브젝트에 `??`, `?.`, `??=`, `is null` 패턴 사용 금지 — UnityEngine.Object 커스텀 == 연산자 우회로 null 체크 실패 (UNT0007/0008/0023/0029)
- `Time.fixedDeltaTime`을 Update에서, `Time.deltaTime`을 FixedUpdate에서 사용 (UNT0004)
- `new`로 MonoBehaviour/ScriptableObject 인스턴스 생성 금지 (UNT0010/0011)
- 코루틴 반환값 무시 — StartCoroutine 없이 호출 (UNT0012)
- Transform에 Destroy/DestroyImmediate 호출 (UNT0030)
- `GameObject.isStatic` 런타임 사용 — 에디터 전용 프로퍼티 (UNT0040)

### Important (유지보수·확장성)
- 시스템 간 강결합 (이벤트/인터페이스 대신 직접 참조)
- 싱글톤 남용·God Object 패턴
- 오브젝트 풀링 미사용 (빈번한 Instantiate/Destroy)
- 코루틴 정리 누락
- 매직 넘버 (SerializeField 또는 const로 추출 필요)
- Unity 메시지 시그니처 또는 대소문자 오류 (예: oncollisionenter → OnCollisionEnter) (UNT0006/0033)
- 잘못되거나 불필요한 SerializeField 속성 (UNT0013)
- GetComponent에 Component/Interface가 아닌 타입 전달 (UNT0014)
- InitializeOnLoad/InitializeOnLoadMethod 초기화 속성의 잘못된 메서드 시그니처 또는 static 생성자 누락 (UNT0009/0015)
- MenuItem 속성이 non-static 메서드에 적용 (UNT0020)
- LoadAttribute 메서드 내 에셋 작업 (UNT0031)
- 자기호출 GetComponent에 RequireComponent 미사용 (UNT0039)

### Minor (스타일·네이밍·마이크로 최적화)
- 네이밍 컨벤션 위반 (CLAUDE.md 기준: private camelCase, public PascalCase, [SerializeField] private)
- 미사용 using 문
- 불필요한 public 필드 (SerializeField private으로 변경)
- `tag ==` 문자열 비교 대신 `CompareTag()` 사용 (UNT0002)
- position·rotation 개별 설정 대신 `SetPositionAndRotation` / `SetLocalPositionAndRotation` 사용 (UNT0022/0032)
- 벡터 계산보다 스칼라 계산 우선 (예: `v.x * 2f` vs `v * new Vector3(2,2,2)`) (UNT0024)
- `GetComponent` 대신 `TryGetComponent` 사용 — 불필요한 할당 제거 (UNT0026)
- 할당 발생하는 Physics API 대신 Non-Allocating 오버로드 사용 (예: `RaycastNonAlloc`) (UNT0028)
- `new WaitForSeconds` 반복 생성 — 캐싱 권장 (UNT0038)
- 반복 Animator 파라미터 접근에 `Animator.StringToHash` 사용 (UNT0041)
- 루프 내 Mesh 배열 프로퍼티 반복 접근 — 로컬 변수에 캐싱 (UNT0042)
- `gameObject.gameObject` 불필요한 간접 호출 (UNT0019)

## 출력 형식

```
## 코드 리뷰 결과

**대상 파일**: (파일 목록)

### Critical
- `파일명:라인` — 설명

### Important
- `파일명:라인` — 설명

### Minor
- `파일명:라인` — 설명

### 요약
(한 줄 요약)
```

항목이 없는 심각도 섹션은 "없음"으로 표시한다.
