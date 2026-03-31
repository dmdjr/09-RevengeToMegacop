using System;
using System.Collections;
using UnityEngine;

public class SmokeBomb : BossPattern
{
    protected override void ExecutePattern(BossEnemy boss, Action onComplete)
    {
        StartCoroutine(ThrowSmokeBomb(boss , onComplete));
    }

    IEnumerator ThrowSmokeBomb(BossEnemy boss, Action onComplete)
    {
        //TODO 연막 폭탄을 생성하고 포물선으로 던지기
        yield return null;
    }

}
