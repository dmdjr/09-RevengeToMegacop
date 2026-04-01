using System;
using System.Collections;

using UnityEngine;

/// <summary>
/// 1초간 조준선(LineRenderer)을 표시한 후 플레이어를 향해 단발 발사하는 패턴.
/// LineRenderer는 이 스크립트와 같은 GameObject에 붙여야 한다.
/// </summary>
public class AimedShotPattern : BossPattern
{
    [Header("Aim Settings")]
    [SerializeField] private float aimDuration = 1f;
    [SerializeField] private Color aimLineColor = Color.red;
    [SerializeField] private float aimLineWidth = 0.05f;
    [SerializeField] private float aimLineLength = 50f;

    [Header("Shot Settings")]
    [SerializeField] private GameObject bulletPrefab;
    [SerializeField] private float bulletSpeed = 25f;

    private LineRenderer lineRenderer;

    void Awake()
    {
        lineRenderer = GetComponent<LineRenderer>();
        if (lineRenderer == null)
        {
            lineRenderer = gameObject.AddComponent<LineRenderer>();
        }

        lineRenderer.positionCount = 2;
        lineRenderer.startWidth = aimLineWidth;
        lineRenderer.endWidth = aimLineWidth;
        lineRenderer.material = new Material(Shader.Find("Sprites/Default"));
        lineRenderer.startColor = aimLineColor;
        lineRenderer.endColor = aimLineColor;
        lineRenderer.enabled = false;
    }

    protected override void ExecutePattern(BossEnemy boss, Action onComplete)
    {
        StartCoroutine(AimAndShoot(boss, onComplete));
    }

    private IEnumerator AimAndShoot(BossEnemy boss, Action onComplete)
    {
        Transform target = boss.Target;
        if (target == null)
        {
            onComplete?.Invoke();
            yield break;
        }

        // 조준 방향 고정
        Vector3 aimDirection = (target.position - boss.transform.position).normalized;
        aimDirection.y = 0f;

        // 조준선 표시
        lineRenderer.enabled = true;
        float elapsed = 0f;

        while (elapsed < aimDuration)
        {
            Vector3 startPos = boss.transform.position;
            Vector3 endPos = startPos + aimDirection * aimLineLength;
            lineRenderer.SetPosition(0, startPos);
            lineRenderer.SetPosition(1, endPos);

            elapsed += Time.deltaTime;
            yield return null;
        }

        lineRenderer.enabled = false;

        // 발사
        if (BulletPool.Instance == null)
        {
            Debug.LogError("BulletPool.Instance is null. AimedShotPattern cannot fire.");
            onComplete?.Invoke();
            yield break;
        }

        Vector3 firePos = boss.transform.position;
        Quaternion fireRotation = Quaternion.LookRotation(aimDirection);
        Bullet bullet = BulletPool.Instance.Get(bulletPrefab, firePos, fireRotation);
        bullet.Speed = bulletSpeed;
        bullet.SetOwner(boss.gameObject);

        onComplete?.Invoke();
    }
}
