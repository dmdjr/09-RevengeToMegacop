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
    [SerializeField] private float bowReleaseDelay = 0.5f;
    [SerializeField] private float afterDelay = 1.5f;
    [SerializeField] private GameObject muzzleEffectPrefab;

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

        Stage2Boss stage2Boss = boss as Stage2Boss;
        stage2Boss?.PauseMovement();

        // 1발: 탕
        boss.GetComponent<Stage2BossAnimator>()?.PlayAttack();
        yield return new WaitForSeconds(bowReleaseDelay);
        FireAtTarget(boss, target);

        // 긴 딜레이
        yield return new WaitForSeconds(firstDelay);

        // 2발: 탕
        boss.GetComponent<Stage2BossAnimator>()?.PlayAttack();
        yield return new WaitForSeconds(bowReleaseDelay);
        FireAtTarget(boss, target);

        // 짧은 딜레이
        yield return new WaitForSeconds(secondDelay);

        // 3발: 탕
        boss.GetComponent<Stage2BossAnimator>()?.PlayAttack();
        yield return new WaitForSeconds(bowReleaseDelay);
        FireAtTarget(boss, target);

        // 공격 후 여유 시간(후딜) 동안 정지 유지, 그 후 이동 재개
        yield return new WaitForSeconds(afterDelay);
        stage2Boss?.ResumeMovement();
        onComplete?.Invoke();
    }

    private void FireAtTarget(BossEnemy boss, Transform target)
    {
        Transform firePoint = (boss as Stage2Boss)?.WeaponPoint ?? boss.transform;
        Vector3 direction = (target.position - firePoint.position).normalized;
        direction.y = 0f;

        Quaternion rotation = Quaternion.LookRotation(direction);

        if (muzzleEffectPrefab != null)
        {
            GameObject effect = Instantiate(muzzleEffectPrefab, firePoint.position, rotation);
            Destroy(effect, 2f);
        }

        Bullet bullet = BulletPool.Instance.Get(bulletPrefab, firePoint.position, rotation);
        bullet.Speed = bulletSpeed;
        bullet.SetOwner(boss.gameObject);
    }
}
