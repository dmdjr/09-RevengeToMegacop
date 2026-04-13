using System.Collections;
using MoreMountains.Feedbacks;
using TMPro;
using UnityEngine;

/// <summary>
/// 패리 성공 횟수를 화면 우측에 콤보 카운터 스타일로 표시하는 UI.
/// 콤보가 쌓일수록 스케일·색상·회전이 점점 강해지는 하이프 스타일.
/// 패리 없거나 피격 시 페이드아웃 후 리셋.
/// 하위 오브젝트(컨테이너·텍스트·피드백 플레이어)는 프리팹에 미리 배치되어야 한다.
/// </summary>
public class ParryCounterUI : MonoBehaviour
{
    [SerializeField] private PlayerHitController playerHitController;

    [Header("UI 참조")]
    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private TextMeshProUGUI counterText;
    [SerializeField] private TextMeshProUGUI rankText;

    [Header("피드백 플레이어")]
    [SerializeField] private MMF_Player punchPlayer;
    [SerializeField] private MMF_Player fadeInPlayer;
    [SerializeField] private MMF_Player fadeOutPlayer;
    [SerializeField] private MMF_Player rotationPlayer;

    [Header("타이밍")]
    [SerializeField] private float resetDelay = 3f;

    [Header("펀치 스케일 증폭")]
    [SerializeField] private float basePunchScale = 1.3f;
    [SerializeField] private float scaleGrowthPerHit = 0.08f;
    [SerializeField] private float maxPunchScale = 2.0f;

    [Header("회전 Wobble")]
    [SerializeField] private int rotationMinCombo = 3;

    [Header("랭크")]
    [SerializeField] private int rankStep = 10;
    [SerializeField] private string[] rankLabels = { "E", "D", "C", "B", "A", "S", "SS", "SSS" };

    [Header("랭크 색상")]
    [SerializeField] private Color[] rankColors = {
        Color.white,                            // E
        new Color(1.00f, 0.90f, 0.43f),         // D
        new Color(1.00f, 0.55f, 0.26f),         // C
        new Color(1.00f, 0.24f, 0.24f),         // B
        new Color(1.00f, 0.20f, 1.00f),         // A
        new Color(1.00f, 0.85f, 0.00f),         // S
        new Color(0.00f, 0.90f, 1.00f),         // SS
        new Color(1.00f, 0.00f, 0.40f)          // SSS
    };

    private int parryCount;
    private int lastRankIndex = -1;
    private Coroutine resetCoroutine;
    private WaitForSeconds waitForResetDelay;

    private MMF_Scale scaleFeedback;

    void Start()
    {
        if (playerHitController == null)
        {
            playerHitController = FindFirstObjectByType<PlayerHitController>();
        }

        if (playerHitController == null)
        {
            Debug.LogError("ParryCounterUI: PlayerHitController를 찾을 수 없습니다.");
            return;
        }

        if (canvasGroup == null || counterText == null || rankText == null ||
            punchPlayer == null || fadeInPlayer == null || fadeOutPlayer == null || rotationPlayer == null)
        {
            Debug.LogError("ParryCounterUI: 프리팹 참조가 누락되었습니다.");
            return;
        }

        waitForResetDelay = new WaitForSeconds(resetDelay);

        // 프리팹에 직렬화된 MMF_Scale 피드백 획득 (동적 RemapCurveOne 조정용)
        foreach (var feedback in punchPlayer.FeedbacksList)
        {
            if (feedback is MMF_Scale scale)
            {
                scaleFeedback = scale;
                break;
            }
        }

        // 페이드아웃 완료 시 카운터 리셋 (UnityEvent 런타임 구독)
        fadeOutPlayer.Events.OnComplete.AddListener(ResetCounter);

        canvasGroup.alpha = 0f;

        playerHitController.OnParry += OnParry;
        playerHitController.OnDamaged += OnDamaged;
    }

    void OnDestroy()
    {
        if (playerHitController != null)
        {
            playerHitController.OnParry -= OnParry;
            playerHitController.OnDamaged -= OnDamaged;
        }

        if (fadeOutPlayer != null)
        {
            fadeOutPlayer.Events.OnComplete.RemoveListener(ResetCounter);
        }
    }

    private void OnParry()
    {
        if (fadeOutPlayer.IsPlaying)
        {
            fadeOutPlayer.StopFeedbacks();
            canvasGroup.alpha = 1f;
        }

        parryCount++;
        int rankIndex = Mathf.Min(parryCount / rankStep, rankLabels.Length - 1);

        counterText.text = parryCount.ToString();
        rankText.text = rankLabels[rankIndex];

        if (rankIndex != lastRankIndex)
        {
            Color rankColor = GetRankColor(rankIndex);
            counterText.color = rankColor;
            rankText.color = rankColor;
            lastRankIndex = rankIndex;
        }

        if (parryCount == 1)
        {
            fadeInPlayer.PlayFeedbacks();
        }

        if (scaleFeedback != null)
        {
            float dynamicScale = Mathf.Min(
                basePunchScale + parryCount * scaleGrowthPerHit,
                maxPunchScale);
            scaleFeedback.RemapCurveOne = dynamicScale;
        }
        punchPlayer.PlayFeedbacks();

        if (parryCount >= rotationMinCombo)
        {
            rotationPlayer.PlayFeedbacks();
        }

        if (resetCoroutine != null)
        {
            StopCoroutine(resetCoroutine);
        }
        resetCoroutine = StartCoroutine(ResetAfterDelay());
    }

    private void OnDamaged()
    {
        if (parryCount <= 0) return;

        parryCount = Mathf.Max(0, parryCount - 5);

        if (resetCoroutine != null)
        {
            StopCoroutine(resetCoroutine);
            resetCoroutine = null;
        }

        if (parryCount <= 0)
        {
            StartFadeOutAndReset();
            return;
        }

        int rankIndex = Mathf.Min(parryCount / rankStep, rankLabels.Length - 1);
        counterText.text = parryCount.ToString();
        rankText.text = rankLabels[rankIndex];

        if (rankIndex != lastRankIndex)
        {
            Color rankColor = GetRankColor(rankIndex);
            counterText.color = rankColor;
            rankText.color = rankColor;
            lastRankIndex = rankIndex;
        }

        resetCoroutine = StartCoroutine(ResetAfterDelay());
    }

    private IEnumerator ResetAfterDelay()
    {
        yield return waitForResetDelay;
        resetCoroutine = null;
        StartFadeOutAndReset();
    }

    private void StartFadeOutAndReset()
    {
        if (fadeOutPlayer.IsPlaying) return;
        fadeOutPlayer.PlayFeedbacks();
    }

    private void ResetCounter()
    {
        parryCount = 0;
        lastRankIndex = -1;
        counterText.text = "";
        counterText.color = Color.white;
        rankText.text = "";
        rankText.color = Color.white;
    }

    private Color GetRankColor(int rankIndex)
    {
        if (rankColors == null || rankColors.Length == 0) return Color.white;
        return rankColors[Mathf.Clamp(rankIndex, 0, rankColors.Length - 1)];
    }
}
