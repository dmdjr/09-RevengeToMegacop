using UnityEngine;

public abstract class Bullet : MonoBehaviour
{
    public float Speed { get; set; }
    [field: SerializeField] public float Damage { get; private set; }

    private float destroyTime;
    private bool isReflected = false;
    private bool isReleased = false;
    private GameObject owner;
    private GameObject prefab;

    internal void SetPrefab(GameObject prefab)
    {
        this.prefab = prefab;
    }

    // 풀에서 꺼낼 때마다 호출되어 상태를 초기화한다
    public void Prepare()
    {
        Speed = 0f;
        isReflected = false;
        isReleased = false;
        owner = null;
        SetDestroyTime();
    }

    public void SetOwner(GameObject owner)
    {
        this.owner = owner;
    }

    public void Reflect(GameObject owner, bool isParry)
    {
        this.owner = owner;
        isReflected = true;
        Vector3 targetDirection = isParry ? GetParryDirection() : GetRandomDirection();

        transform.forward = new Vector3(targetDirection.x, 0, targetDirection.z);
        SetDestroyTime(isParry ? 3f : 0.3f);
    }

    private Vector3 GetParryDirection()
    {
        return (MousePositionGetter.GetMousePositionInWorld(transform.position) - transform.position).normalized;
    }

    private Vector3 GetRandomDirection()
    {
        float randomDegree = Random.Range(-60f, 60f);
        return Quaternion.Euler(0, randomDegree, 0) * GetParryDirection();
    }

    private void SetDestroyTime(float destroyDelay = 3f)
    {
        destroyTime = Time.time + destroyDelay;
    }

    void Update()
    {
        transform.Translate(Vector3.forward * (Speed * Time.deltaTime));
        if (destroyTime < Time.time) Remove();
    }

    virtual protected void OnTriggerEnter(Collider other)
    {
        if (other == null) return;
        GameObject obj = other.attachedRigidbody ? other.attachedRigidbody.gameObject : other.gameObject;
        if (obj == owner) return;

        if (!isReflected && obj.CompareTag("Enemy")) return;

        IDamageable damageable = obj.GetComponent<IDamageable>();
        if (damageable != null)
            damageable.Hit(this);
    }

    public void Remove()
    {
        if (isReleased) return;
        isReleased = true;

        if (prefab == null)
        {
            Destroy(gameObject);
            return;
        }
        BulletPool.Instance.Release(prefab, this);
    }
}
