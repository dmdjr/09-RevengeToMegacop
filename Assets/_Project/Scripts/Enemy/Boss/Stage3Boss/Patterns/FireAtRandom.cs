using System;
using System.Collections;
using UnityEngine;

public class FireAtRandom : BossPattern
{
    protected override void ExecutePattern(BossEnemy boss, Action onComplete)
    {
        StartCoroutine(FireRandom(boss , onComplete)); 
    }

    private IEnumerator  FireRandom(BossEnemy boss , Action onComplete)
    {
        //TODO : 총알을 45도 방향으로 흩뿌리는 패턴
        yield return null;
    }
}
