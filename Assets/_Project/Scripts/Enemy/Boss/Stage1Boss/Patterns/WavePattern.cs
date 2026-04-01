using System;
using System.Collections;
using UnityEngine;

public class WavePattern : BossPattern
{
    [SerializeField] private GameObject wavePrefab;
    [SerializeField] private float maxRange = 10f;
    [SerializeField] private float holdDuration = 3f;

    protected override void ExecutePattern(BossEnemy boss, Action onComplete)
    {
        StartCoroutine(EmitCircularWave(boss, onComplete));
    }

    private IEnumerator EmitCircularWave(BossEnemy boss, Action onComplete)
    {
        if (boss.Target == null || Vector3.Distance(boss.transform.position, boss.Target.position) > maxRange)
        {
            yield return null;
            onComplete?.Invoke();
            yield break;
        }

        (boss as Stage1Boss)?.NotifyPatternStart();

        if (wavePrefab != null)
            Instantiate(wavePrefab, boss.transform.position, Quaternion.identity);

        yield return new WaitForSeconds(holdDuration);
        (boss as Stage1Boss)?.NotifyPatternEnd();
        onComplete?.Invoke();
    }
}