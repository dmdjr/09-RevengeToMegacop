# 프로젝트 아키텍처

## 플레이어 서브컨트롤러 패턴

`PlayerController`는 얇은 조율자 — 매 프레임 6개 서브컨트롤러의 `Update*`, `Handle*` 메서드를 호출한다.

| 서브컨트롤러 | 역할 |
|---|---|
| `PlayerStateController` | HP, 스태미나, 처형 게이지. C# 이벤트로 UI에 알림 |
| `PlayerMovementController` | WASD 이동, Shift 대시, 마우스 회전, 텔레포트 |
| `PlayerHitController` | Q/E 패리 & 가드 입력. `IDamageable.Hit()` 진입점 |
| `PlayerSwordController` | 1키 — 커서 방향으로 `SwordController` 프리팹 투척 |
| `PlayerShurikenController` | F키 — 첫 번째 누르면 수리검 투척, 두 번째 누르면 수리검 위치로 텔레포트 |
| `PlayerExecutionController` | LMB (게이지 충전 시) — 적을 즉사 처형, 플레이어가 적 위치로 이동 |

## 총알 & 데미지 시스템

- `Bullet`(abstract)이 매 프레임 전진, `destroyTime` 후 자동 파괴
- `Bullet.OnTriggerEnter` → `IDamageable.Hit(bullet)` 호출. 반사된 총알(`isReflected = true`)만 적에게 데미지
- `PlayerHitController.Hit()` 우선순위: 패리(타이밍 + 스태미나) → 가드(홀드 + 스태미나) → 피격
- **패리**: 총알을 마우스 커서 방향으로 반사 + 처형 게이지 증가
- **가드**: 총알을 랜덤 +-60도 각도로 반사 + 스태미나 소모

## 키 입력

| 키 | 동작 |
|---|---|
| WASD | 이동 |
| Left Shift | 대시 (2배속) |
| Q / E | 패리 / 가드 |
| F | 수리검 투척 / 수리검으로 텔레포트 |
| 1 | 검 투척 |
| LMB (게이지 충전 시) | 적 처형 |
