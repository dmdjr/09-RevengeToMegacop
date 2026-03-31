using System;
using System.Collections;

using UnityEngine;

public class BasicShotPattern : BossPattern
{
    [SerializeField] private GameObject bulletPrefab;
    [SerializeField] private Transform firePoint;
    [SerializeField] private float bulletSpeed = 12f;
    [SerializeField] private int shotCount = 3;
    [SerializeField] private float shotInterval = 0.3f;

    protected override void ExecutePattern(BossEnemy boss, Action onComplete)
    {
        StartCoroutine(FireSequence(boss, onComplete));
    }

    private IEnumerator FireSequence(BossEnemy boss, Action onComplete)
    {
        for (int i = 0; i < shotCount; i++)
        {
            Fire(boss);
            yield return new WaitForSeconds(shotInterval);
        }
        onComplete?.Invoke();
    }

    private void Fire(BossEnemy boss)
    {
        if (bulletPrefab == null || boss.Target == null) return;

        Transform origin = firePoint != null ? firePoint : boss.transform;
        Vector3 direction = boss.Target.position - origin.position;
        direction.y = 0f;

        if (direction == Vector3.zero) return;

        Quaternion rotation = Quaternion.LookRotation(direction.normalized);
        Bullet bullet = BulletPool.Instance.Get(bulletPrefab, origin.position, rotation);
        if (bullet == null) return;

        bullet.SetOwner(boss.gameObject);
        bullet.Speed = bulletSpeed;
    }
}
