using System;
using System.Collections;

using UnityEngine;

/// <summary>
/// 5연발을 발사하며 간격이 점점 짧아지는 패턴.
/// 첫 발은 느리게, 마지막 발은 거의 연사에 가깝다.
/// 매 발마다 플레이어 현재 위치를 조준한다.
/// </summary>
public class AcceleratingRhythmShotPattern : BossPattern
{
    [Header("Shot Settings")]
    [SerializeField] private GameObject bulletPrefab;
    [SerializeField] private float bulletSpeed = 22f;
    [SerializeField] private int shotCount = 5;
    [SerializeField] private float firstDelay = 0.6f;
    [SerializeField] private float lastDelay = 0.1f;
    [SerializeField] private float afterDelay = 1.5f;

    protected override void ExecutePattern(BossEnemy boss, Action onComplete)
    {
        StartCoroutine(AcceleratingShot(boss, onComplete));
    }

    private IEnumerator AcceleratingShot(BossEnemy boss, Action onComplete)
    {
        Transform target = boss.Target;
        if (target == null)
        {
            onComplete?.Invoke();
            yield break;
        }

        if (BulletPool.Instance == null)
        {
            Debug.LogError("BulletPool.Instance is null. AcceleratingRhythmShotPattern cannot fire.");
            onComplete?.Invoke();
            yield break;
        }

        Stage2Boss stage2Boss = boss as Stage2Boss;
        stage2Boss?.PauseMovement();

        Transform firePoint = stage2Boss != null ? stage2Boss.WeaponPoint : boss.transform;
        for (int i = 0; i < shotCount; i++)
        {
            // 매 발마다 플레이어 방향 재조준
            Vector3 direction = (target.position - firePoint.position).normalized;
            direction.y = 0f;

            boss.GetComponent<Stage2BossAnimator>()?.PlayAttack();

            Quaternion rotation = Quaternion.LookRotation(direction);
            Bullet bullet = BulletPool.Instance.Get(bulletPrefab, firePoint.position, rotation);
            bullet.Speed = bulletSpeed;
            bullet.SetOwner(boss.gameObject);

            // 간격이 점점 짧아짐
            if (i < shotCount - 1)
            {
                float t = (float)i / (shotCount - 1);
                float delay = Mathf.Lerp(firstDelay, lastDelay, t);
                yield return new WaitForSeconds(delay);
            }
        }

        stage2Boss?.ResumeMovement();

        yield return new WaitForSeconds(afterDelay);
        onComplete?.Invoke();
    }
}
