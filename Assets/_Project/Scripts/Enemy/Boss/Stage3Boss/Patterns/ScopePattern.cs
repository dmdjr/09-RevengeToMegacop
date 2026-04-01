using System;
using System.Collections;
using UnityEngine;

public class ScopePattern : BossPattern
{
    protected override void ExecutePattern(BossEnemy boss, Action onComplete)
    {
        StartCoroutine(SpwanScope(boss,onComplete));
    }

    IEnumerator SpwanScope(BossEnemy boss, Action onComplete)
    {
        //TODO : 플레이어를 천천히 따라가는 스코프를 생성
        yield return null;
    }
}
