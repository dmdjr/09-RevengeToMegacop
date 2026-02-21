using UnityEngine;

public abstract class Bullet : MonoBehaviour
{
    public float Speed { get; set; }
    [field: SerializeField] public float Damage { get; private set; }

    private float destroyTime;

    public void Reflect(bool isParry)
    {
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

    void Awake()
    {
        SetDestroyTime();
    }

    private void SetDestroyTime(float destroyDelay = 3f)
    {
        destroyTime = Time.time + destroyDelay;
    }

    void Update()
    {
        transform.Translate(Vector3.forward * (Speed * Time.deltaTime));
        DestoryIfNeed();
    }

    private void DestoryIfNeed()
    {
        if (destroyTime < Time.time)
        {
            Destroy(gameObject);
        }
    }


    void OnTriggerEnter(Collider other)
    {
        if (other == null) return;
        GameObject obj = other.attachedRigidbody ? other.attachedRigidbody.gameObject : other.gameObject;
        IDamageable damageable = obj.GetComponent<IDamageable>();
        damageable?.Hit(this);
    }
}
