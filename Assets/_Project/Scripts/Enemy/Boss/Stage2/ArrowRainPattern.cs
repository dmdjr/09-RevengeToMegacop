using System;
using System.Collections;

using UnityEngine;

/// <summary>
/// 플레이어 위치 주변에 경고 구역을 표시한 후, 다수의 화살을 낙하시키는 패턴.
/// 경고 구역은 바닥에 빨간 원(Projector 또는 Quad)으로 표현한다.
/// </summary>
public class ArrowRainPattern : BossPattern
{
    [Header("Warning Settings")]
    [SerializeField] private GameObject warningPrefab;
    [SerializeField] private float warningDuration = 1.5f;
    [SerializeField] private float warningRadius = 3f;

    [Header("Rain Settings")]
    [SerializeField] private GameObject bulletPrefab;
    [SerializeField] private int bulletCount = 15;
    [SerializeField] private float bulletSpeed = 15f;
    [SerializeField] private float spawnHeight = 15f;
    [SerializeField] private float spawnDuration = 0.5f;
    [SerializeField] private float afterDelay = 2f;

    protected override void ExecutePattern(BossEnemy boss, Action onComplete)
    {
        StartCoroutine(ArrowRain(boss, onComplete));
    }

    private IEnumerator ArrowRain(BossEnemy boss, Action onComplete)
    {
        Transform target = boss.Target;
        if (target == null)
        {
            onComplete?.Invoke();
            yield break;
        }

        if (BulletPool.Instance == null)
        {
            Debug.LogError("BulletPool.Instance is null. ArrowRainPattern cannot fire.");
            onComplete?.Invoke();
            yield break;
        }

        // 경고 구역 중심 = 플레이어 현재 위치
        Vector3 center = target.position;
        center.y = 0.01f;

        // 경고 표시
        GameObject warning = null;
        if (warningPrefab != null)
        {
            Quaternion flatRotation = Quaternion.Euler(90f, 0f, 0f);
            warning = Instantiate(warningPrefab, center, flatRotation);
            warning.transform.localScale = new Vector3(warningRadius * 2f, warningRadius * 2f, 1f);
        }

        yield return new WaitForSeconds(warningDuration);

        // 경고 제거
        if (warning != null)
        {
            Destroy(warning);
        }

        // 화살 낙하
        float interval = spawnDuration / bulletCount;
        for (int i = 0; i < bulletCount; i++)
        {
            // 경고 범위 내 랜덤 위치
            Vector2 randomOffset = UnityEngine.Random.insideUnitCircle * warningRadius;
            Vector3 spawnPos = new Vector3(center.x + randomOffset.x, spawnHeight, center.z + randomOffset.y);

            // 아래로 발사
            Quaternion downRotation = Quaternion.LookRotation(Vector3.down);
            Bullet bullet = BulletPool.Instance.Get(bulletPrefab, spawnPos, downRotation);
            bullet.Speed = bulletSpeed;
            bullet.SetOwner(boss.gameObject);

            yield return new WaitForSeconds(interval);
        }

        yield return new WaitForSeconds(afterDelay);
        onComplete?.Invoke();
    }
}
