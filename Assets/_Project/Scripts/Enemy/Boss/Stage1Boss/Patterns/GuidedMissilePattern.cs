using System;
using System.Collections;
using UnityEngine;

public class GuidedMissilePattern : BossPattern
{
    protected override void ExecutePattern(BossEnemy boss, Action onComplete)
    {
        StartCoroutine(LaunchGuidedMissiles(boss, onComplete));
    }

    private IEnumerator LaunchGuidedMissiles(BossEnemy boss, Action onComplete)
    {
        // TODO: 유도탄을 3연발사하는 로직 구현
        
        yield return null;
    }
}