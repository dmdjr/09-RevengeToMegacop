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
    }

    /// <summary>
    /// 처형 시 즉사 대신 최대 HP의 15% 데미지를 준다.
    /// HP가 0 이하일 때만 진짜 사망 처리한다.
    /// </summary>
    public override void Die()
    {
        if (Hp > 0f)
        {
            float damage = MaxHp * maxDamagePerHitRatio;
            float newHp = Hp - damage;
            if (newHp < 0f) newHp = 0f;
            SetHp(newHp);
            return;
        }

        base.Die();
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
        // TODO: 페이즈 전환 연출 (무적 + 애니메이션)
    }

    protected override IEnumerator OnBossIntro()
    {
        // TODO: 등장 연출
        yield break;
    }

    protected override IEnumerator OnBossDeath()
    {
        // TODO: 사망 연출
        yield break;
    }
}
