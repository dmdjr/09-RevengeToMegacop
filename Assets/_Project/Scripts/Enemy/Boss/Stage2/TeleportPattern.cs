using System;
using System.Collections;

using UnityEngine;

/// <summary>
/// 순간이동 패턴. 두 가지 방식으로 발동한다.
///
/// [패턴 시스템 발동] BossEnemy가 일반 패턴으로 실행할 때 플레이어에게서 먼 거리로 이동.
/// [근접 감지 발동]  Update()에서 플레이어가 proximityTriggerDistance 이내로 접근하면
///                  쿨타임(proximityCooldown)이 지났을 때 즉시 원거리로 이동.
///                  처형 사거리(~10m)를 벗어난 거리(기본 11m)를 기준으로 한다.
/// </summary>
public class TeleportPattern : BossPattern
{
    [Header("Teleport Distance")]
    [SerializeField] private float minDistance = 18f;
    [SerializeField] private float maxDistance = 28f;

    [Header("Timing")]
    [SerializeField] private float teleportDelay = 0.3f;
    [SerializeField] private float afterDelay = 1f;

    [Header("Proximity Trigger")]
    [SerializeField] private float proximityTriggerDistance = 11f;
    [SerializeField] private float proximityCooldown = 4f;

    private BossEnemy cachedBoss;
    private float lastProximityTeleportTime = float.NegativeInfinity;

    private void Start()
    {
        cachedBoss = GetComponentInParent<BossEnemy>();
    }

    private void Update()
    {
        if (cachedBoss == null || cachedBoss.Target == null) return;
        if (Time.time < lastProximityTeleportTime + proximityCooldown) return;

        float dist = Vector3.Distance(cachedBoss.transform.position, cachedBoss.Target.position);
        if (dist < proximityTriggerDistance)
        {
            lastProximityTeleportTime = Time.time;
            TeleportTo(cachedBoss, cachedBoss.Target);
        }
    }

    protected override void ExecutePattern(BossEnemy boss, Action onComplete)
    {
        StartCoroutine(Teleport(boss, onComplete));
    }

    private IEnumerator Teleport(BossEnemy boss, Action onComplete)
    {
        if (boss.Target == null)
        {
            onComplete?.Invoke();
            yield break;
        }

        yield return new WaitForSeconds(teleportDelay);
        TeleportTo(boss, boss.Target);

        yield return new WaitForSeconds(afterDelay);
        onComplete?.Invoke();
    }

    /// <summary>보스를 플레이어 기준 원거리 랜덤 위치로 즉시 이동시킨다.</summary>
    private void TeleportTo(BossEnemy boss, Transform target)
    {
        float angle    = UnityEngine.Random.Range(0f, 360f);
        float distance = UnityEngine.Random.Range(minDistance, maxDistance);

        Vector3 offset = Quaternion.Euler(0f, angle, 0f) * Vector3.forward * distance;
        Vector3 newPos = target.position + offset;
        newPos.y = boss.transform.position.y;

        boss.transform.position = newPos;
    }
}
