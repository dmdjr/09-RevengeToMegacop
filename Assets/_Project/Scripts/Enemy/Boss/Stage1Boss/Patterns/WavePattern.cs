using System;
using System.Collections;
using UnityEngine;

public class WavePattern : BossPattern
{
    [SerializeField] private GameObject wavePrefab;

    protected override void ExecutePattern(BossEnemy boss, Action onComplete)
    {
        StartCoroutine(EmitCircularWave(boss, onComplete));
    }

    private IEnumerator EmitCircularWave(BossEnemy boss, Action onComplete)
    {
        if (wavePrefab != null)
            Instantiate(wavePrefab, boss.transform.position, Quaternion.identity);

        yield return null;
        onComplete?.Invoke();
    }
}