using System;
using System.Collections;
using UnityEngine;

public class BombPattern : BossPattern
{
    [SerializeField] private GameObject bombPrefab;
    [SerializeField] private Transform firePoint;
    [SerializeField] private float holdDuration = 1f;

    protected override void ExecutePattern(BossEnemy boss, Action onComplete)
    {
        StartCoroutine(ThrowBomb(boss, onComplete));
    }

    private IEnumerator ThrowBomb(BossEnemy boss, Action onComplete)
    {
        if (bombPrefab == null || boss.Target == null)
        {
            yield return null;
            onComplete?.Invoke();
            yield break;
        }

        Transform origin = firePoint != null ? firePoint : boss.transform;
        Bullet bullet = BulletPool.Instance.Get(bombPrefab, origin.position, Quaternion.identity);
        Stage1BossBomb bomb = bullet as Stage1BossBomb;

        if (bomb == null)
        {
            yield return null;
            onComplete?.Invoke();
            yield break;
        }

        float bossToPlayerDist = Vector3.Distance(
            new Vector3(boss.transform.position.x, 0f, boss.transform.position.z),
            new Vector3(boss.Target.position.x, 0f, boss.Target.position.z));
        (boss as Stage1Boss)?.NotifyPatternStart();
        bomb.SetOwner(boss.gameObject);
        bomb.Launch(origin.position, boss.Target.position, bossToPlayerDist);

        yield return new WaitForSeconds(holdDuration);
        (boss as Stage1Boss)?.NotifyPatternEnd();
        onComplete?.Invoke();
    }
}