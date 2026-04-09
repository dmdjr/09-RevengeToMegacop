using System;
using System.Collections;

using UnityEngine;

public class GuidedMissilePattern : BossPattern
{
    [SerializeField] private GameObject missilePrefab;
    [SerializeField] private Transform firePoint;
    [SerializeField] private float missileSpeed = 10f;
    [SerializeField] private int missileCount = 3;
    [SerializeField] private float interval = 0.5f;
    [SerializeField] private float holdDuration = 2f;

    protected override void ExecutePattern(BossEnemy boss, Action onComplete)
    {
        StartCoroutine(LaunchGuidedMissiles(boss, onComplete));
    }

    private IEnumerator LaunchGuidedMissiles(BossEnemy boss, Action onComplete)
    {
        if (missilePrefab == null || boss.Target == null)
        {
            yield return null;
            onComplete?.Invoke();
            yield break;
        }

        (boss as Stage1Boss)?.NotifyPatternStart();
        for (int i = 0; i < missileCount; i++)
        {
            LaunchMissile(boss);
            yield return new WaitForSeconds(interval);
        }
        yield return new WaitForSeconds(holdDuration);
        (boss as Stage1Boss)?.NotifyPatternEnd();
        onComplete?.Invoke();
    }

    private void LaunchMissile(BossEnemy boss)
    {
        Transform origin = firePoint != null ? firePoint : boss.transform;
        Bullet bullet = BulletPool.Instance.Get(missilePrefab, origin.position, boss.transform.rotation);
        Stage1BossMissile missile = bullet as Stage1BossMissile;
        if (missile == null) return;

        missile.SetOwner(boss.gameObject);
        missile.Speed = missileSpeed;
        missile.Launch(boss.Target, boss.transform);
        missile.GetComponentInChildren<BulletVFX>()?.PlayMuzzle();
    }
}
