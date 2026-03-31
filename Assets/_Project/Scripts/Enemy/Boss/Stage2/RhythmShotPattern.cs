using System;
using System.Collections;

using UnityEngine;

/// <summary>
/// 탕..탕탕 리듬으로 3연발을 발사하는 패턴.
/// 첫 발 후 긴 딜레이, 이후 짧은 간격으로 2발 연속 발사.
/// 매 발마다 플레이어 현재 위치를 조준한다.
/// </summary>
public class RhythmShotPattern : BossPattern
{
    [Header("Rhythm Settings")]
    [SerializeField] private float firstDelay = 0.7f;
    [SerializeField] private float secondDelay = 0.2f;

    [Header("Shot Settings")]
    [SerializeField] private GameObject bulletPrefab;
    [SerializeField] private float bulletSpeed = 20f;

    protected override void ExecutePattern(BossEnemy boss, Action onComplete)
    {
        StartCoroutine(RhythmShot(boss, onComplete));
    }

    private IEnumerator RhythmShot(BossEnemy boss, Action onComplete)
    {
        Transform target = boss.Target;
        if (target == null)
        {
            onComplete?.Invoke();
            yield break;
        }

        if (BulletPool.Instance == null)
        {
            Debug.LogError("BulletPool.Instance is null. RhythmShotPattern cannot fire.");
            onComplete?.Invoke();
            yield break;
        }

        // 1발: 탕
        FireAtTarget(boss, target);

        // 긴 딜레이
        yield return new WaitForSeconds(firstDelay);

        // 2발: 탕
        FireAtTarget(boss, target);

        // 짧은 딜레이
        yield return new WaitForSeconds(secondDelay);

        // 3발: 탕
        FireAtTarget(boss, target);

        onComplete?.Invoke();
    }

    private void FireAtTarget(BossEnemy boss, Transform target)
    {
        Vector3 direction = (target.position - boss.transform.position).normalized;
        direction.y = 0f;

        Quaternion rotation = Quaternion.LookRotation(direction);
        Bullet bullet = BulletPool.Instance.Get(bulletPrefab, boss.transform.position, rotation);
        bullet.Speed = bulletSpeed;
        bullet.SetOwner(boss.gameObject);
    }
}
