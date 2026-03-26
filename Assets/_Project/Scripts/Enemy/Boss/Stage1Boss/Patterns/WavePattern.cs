using System;
using System.Collections;
using UnityEngine;

public class WavePattern : BossPattern
{
    protected override void ExecutePattern(BossEnemy boss, Action onComplete)
    {
        StartCoroutine(EmitCircularWave(boss, onComplete));
    }

    private IEnumerator EmitCircularWave(BossEnemy boss, Action onComplete)
    {
        // TODO: 원형 파동을 방출하는 로직 구현

        yield return null;
    }
}