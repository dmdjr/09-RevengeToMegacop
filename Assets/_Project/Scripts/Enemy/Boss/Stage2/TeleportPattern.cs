using System;
using System.Collections;

using UnityEngine;

/// <summary>
/// 플레이어가 근접하면 맵 반대편으로 순간이동하는 패턴.
/// 일정 거리 이내일 때만 발동하며, 텔레포트 전 짧은 연출 시간이 있다.
/// </summary>
public class TeleportPattern : BossPattern
{
    [Header("Teleport Settings")]
    [SerializeField] private float triggerDistance = 5f;
    [SerializeField] private float teleportDelay = 0.3f;
    [SerializeField] private float afterDelay = 1f;

    [Header("Map Bounds")]
    [SerializeField] private float mapMinX = -20f;
    [SerializeField] private float mapMaxX = 20f;
    [SerializeField] private float mapMinZ = -20f;
    [SerializeField] private float mapMaxZ = 20f;

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

        // 플레이어가 멀면 텔레포트 안 함
        float distance = Vector3.Distance(boss.transform.position, target.position);
        if (distance > triggerDistance)
        {
            onComplete?.Invoke();
            yield break;
        }

        // 텔레포트 전 딜레이 (연출용)
        yield return new WaitForSeconds(teleportDelay);

        // 맵 중심 기준 반대편으로 이동
        float mapCenterX = (mapMinX + mapMaxX) / 2f;
        float mapCenterZ = (mapMinZ + mapMaxZ) / 2f;

        Vector3 bossPos = boss.transform.position;
        float newX = mapCenterX + (mapCenterX - bossPos.x);
        float newZ = mapCenterZ + (mapCenterZ - bossPos.z);

        // 맵 범위 내로 클램프
        newX = Mathf.Clamp(newX, mapMinX, mapMaxX);
        newZ = Mathf.Clamp(newZ, mapMinZ, mapMaxZ);

        boss.transform.position = new Vector3(newX, bossPos.y, newZ);

        yield return new WaitForSeconds(afterDelay);
        onComplete?.Invoke();
    }
}
