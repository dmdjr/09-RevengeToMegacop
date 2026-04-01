using System;
using System.Collections;
using UnityEngine;

public class BombPattern : BossPattern
{
    [SerializeField] private GameObject bombPrefab;
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

        Bullet bullet = BulletPool.Instance.Get(bombPrefab, boss.transform.position, Quaternion.identity);
        Stage1BossBomb bomb = bullet as Stage1BossBomb;

        if (bomb == null)
        {
            yield return null;
            onComplete?.Invoke();
            yield break;
        }

        (boss as Stage1Boss)?.NotifyPatternStart();
        bomb.SetOwner(boss.gameObject);
        bomb.Launch(boss.transform.position, boss.Target.position);

        yield return new WaitForSeconds(holdDuration);
        (boss as Stage1Boss)?.NotifyPatternEnd();
        onComplete?.Invoke();
    }
}