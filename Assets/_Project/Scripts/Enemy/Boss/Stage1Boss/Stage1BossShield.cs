using System;

using UnityEngine;

public class Stage1BossShield : MonoBehaviour, IDamageable
{
    [SerializeField] private float maxShieldGauge = 100f;
    private float shieldGauge;

    public float ShieldRatio => maxShieldGauge > 0f ? shieldGauge / maxShieldGauge : 0f;

    public event Action<float> OnShieldChanged;

    private Transform target;

    public void Initialize(Transform target)
    {
        this.target = target;
        shieldGauge = maxShieldGauge;
    }

    public void Hit(Bullet bullet)
    {
        if (bullet == null) return;
        if (shieldGauge <= 0f) return;

        shieldGauge = Mathf.Max(shieldGauge - bullet.Damage, 0f);
        OnShieldChanged?.Invoke(ShieldRatio);

        ReflectToPlayer(bullet);

        if (shieldGauge <= 0f)
            gameObject.SetActive(false);
    }

    private void ReflectToPlayer(Bullet bullet)
    {
        if (target == null) return;

        bullet.Reflect(gameObject, false);

        Vector3 direction = target.position - bullet.transform.position;
        direction.y = 0f;
        if (direction != Vector3.zero)
            bullet.transform.forward = direction.normalized;
    }
}
