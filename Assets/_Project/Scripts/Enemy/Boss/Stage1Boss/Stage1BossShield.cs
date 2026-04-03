using System;
using FXV;
using UnityEngine;

public class Stage1BossShield : MonoBehaviour, IDamageable
{
    [SerializeField] private float maxShieldGauge = 100f;
    [SerializeField] private Shield shield;
    private float shieldGauge;

    public float ShieldRatio => maxShieldGauge > 0f ? shieldGauge / maxShieldGauge : 0f;

    public event Action<float> OnShieldChanged; // 실드 게이지가 변할 때 비율(0~1)을 전달하는 이벤트    

    private Transform _target;

    private void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent<Bullet>(out var bullet))
        {
            Vector3 hitPos = other.ClosestPoint(transform.position);
            Vector3 hitNormal = (hitPos - transform.position).normalized;
            shield.OnHit(hitPos, hitNormal, 1.5f, 0.8f);
        }
    }

    public void Initialize(Transform target)
    {
        this._target = target;
        shieldGauge = maxShieldGauge;
    }

    public void Hit(Bullet bullet)
    {
        if (bullet == null) return;
        if (shieldGauge <= 0f) return;

        shieldGauge = Mathf.Max(shieldGauge - bullet.Damage, 0f);
        OnShieldChanged?.Invoke(ShieldRatio);
        Debug.Log($"Shield hit! Remaining Shield: {shieldGauge}/{maxShieldGauge}");
        ReflectToPlayer(bullet);

        if (shieldGauge <= 0f)
            gameObject.SetActive(false);
    }

    private void ReflectToPlayer(Bullet bullet)
    {
        if (_target == null) return;

        bullet.Reflect(gameObject, true);

        Vector3 direction = _target.position - bullet.transform.position;
        direction.y = 0f;
        if (direction != Vector3.zero)
            bullet.transform.forward = direction.normalized;
    }
}
