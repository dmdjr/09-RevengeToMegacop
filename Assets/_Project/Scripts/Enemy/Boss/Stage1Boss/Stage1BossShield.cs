using System;

using UnityEngine;

public class Stage1BossShield : MonoBehaviour, IDamageable
{
    [SerializeField] private float maxShieldGauge = 100f;
    private float shieldGauge;

    public float ShieldRatio => maxShieldGauge > 0f ? shieldGauge / maxShieldGauge : 0f;

    public event Action<float> OnShieldChanged; // 실드 게이지가 변할 때 비율(0~1)을 전달하는 이벤트    

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

        bullet.Reflect(gameObject, true);

        Vector3 direction = target.position - bullet.transform.position;
        direction.y = 0f;
        if (direction != Vector3.zero)
            bullet.transform.forward = direction.normalized;
    }
}
