using System;
using System.Collections;

using UnityEngine;

/// <summary>
/// 분신 1~3명을 랜덤 소환하는 패턴.
/// 분신은 보스와 동일한 외형이지만, 반사탄 1발에 소멸한다.
/// 분신은 플레이어를 향해 단발 사격 후 일정 시간 뒤 자동 소멸한다.
/// </summary>
public class CloneSummonPattern : BossPattern
{
    [Header("Clone Settings")]
    [SerializeField] private GameObject clonePrefab;
    [SerializeField] private int minClones = 1;
    [SerializeField] private int maxClones = 3;
    [SerializeField] private float spawnRadius = 8f;
    [SerializeField] private float cloneLifetime = 5f;
    [SerializeField] private float afterDelay = 2f;

    [Header("Clone Attack")]
    [SerializeField] private GameObject bulletPrefab;
    [SerializeField] private float bulletSpeed = 18f;
    [SerializeField] private float attackDelay = 1f;

    protected override void ExecutePattern(BossEnemy boss, Action onComplete)
    {
        StartCoroutine(SummonClones(boss, onComplete));
    }

    private IEnumerator SummonClones(BossEnemy boss, Action onComplete)
    {
        Transform target = boss.Target;
        if (target == null)
        {
            onComplete?.Invoke();
            yield break;
        }

        int cloneCount = UnityEngine.Random.Range(minClones, maxClones + 1);

        for (int i = 0; i < cloneCount; i++)
        {
            // 보스 주변 랜덤 위치에 소환
            Vector2 randomOffset = UnityEngine.Random.insideUnitCircle * spawnRadius;
            Vector3 spawnPos = boss.transform.position + new Vector3(randomOffset.x, 0f, randomOffset.y);

            GameObject clone = Instantiate(clonePrefab, spawnPos, Quaternion.identity);

            // 분신 자동 소멸
            Destroy(clone, cloneLifetime);

            // 분신이 일정 시간 후 플레이어를 향해 사격
            StartCoroutine(CloneAttack(clone, target, boss));
        }

        // 소환 후 대기 (분신은 독립적으로 행동)
        yield return new WaitForSeconds(afterDelay);
        onComplete?.Invoke();
    }

    private IEnumerator CloneAttack(GameObject clone, Transform target, BossEnemy boss)
    {
        yield return new WaitForSeconds(attackDelay);

        if (clone == null || target == null) yield break;

        if (BulletPool.Instance == null) yield break;

        Vector3 direction = (target.position - clone.transform.position).normalized;
        direction.y = 0f;

        // 분신이 플레이어를 바라봄
        clone.transform.forward = direction;

        Quaternion rotation = Quaternion.LookRotation(direction);
        Bullet bullet = BulletPool.Instance.Get(bulletPrefab, clone.transform.position, rotation);
        bullet.Speed = bulletSpeed;
        bullet.SetOwner(boss.gameObject);
    }
}
