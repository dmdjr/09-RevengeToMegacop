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

    private Transform target;
    private GameObject owner;
    private GameObject bulletPrefab;
    private float bulletSpeed;
    private bool isDead = false;

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
    /// 반사탄에 맞으면 즉시 소멸.
    /// </summary>
    public void Hit(Bullet bullet)
    {
        if (bullet == null || isDead) return;
        isDead = true;
        Destroy(gameObject);
    }

    private IEnumerator AttackLoop()
    {
        yield return new WaitForSeconds(firstAttackDelay);

        while (!isDead && target != null)
        {
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

        Quaternion rotation = Quaternion.LookRotation(direction);
        Bullet bullet = BulletPool.Instance.Get(bulletPrefab, transform.position, rotation);
        bullet.Speed = bulletSpeed;
        bullet.SetOwner(owner);
    }
}
