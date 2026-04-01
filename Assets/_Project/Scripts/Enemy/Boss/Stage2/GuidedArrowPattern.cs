using System;
using System.Collections;

using UnityEngine;

/// <summary>
/// 느린 유도 화살을 발사하는 패턴.
/// 화살이 천천히 플레이어를 추적하며, 패링하면 보스에게 유도된다.
/// </summary>
public class GuidedArrowPattern : BossPattern
{
    [Header("Guided Arrow Settings")]
    [SerializeField] private GameObject guidedArrowPrefab;
    [SerializeField] private float arrowSpeed = 6f;
    [SerializeField] private float guidedDuration = 4f;
    [SerializeField] private float turnSpeed = 2f;
    [SerializeField] private float afterDelay = 1.5f;

    protected override void ExecutePattern(BossEnemy boss, Action onComplete)
    {
        StartCoroutine(FireGuidedArrow(boss, onComplete));
    }

    private IEnumerator FireGuidedArrow(BossEnemy boss, Action onComplete)
    {
        Transform target = boss.Target;
        if (target == null)
        {
            onComplete?.Invoke();
            yield break;
        }

        // 발사 방향
        Vector3 direction = (target.position - boss.transform.position).normalized;
        direction.y = 0f;
        Quaternion rotation = Quaternion.LookRotation(direction);

        // 유도 화살 생성 (풀 사용하지 않고 직접 생성 - 특수 동작이라)
        GameObject arrowObj = Instantiate(guidedArrowPrefab, boss.transform.position, rotation);
        Bullet bullet = arrowObj.GetComponent<Bullet>();
        if (bullet == null)
        {
            Destroy(arrowObj);
            onComplete?.Invoke();
            yield break;
        }

        bullet.Speed = arrowSpeed;
        bullet.SetOwner(boss.gameObject);

        // 유도 로직: guidedDuration 동안 플레이어를 추적
        float elapsed = 0f;
        while (elapsed < guidedDuration && arrowObj != null)
        {
            if (target != null)
            {
                Vector3 toTarget = (target.position - arrowObj.transform.position).normalized;
                toTarget.y = 0f;

                Quaternion targetRotation = Quaternion.LookRotation(toTarget);
                arrowObj.transform.rotation = Quaternion.Slerp(
                    arrowObj.transform.rotation,
                    targetRotation,
                    turnSpeed * Time.deltaTime
                );
            }

            elapsed += Time.deltaTime;
            yield return null;
        }

        yield return new WaitForSeconds(afterDelay);
        onComplete?.Invoke();
    }
}
