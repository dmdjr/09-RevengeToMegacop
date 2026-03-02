# /review — 변경분 코드 리뷰

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

## 체크리스트

### Critical (버그·크래시·심각한 성능 이슈)
- MonoBehaviour 라이프사이클 오용 (Awake/Start/Update/OnDestroy)
- Update 내 매 프레임 할당 (string concat, LINQ, new WaitForSeconds, GetComponent 미캐싱)
- Camera.main 매 프레임 호출
- 이벤트 구독 후 OnDestroy에서 미해제 → 메모리 누수
- Null reference 위험 (Inspector 미할당 가능성)
- IDamageable / Hit 흐름에서 데미지 미적용 등 로직 누락

### Important (유지보수·확장성)
- 시스템 간 강결합 (이벤트/인터페이스 대신 직접 참조)
- 싱글톤 남용·God Object 패턴
- 오브젝트 풀링 미사용 (빈번한 Instantiate/Destroy)
- 코루틴 정리 누락
- 매직 넘버 (SerializeField 또는 const로 추출 필요)

### Minor (스타일·네이밍·마이크로 최적화)
- 네이밍 컨벤션 위반 (CLAUDE.md 기준: private camelCase, public PascalCase, [SerializeField] private)
- 미사용 using 문
- 불필요한 public 필드 (SerializeField private으로 변경)

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
