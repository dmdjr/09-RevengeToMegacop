using System.Collections;

using UnityEngine;

/// <summary>
/// 2스테이지 보스
/// 원거리 궁수 보스. 반사탄과 처형(최대 HP 15%)으로만 데미지를 줄 수 있다
/// Phase1(HP 100%~30%): 단발 조준, 3연발 리듬, 화살비, 순간이동
/// Phase2(HP 30%~0%): 5연발 가속, 유도 화살, 분신 소환, 순간이동
/// </summary>
public class Stage2Boss : BossEnemy
{
    [Header("Stage2 Boss Settings")]
    [SerializeField] private float maxDamagePerHitRatio = 0.15f;
    [SerializeField] private Stage2BossAnimator bossAnimator;
    [SerializeField] private Transform weaponPoint;
    [Header("Knockback")]
    [SerializeField] private float knockbackDistance = 1.5f;
    [SerializeField] private float knockbackDuration = 0.15f;

    /// <summary>화살 발사 기준점. 미설정 시 보스 루트 위치를 사용한다.</summary>
    public Transform WeaponPoint => weaponPoint != null ? weaponPoint : transform;

    [Header("Arrow Rain (Background Hazard)")]
    [SerializeField] private ArrowRainPattern arrowRainPattern;

    [Header("Phase1 Patterns")]
    [SerializeField] private BossPattern[] phase1Patterns;

    [Header("Phase2 Patterns")]
    [SerializeField] private BossPattern[] phase2Patterns;

    /// <summary>
    /// 반사탄/처형만 데미지 허용. 1회 최대 데미지는 MaxHp의 15%.
    /// base.Hit() 호출 후 HP가 너무 많이 깎였으면 보정한다.
    /// </summary>
    public override void Hit(Bullet bullet)
    {
        if (bullet == null) return;

        float hpBefore = Hp;
        float maxDamage = MaxHp * maxDamagePerHitRatio;
        float minHpAfterHit = hpBefore - maxDamage;
        if (minHpAfterHit < 0f) minHpAfterHit = 0f;

        base.Hit(bullet);

        if (Hp < minHpAfterHit)
        {
            SetHp(minHpAfterHit);
        }

        bossAnimator?.PlayHit();
        // 반사탄 방향으로 넉백
        Vector3 knockbackDirection = bullet.transform.forward;
        knockbackDirection.y = 0f;
        knockbackDirection.Normalize();
        StartCoroutine(KnockbackCoroutine(knockbackDirection));


        Debug.Log($"[Stage2Boss] HP: {Hp}/{MaxHp} ({Mathf.RoundToInt(HpRatio * 100)}%)");
    }

    /// <summary>
    /// 처형 시 즉사 대신 최대 HP의 15% 데미지를 준다. 슬라이스 없이 슬래시 VFX만 재생한다.
    /// HP가 0 이하가 되면 base.Die()로 진짜 사망 처리한다.
    /// </summary>
    public override ExecutionResult HandleExecution(ExecutionContext context)
    {
        if (context.SlashVfx != null)
            context.SlashVfx.Play(context.SlicePosition, context.SlashDirection);

        bossAnimator?.PlayHit();

        float damage = MaxHp * maxDamagePerHitRatio;
        float newHp = Hp - damage;
        if (newHp < 0f) newHp = 0f;
        SetHp(newHp);

        if (newHp <= 0f)
            TriggerDeathSequence();
        return new ExecutionResult
        {
            Target = this,
            Position = context.SlicePosition
        };
    }

    /// <summary>
    /// 유도 화살 명중 시 호출. MaxHp × damageRatio 만큼 HP를 깎는다.
    /// GuidedArrowBullet에서 사용. SetHp()가 protected이므로 이 메서드로 중개한다.
    /// </summary>
    public void TakeGuidedArrowDamage(float damageRatio)
    {
        float damage = MaxHp * damageRatio;
        float newHp = Hp - damage;
        if (newHp < 0f) newHp = 0f;
        SetHp(newHp);
    }

    protected override BossPattern[] GetPatternsForPhase(int phaseIndex)
    {
        return phaseIndex switch
        {
            0 => phase1Patterns,
            1 => phase2Patterns,
            _ => phase1Patterns
        };
    }

    protected override void OnPhaseChanged(int phaseIndex, BossPhaseData data)
    {
        // Phase2 진입 시 색상 변경 (임시 시각 피드백)
        if (phaseIndex == 1)
        {
            Renderer renderer = GetComponentInChildren<Renderer>();
            if (renderer != null)
            {
                renderer.material.color = Color.red;
            }

            Debug.Log("[Stage2Boss] Phase2 진입");
        }
    }

    protected override IEnumerator OnBossIntro()
    {
        // TODO: 등장 연출

        // 보스전 시작 시 화살비 가동
        if (arrowRainPattern != null)
        {
            arrowRainPattern.StartRain(Target, gameObject);
        }

        yield break;
    }

    protected override IEnumerator OnBossDeath()
    {
        bossAnimator?.PlayDie();

        // 사망 시 화살비 중단
        if (arrowRainPattern != null)
        {
            arrowRainPattern.StopRain();
        }

        // 사망 시 모든 분신 제거
        BossClone[] clones = FindObjectsByType<BossClone>(FindObjectsSortMode.None);
        for (int i = 0; i < clones.Length; i++)
        {
            if (clones[i] != null)
            {
                Destroy(clones[i].gameObject);
            }
        }

        // TODO: 사망 연출
        yield break;
    }
    private IEnumerator KnockbackCoroutine(Vector3 direction)
    {
        Vector3 startPosition = transform.position;
        Vector3 endPosition = startPosition + direction * knockbackDistance;
        float elapsed = 0f;

        while (elapsed < knockbackDuration)
        {
            elapsed += Time.deltaTime;
            float ratio = elapsed / knockbackDuration;
            transform.position = Vector3.Lerp(startPosition, endPosition, ratio);
            yield return null;
        }

        transform.position = endPosition;
    }
}
