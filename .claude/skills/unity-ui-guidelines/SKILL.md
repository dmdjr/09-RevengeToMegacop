---
name: unity-ui-guidelines
description: Unity UI 가이드라인. 트리거 조건 — UI 작성/생성 요청("메뉴 만들어", "HUD 추가", "버튼 배치", "Canvas 생성", "UI 씬 만들어", "화면 구성"), UI 리뷰/점검 요청("UI 리뷰", "UI 점검", "UI 최적화"), UI 수정 요청("버튼 위치 조정", "레이아웃 변경", "UI 수정").
metadata:
  author: project
  version: "2.0.0"
  argument-hint: <file-or-pattern>
---

# Unity UI Guidelines

Unity uGUI(Canvas + UnityEngine.UI + TextMeshPro) 기반 UI를 작성하거나 리뷰할 때 아래 규칙을 적용한다.

## 사용 방법

1. 인자로 받은 파일(또는 패턴)을 읽는다. 인자가 없으면 사용자에게 파일을 묻는다.
2. 아래 모든 규칙에 대해 위반 여부를 검사한다.
3. 위반 사항을 `file:line — [카테고리] 설명` 형식으로 출력한다.
4. 위반이 없으면 "위반 없음"으로 출력한다.

씬 편집(execute_csharp 등)으로 UI를 생성할 때도 아래 규칙을 준수하여 코드를 작성한다.

## UI 작성 시 시각적 검증 루프

**UI를 새로 만들거나 수정할 때 반드시 아래 루프를 따른다.**

### 캡처 방법

Unity CLI의 `execute_csharp`로 Game 뷰 스크린샷을 캡처한다:

```csharp
// usings: ['UnityEditor']
var gameView = EditorWindow.GetWindow(
    typeof(EditorWindow).Assembly.GetType("UnityEditor.GameView"));
gameView.Focus();
gameView.Repaint();
ScreenCapture.CaptureScreenshot("D:/unity/RevengeToMegacop/ui_screenshot.png");
return "captured";
```

캡처 후 `Read` 도구로 이미지를 열어 시각적으로 분석한다.

### 검증 루프

```
1. UI 배치 (execute_csharp로 씬에 요소 생성/수정)
2. 스크린샷 캡처 (위 코드 실행)
3. Read 도구로 이미지 분석
   - 요소 간 정렬이 일관적인가?
   - 여백/간격이 균일한가?
   - 시각적 계층구조가 명확한가? (타이틀 > 본문 > 부가 정보)
   - 텍스트 가독성이 충분한가?
   - 버튼/인터랙티브 요소가 명확히 구분되는가?
   - 화면 내 요소 배치가 균형 잡혀 있는가? (한쪽 쏠림 없음)
4. 문제 발견 시 → 수정 후 2번으로 돌아감
5. 문제 없으면 → 완료
```

### 수치적 검증 (보조)

스크린샷과 함께 `execute_csharp`로 RectTransform 데이터를 추출하여 수치 검증을 병행할 수 있다:

```csharp
// usings: ['UnityEditor', 'TMPro']
var result = new System.Text.StringBuilder();
foreach (var rt in GameObject.FindObjectsByType<RectTransform>(FindObjectsSortMode.None)) {
    if (rt.parent != null && rt.parent.GetComponent<Canvas>() != null) {
        result.AppendLine(rt.name
            + " pos=" + rt.anchoredPosition
            + " size=" + rt.sizeDelta
            + " anchor=(" + rt.anchorMin + "," + rt.anchorMax + ")");
    }
}
return result.ToString();
```

확인 항목:
- 동일 그룹 요소 간 간격이 일정한가 (anchoredPosition 차이값 비교)
- 버튼 sizeDelta가 최소 48×48을 충족하는가 ([AC-03])
- 앵커가 논리적 위치에 맞는가 ([LO-04])

---

## 규칙

### Canvas 설정

- **[CS-01]** CanvasScaler는 반드시 `ScaleWithScreenSize` 모드를 사용한다.
- **[CS-02]** referenceResolution은 프로젝트 기준 해상도(기본 1920×1080)로 설정한다.
- **[CS-03]** matchWidthOrHeight는 가로 게임이면 0.5~1, 세로 게임이면 0~0.5로 설정한다.
- **[CS-04]** 용도가 다른 UI는 별도 Canvas로 분리한다 (HUD, 팝업, 월드스페이스 등).
- **[CS-05]** 자주 갱신되는 요소(HP 바, 타이머)와 정적 요소(라벨, 테두리)는 서브 Canvas로 분리하여 Canvas rebuild 비용을 줄인다.

### 레이아웃

- **[LO-01]** 동적 개수의 자식 요소는 LayoutGroup(Horizontal/Vertical/Grid)을 사용한다. 수동 좌표 배치 금지.
- **[LO-02]** LayoutGroup의 `childForceExpand`는 의도적으로 설정한다. 기본값(true)이 예상치 못한 늘림을 유발한다.
- **[LO-03]** ContentSizeFitter와 LayoutGroup을 같은 GameObject에 함께 사용하지 않는다. 레이아웃 루프가 발생할 수 있다.
- **[LO-04]** 앵커는 요소의 논리적 위치에 맞춘다 (HP 바 → 좌상단, 미니맵 → 우상단 등).
- **[LO-05]** 화면 크기에 따라 늘어나야 하는 컨테이너는 stretch 앵커 + 오프셋을 사용한다. 고정 sizeDelta 금지.

### 성능

- **[PF-01]** 상호작용하지 않는 Image/Text의 `Raycast Target`은 반드시 비활성화한다.
- **[PF-02]** alpha=0으로 숨긴 Image는 여전히 드로우콜을 소비한다. 숨길 때는 `CanvasGroup.alpha = 0` + `blocksRaycasts = false` 또는 컴포넌트 비활성화를 사용한다.
- **[PF-03]** UI 스프라이트는 Sprite Atlas로 묶어 드로우콜을 줄인다.
- **[PF-04]** 자주 생성/파괴되는 UI 요소(데미지 넘버, 알림 등)는 오브젝트 풀링을 사용한다.
- **[PF-05]** LayoutGroup 내부 요소의 RectTransform을 매 프레임 변경하지 않는다. 레이아웃 리빌드가 비싸다.
- **[PF-06]** 같은 Sprite Atlas의 요소는 Hierarchy에서 인접하게 배치한다. 다른 아틀라스 요소가 중간에 끼면 배치가 깨진다.
- **[PF-07]** Frame Debugger(`Window > Analysis > Frame Debugger`)로 배치 카운트를 확인한다. `Canvas.RenderSubBatch` 라인 1개 = 드로우콜 1개.
- **[PF-08]** 겹치는 위치에 TMP Text와 Image를 교차 배치하지 않는다. 같은 타입끼리 Hierarchy에서 그룹핑한다.
- **[PF-09]** 런타임에서 `graphic.material` setter에 접근하지 않는다. 머티리얼 인스턴스가 생성되어 배치가 영구적으로 파괴된다.
- **[PF-10]** `MaterialPropertyBlock`은 Canvas UI 배칭과 호환되지 않는다. UI에서 사용 금지.
- **[PF-11]** 커스텀 UI 셰이더에 per-element 데이터를 전달할 때 UV 채널(UV1-UV3) + `IMeshModifier`를 사용한다.
- **[PF-12]** Canvas 내 요소 하나라도 변경되면 해당 Canvas 전체가 `BuildBatch`를 다시 실행한다. 서브 Canvas로 영향 범위를 제한한다.
- **[PF-13]** Canvas당 동적으로 변하는 요소는 50~200개 이하로 유지한다. 초과 시 Canvas 분할을 고려한다.
- **[PF-14]** `Text.text` 변경, LayoutGroup 자식 토글 등 `SetLayoutDirty`를 유발하는 작업을 같은 프레임에 다수 실행하지 않는다.

### 바(Bar) / 게이지

- **[BR-01]** HP, 스태미나 등 fill 바는 `Image.type = Filled` + `Image.fillAmount`를 사용한다. `localScale` 방식은 자식 요소 왜곡과 서브픽셀 아티팩트를 유발한다.
- **[BR-02]** fillMethod은 `Horizontal`, fillOrigin은 `Left`가 기본이다. 디자인에 맞게 조정.

### TextMeshPro

- **[TM-01]** 한국어/중국어/일본어 등 CJK 텍스트는 Dynamic Font Asset을 사용한다. Static Font Asset은 글리프 수 제한으로 부적합.
- **[TM-02]** 주 폰트에서 지원하지 않는 문자를 위해 Fallback Font Assets 체인을 설정한다.
- **[TM-03]** 사용자 입력을 표시하는 TMP 컴포넌트는 Rich Text를 비활성화한다 (인젝션 방지).
- **[TM-04]** 한국어용 Font Atlas는 최소 4096×4096 해상도를 사용한다.

### 접근성

- **[AC-01]** 본문 텍스트 최소 fontSize: 16 (기준 해상도 기준). 중요 HUD 정보는 20 이상.
- **[AC-02]** 텍스트와 배경의 명암 대비비 최소 4.5:1 (WCAG 2.1 AA 기준).
- **[AC-03]** 버튼/터치 타겟의 최소 크기: 48×48 픽셀 (기준 해상도 기준).
- **[AC-04]** 색상만으로 정보를 전달하지 않는다. 아이콘, 텍스트, 형태를 병용한다.

### 네비게이션

- **[NV-01]** 메뉴 화면에는 반드시 첫 번째 선택 요소를 지정한다: `EventSystem.current.SetSelectedGameObject(firstButton.gameObject)`.
- **[NV-02]** 자동 네비게이션이 부적절한 경우 Button.Navigation을 Explicit으로 설정하고 상하좌우를 수동 지정한다.
- **[NV-03]** 모든 메뉴는 키보드(Tab/화살표)와 게임패드만으로 완전히 조작 가능해야 한다.

### 씬 구성

- **[SC-01]** UI가 있는 모든 씬에는 정확히 1개의 EventSystem이 있어야 한다. 0개면 입력 불가, 2개 이상이면 중복 처리.
- **[SC-02]** Screen Space - Overlay Canvas가 있는 씬에도 Camera + AudioListener가 있어야 한다 (Unity 경고 방지).
- **[SC-03]** Additive 씬 로딩 시 EventSystem 중복에 주의한다.

### 아키텍처

- **[AR-01]** UI 갱신은 이벤트 기반으로 한다. `Update()`에서 상태를 폴링하여 UI를 갱신하지 않는다.
- **[AR-02]** 여러 화면(메뉴 → 옵션 → 뒤로)은 UI 스택 패턴으로 관리한다. push/pop으로 화면 전환.
- **[AR-03]** 화면을 비활성화할 때는 `CanvasGroup.interactable = false` + `blocksRaycasts = false`를 우선 사용한다. `SetActive(false)`는 Canvas rebuild와 OnEnable/OnDisable 비용이 발생한다.

### 오버드로우

- **[OD-01]** 보이지 않는 클릭 영역에는 투명 Image 대신 `NonDrawingGraphic`(빈 `OnPopulateMesh`)을 사용한다. 드로우콜과 오버드로우가 없다.
- **[OD-02]** `Graphic.raycastPadding`으로 터치/클릭 영역을 확장·축소한다. 추가 Image 컴포넌트 불필요.
- **[OD-03]** Scene 뷰 Overdraw 모드로 레이어 수를 확인한다. 한 픽셀에 3~4 레이어 이하를 목표로 한다.
- **[OD-04]** 전체화면 딤 오버레이는 전용 Canvas에 배치하고, 미사용 시 Canvas GameObject 자체를 비활성화한다. alpha=0만으로는 드로우콜이 남는다.
- **[OD-05]** 레이아웃 전용 컨테이너(배경이 없는 그룹핑 오브젝트)의 불필요한 Image 컴포넌트를 제거한다.

### 마스크

- **[MK-01]** 사각형 클리핑은 `RectMask2D`를 사용한다. 드로우콜 추가 없이 영역 밖 자식을 자동 컬링한다.
- **[MK-02]** 원형·복잡한 형태의 마스킹에만 `Mask`를 사용한다.
- **[MK-03]** `Mask` 중첩은 2~3단계 이하로 제한한다. 중첩 단계마다 드로우콜이 +2 증가한다.
- **[MK-04]** `ScrollRect`에는 반드시 `RectMask2D`를 사용한다. `Mask`는 배치를 깨고 자식 컬링을 지원하지 않는다.

### 스크롤 / 대량 목록

- **[LS-01]** 아이템이 20~30개를 초과하는 목록은 가상화(virtualized) 리스트 패턴을 사용한다. 뷰포트에 보이는 수 + 버퍼만 인스턴스화한다.
- **[LS-02]** 가상화 리스트의 content에 LayoutGroup을 사용하지 않는다. 아이템 위치를 `anchoredPosition`으로 수동 배치한다.
- **[LS-03]** content의 `sizeDelta.y`를 `totalCount * itemHeight`로 설정해 스크롤바 동작을 보장한다.
- **[LS-04]** 스크롤 이벤트는 `ScrollRect.onValueChanged` 콜백으로 처리한다. `Update()` 폴링 금지.
- **[LS-05]** 직접 구현이 복잡한 경우 LoopScrollRect, EnhancedScroller 등 검증된 서드파티 솔루션을 고려한다.

### UI 애니메이션

- **[AN-01]** UI에 `Animator` 컴포넌트를 사용하지 않는다. idle 상태에서도 매 프레임 Canvas를 dirty로 만들어 `SendWillRenderCanvases`가 계속 실행된다.
- **[AN-02]** `Animator` 사용이 불가피하면 애니메이션 완료 즉시 컴포넌트를 비활성화한다. 재생 시에만 활성화한다.
- **[AN-03]** DOTween 또는 코루틴 기반 애니메이션을 권장한다. 활성 트윈 기간에만 dirty가 발생하고 완료 후 오버헤드가 없다.
- **[AN-04]** 애니메이션이 있는 요소는 전용 서브 Canvas에 배치한다. 정적 부모 Canvas의 rebuild에 영향을 주지 않는다.

### 프로파일링

- **[PR-01]** `Canvas.SendWillRenderCanvases`를 CPU Profiler로 모니터링한다. 전체 Canvas 합산 2ms 이하를 목표로 한다.
- **[PR-02]** `Canvas.BuildBatch`가 1ms를 초과하면 해당 Canvas를 분할한다.
- **[PR-03]** UI 드로우콜 예산: 모바일 30~50개, PC 100개 이하. Frame Debugger로 측정한다.
- **[PR-04]** `Layout.Rebuild` 비용이 높으면 빈번하게 재계산되는 LayoutGroup을 찾아 수동 배치로 전환한다.
- **[PR-05]** UI Profiler 모듈(Unity 2021+)에서 Canvas별 배치 수와 버텍스 통계를 확인한다.

### URP / SRP 연동

- **[SRP-01]** SRP Batcher와 GPU Instancing은 Canvas UI에 적용되지 않는다. UI는 별도의 배칭 시스템을 사용한다.
- **[SRP-02]** URP render scale이 1.0 미만일 때 UI는 Screen Space - Overlay를 사용한다. Screen Space - Camera는 낮은 render scale의 영향을 받아 흐릿해진다.
- **[SRP-03]** 포스트프로세싱(블룸, 컬러그레이딩 등) 효과가 UI에 적용되어야 한다면 Screen Space - Camera를 사용한다. Overlay는 포스트프로세싱 이후에 렌더링된다.
- **[SRP-04]** URP에서 UI 커스텀 이펙트는 `BaseMeshEffect` 또는 커스텀 UI 셰이더로 구현한다. Render Feature는 Overlay UI에 삽입 불가.

### 픽셀 퍼펙트

- **[PP-01]** 정적 UI Canvas에는 `Canvas.pixelPerfect`를 활성화한다. 애니메이션 Canvas에는 비활성화한다(픽셀 스냅으로 인한 지터 방지).
- **[PP-02]** TMP 폰트 아틀라스 해상도: Latin 계열 최소 2048×2048, CJK(한/중/일) 최소 4096×4096.
- **[PP-03]** `CanvasScaler.referencePixelsPerUnit`은 100(기본값)을 유지한다. 변경하면 스프라이트 스케일링으로 프랙셔널 픽셀이 발생한다.
- **[PP-04]** 픽셀아트 UI 스프라이트는 텍스처 임포트 설정에서 `Filter Mode = Point`, `Compression = None`으로 설정한다.
- **[PP-05]** 고DPI 디스플레이에서 `Canvas.scaleFactor`가 OS 디스플레이 스케일링을 반영하는지 확인한다.
