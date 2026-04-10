using UnityEngine;

/// <summary>
/// Stage2Boss 애니메이션 제어.
/// Stage2Boss 루트에 붙이고, Animator는 자식(Character_Geisha_Black)에서 자동으로 찾는다.
/// Stage2Boss.cs와 각 공격 패턴에서 호출한다.
/// </summary>
public class Stage2BossAnimator : MonoBehaviour
{
    private Animator animator;

    private static readonly int AttackHash = Animator.StringToHash("Attack");
    private static readonly int HitHash = Animator.StringToHash("Hit");
    private static readonly int DieHash = Animator.StringToHash("Die");
    private static readonly int IsMovingHash = Animator.StringToHash("IsMoving");

    private void Awake()
    {
        // GetComponentInChildren은 자기 자신도 포함하므로, 자식에서만 탐색
        foreach (Animator a in GetComponentsInChildren<Animator>())
        {
            if (a.gameObject != gameObject)
            {
                animator = a;
                break;
            }
        }

        if (animator == null)
            Debug.LogWarning("[Stage2BossAnimator] 자식에서 Animator를 찾을 수 없습니다.");
        else
            Debug.Log($"[Stage2BossAnimator] Animator 연결됨: {animator.gameObject.name}, Controller: {animator.runtimeAnimatorController?.name}");
    }

    /// <summary>공격 패턴 발사 시 호출.</summary>
    public void PlayAttack()
    {
        Debug.Log("[Stage2BossAnimator] PlayAttack 호출됨");
        animator?.SetTrigger(AttackHash);
    }

    /// <summary>반사탄 피격 또는 처형 시 호출.</summary>
    public void PlayHit()
    {
        Debug.Log("[Stage2BossAnimator] PlayHit 호출됨");
        animator?.SetTrigger(HitHash);
    }

    /// <summary>보스 사망 시 호출.</summary>
    public void PlayDie() => animator?.SetTrigger(DieHash);

    /// <summary>스트레이핑 이동 시작/정지 시 호출.</summary>
    public void SetMoving(bool isMoving)
    {
        if (animator != null) animator.SetBool(IsMovingHash, isMoving);
    }
}
