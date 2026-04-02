using System.Collections;

using UnityEngine;

/// <summary>
/// 보스전 동안 백그라운드로 계속 작동하는 화살비 환경 위협.
/// 패턴 시스템(BossPattern)과 독립적으로 동작하며,
/// 일정 간격마다 플레이어 주변에 경고 구역 표시 → 화살 낙하를 반복한다.
/// 패링 불가능 (위에서 떨어지므로), 플레이어 이동을 방해하는 용도.
/// Stage2Boss에서 StartRain() / StopRain()으로 제어한다.
/// </summary>
public class ArrowRainPattern : MonoBehaviour
{
    [Header("Warning Settings")]
    [SerializeField] private GameObject warningPrefab;
    [SerializeField] private float warningDuration = 1.5f;
    [SerializeField] private float warningRadius = 3f;

    [Header("Rain Settings")]
    [SerializeField] private GameObject bulletPrefab;
    [SerializeField] private int bulletCount = 15;
    [SerializeField] private float bulletSpeed = 15f;
    [SerializeField] private float spawnHeight = 15f;
    [SerializeField] private float spawnDuration = 0.5f;
    [SerializeField] private float rainDamage = 10f;

    [Header("Repeat Settings")]
    [SerializeField] private float repeatInterval = 4f;

    private Transform target;
    private GameObject owner;
    private Coroutine rainLoop;

    /// <summary>
    /// 화살비 반복 시작. 보스전 시작 시 호출한다.
    /// </summary>
    public void StartRain(Transform target, GameObject owner)
    {
        this.target = target;
        this.owner = owner;

        if (rainLoop != null) StopCoroutine(rainLoop);
        rainLoop = StartCoroutine(RainLoop());
    }

    /// <summary>
    /// 화살비 중단. 보스 사망 시 호출한다.
    /// </summary>
    public void StopRain()
    {
        if (rainLoop != null)
        {
            StopCoroutine(rainLoop);
            rainLoop = null;
        }
    }

    private IEnumerator RainLoop()
    {
        WaitForSeconds repeatWait = new WaitForSeconds(repeatInterval);

        while (true)
        {
            yield return StartCoroutine(SingleArrowRain());
            yield return repeatWait;
        }
    }

    private IEnumerator SingleArrowRain()
    {
        if (target == null || BulletPool.Instance == null) yield break;

        // 경고 구역 중심 = 플레이어 현재 위치
        Vector3 center = target.position;
        center.y = 0.01f;

        // 경고 표시
        GameObject warning = null;
        if (warningPrefab != null)
        {
            Quaternion flatRotation = Quaternion.Euler(90f, 0f, 0f);
            warning = Instantiate(warningPrefab, center, flatRotation);
            warning.transform.localScale = new Vector3(warningRadius * 2f, warningRadius * 2f, 1f);
        }

        yield return new WaitForSeconds(warningDuration);

        // 경고 제거
        if (warning != null)
        {
            Destroy(warning);
        }

        // 영역 내 플레이어에게 직접 데미지 (총알 물리 판정 대신 확실한 판정)
        if (target != null)
        {
            float distanceToCenter = Vector3.Distance(
                new Vector3(target.position.x, 0f, target.position.z),
                new Vector3(center.x, 0f, center.z));

            if (distanceToCenter <= warningRadius)
            {
                PlayerStateController playerState = target.GetComponent<PlayerStateController>();
                if (playerState != null)
                {
                    playerState.TakeDamage(rainDamage);
                }
            }
        }

        // 시각 효과용 화살 낙하 (데미지 없음)
        float interval = spawnDuration / bulletCount;
        for (int i = 0; i < bulletCount; i++)
        {
            if (BulletPool.Instance == null) yield break;

            Vector2 randomOffset = UnityEngine.Random.insideUnitCircle * warningRadius;
            Vector3 spawnPos = new Vector3(center.x + randomOffset.x, spawnHeight, center.z + randomOffset.y);

            Quaternion downRotation = Quaternion.LookRotation(Vector3.down);
            Bullet bullet = BulletPool.Instance.Get(bulletPrefab, spawnPos, downRotation);
            bullet.Speed = bulletSpeed;
            bullet.SetOwner(owner);

            yield return new WaitForSeconds(interval);
        }
    }
}
