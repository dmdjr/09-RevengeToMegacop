using System.Collections;

using UnityEngine;

/// <summary>
/// Stage2 보스의 분신. 반사탄 1발에 소멸하며, 살아있는 동안 일정 간격으로 플레이어를 향해 사격한다.
/// IDamageable을 구현하여 반사탄에 반응한다.
/// CloneSummonPattern이 생성하며, lifetime 후 자동 소멸한다.
/// </summary>
public class BossClone : MonoBehaviour, IDamageable
{
    [SerializeField] private float attackInterval = 1.5f;
    [SerializeField] private float firstAttackDelay = 0.8f;
    [SerializeField] private float bowReleaseDelay = 0.5f;
    [SerializeField] private GameObject muzzleEffectPrefab;

    private static readonly int AttackHash = Animator.StringToHash("Attack");
    private static readonly int DieHash = Animator.StringToHash("Die");

    private Animator animator;
    private Transform target;
    private GameObject owner;
    private GameObject bulletPrefab;
    private float bulletSpeed;
    private bool isDead = false;

    void Awake()
    {
        foreach (Animator anim in GetComponentsInChildren<Animator>())
        {
            if (anim.gameObject != gameObject)
            {
                animator = anim;
                break;
            }
        }
    }

    /// <summary>
    /// 분신 초기화. CloneSummonPattern에서 생성 직후 호출한다.
    /// </summary>
    public void Initialize(Transform target, GameObject owner, GameObject bulletPrefab, float bulletSpeed)
    {
        this.target = target;
        this.owner = owner;
        this.bulletPrefab = bulletPrefab;
        this.bulletSpeed = bulletSpeed;

        StartCoroutine(AttackLoop());
    }

    /// <summary>
    /// 반사탄에 맞으면 사망 애니메이션 재생 후 소멸.
    /// </summary>
    public void Hit(Bullet bullet)
    {
        if (bullet == null || isDead) return;
        isDead = true;
        StartCoroutine(DeathSequence());
    }

    private IEnumerator DeathSequence()
    {
        if (animator != null)
        {
            animator.SetTrigger(DieHash);

            // Die 상태 진입 대기
            while (!animator.GetCurrentAnimatorStateInfo(0).IsName("Die"))
                yield return null;

            // Die 애니메이션 재생 완료 대기
            while (animator.GetCurrentAnimatorStateInfo(0).normalizedTime < 1f)
                yield return null;
        }

        Destroy(gameObject);
    }

    private IEnumerator AttackLoop()
    {
        yield return new WaitForSeconds(firstAttackDelay);

        while (!isDead && target != null)
        {
            if (animator != null) animator.SetTrigger(AttackHash);
            yield return new WaitForSeconds(bowReleaseDelay);
            Fire();
            yield return new WaitForSeconds(attackInterval);
        }
    }

    private void Fire()
    {
        if (isDead || target == null || BulletPool.Instance == null) return;

        Vector3 direction = (target.position - transform.position).normalized;
        direction.y = 0f;

        transform.forward = direction;

        Vector3 firePosition = transform.position;
        firePosition.y = target.position.y;

        Quaternion rotation = Quaternion.LookRotation(direction);

        if (muzzleEffectPrefab != null)
        {
            GameObject effect = Instantiate(muzzleEffectPrefab, firePosition, rotation);
            Destroy(effect, 2f);
        }

        Bullet bullet = BulletPool.Instance.Get(bulletPrefab, firePosition, rotation);
        bullet.Speed = bulletSpeed;
        bullet.SetOwner(owner);
    }
}
