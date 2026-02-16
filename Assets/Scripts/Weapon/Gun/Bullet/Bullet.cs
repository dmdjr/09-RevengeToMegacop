using UnityEngine;

public abstract class Bullet : MonoBehaviour
{
    [SerializeField] protected float speed = 5f;
    [SerializeField] protected float damage;

    private float destroyTime;

    private Vector3 direction = Vector3.forward;

    public void Reflect()
    {
        Vector3 targetDirection = MousePositionGetter.GetMousePositionInWorld(transform.position) - transform.position;

        Debug.Log($"targetDirection: {targetDirection}");
        transform.forward = new Vector3(targetDirection.x, 0, targetDirection.z).normalized;
        SetDestroyTime();
    }

    void Awake()
    {
        SetDestroyTime();
    }

    private void SetDestroyTime()
    {
        destroyTime = Time.time + 3f;
    }

    void Update()
    {
        transform.Translate(direction * (speed * Time.deltaTime));
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
        IDamageable damageable = other.GetComponent<IDamageable>();
        damageable?.Hit(this);
    }
}
