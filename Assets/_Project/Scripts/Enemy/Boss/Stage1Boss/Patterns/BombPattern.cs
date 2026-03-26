using System;
using System.Collections;
using UnityEngine;

public class BombPattern : BossPattern
{
    protected override void ExecutePattern(BossEnemy boss, Action onComplete)
    {
        StartCoroutine(ThrowBomb(boss, onComplete));
    }

    private IEnumerator ThrowBomb(BossEnemy boss, Action onComplete)
    {
        // TODO: 포물선으로 폭탄을 던지는 로직 구현

        yield return null;
    }
}