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
    [SerializeField] private float bowReleaseDelay = 0.5f;
    [SerializeField] private float afterDelay = 1.5f;
    [SerializeField] private GameObject muzzleEffectPrefab;

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
            // 매 발마다 활 시위 당기고 발사
            boss.GetComponent<Stage2BossAnimator>()?.PlayAttack();
            yield return new WaitForSeconds(bowReleaseDelay);

            // 매 발마다 플레이어 방향 재조준
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

            // 간격이 점점 짧아짐
            if (i < shotCount - 1)
            {
                float t = (float)i / (shotCount - 1);
                float delay = Mathf.Lerp(firstDelay, lastDelay, t);
                yield return new WaitForSeconds(delay);
            }
        }

        // 공격 후 여유 시간(후딜) 동안 정지 유지, 그 후 이동 재개
        yield return new WaitForSeconds(afterDelay);
        stage2Boss?.ResumeMovement();
        onComplete?.Invoke();
    }
}
