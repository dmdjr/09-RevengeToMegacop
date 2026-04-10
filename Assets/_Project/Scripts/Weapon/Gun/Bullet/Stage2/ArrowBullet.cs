using UnityEngine;

public class ArrowBullet : Bullet
{
    [SerializeField] private GameObject impactPrefab;
    [SerializeField] private float impactLifetime = 2f;

    private float enableTime;

    void OnEnable()
    {
        enableTime = Time.time;
    }

    protected override void OnTriggerEnter(Collider other)
    {
        if (Time.time < enableTime + 0.1f) return;
        if (other == null) return;

        GameObject obj = other.attachedRigidbody ? other.attachedRigidbody.gameObject : other.gameObject;

        if (obj.CompareTag("Enemy"))
        {
            // 반사탄이 적(보스/클론)에 맞음 → Hit + 제거
            base.OnTriggerEnter(other);
            SpawnImpact();
            Remove();
        }
        else
        {
            // 플레이어에 닿음 → base가 패링/피격 판단 (패링이면 Reflect, 피격이면 데미지)
            // 여기서 Remove 안 함 → 패링 시 반사돼서 계속 날아감
            base.OnTriggerEnter(other);
            SpawnImpact();
        }
    }
    private void SpawnImpact()
    {
        if (impactPrefab == null) return;
        GameObject impact = Instantiate(impactPrefab, transform.position, transform.rotation);
        Destroy(impact, impactLifetime);
        
    }
}
