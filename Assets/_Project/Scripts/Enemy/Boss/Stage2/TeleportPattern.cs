using System;
using System.Collections;

using UnityEngine;

/// <summary>
/// 순간이동 패턴. 플레이어 주변 랜덤 위치로 이동한다.
/// 반사탄 회피 용도로, 플레이어 근처에서 위치만 바꿔 긴장감을 유지한다.
/// </summary>
public class TeleportPattern : BossPattern
{
    [Header("Teleport Settings")]
    [SerializeField] private float teleportDelay = 0.3f;
    [SerializeField] private float afterDelay = 1f;
    [SerializeField] private float minDistance = 5f;
    [SerializeField] private float maxDistance = 12f;

    protected override void ExecutePattern(BossEnemy boss, Action onComplete)
    {
        StartCoroutine(Teleport(boss, onComplete));
    }

    private IEnumerator Teleport(BossEnemy boss, Action onComplete)
    {
        Transform target = boss.Target;
        if (target == null)
        {
            onComplete?.Invoke();
            yield break;
        }

        yield return new WaitForSeconds(teleportDelay);

        // 플레이어 주변 랜덤 방향, minDistance~maxDistance 거리
        float angle = UnityEngine.Random.Range(0f, 360f);
        float distance = UnityEngine.Random.Range(minDistance, maxDistance);

        Vector3 offset = Quaternion.Euler(0f, angle, 0f) * Vector3.forward * distance;
        Vector3 newPos = target.position + offset;
        newPos.y = boss.transform.position.y;

        boss.transform.position = newPos;

        yield return new WaitForSeconds(afterDelay);
        onComplete?.Invoke();
    }
}
