using UnityEngine;

public class SwordController : MonoBehaviour
{
    [SerializeField] private float range = 20f;
    [SerializeField] private float lifeTime = 5f;
    [SerializeField] private float speed = 10f;

    private bool isArrived = false;
    private Vector3 targetPosition;

    void Awake()
    {
        targetPosition = transform.position;
    }

    public void Throw(Vector3 position)
    {
        targetPosition = Vector3.Distance(transform.position, position) <= range ? position : transform.position + (position - transform.position).normalized * range;
    }

    void Start()
    {
        Destroy(gameObject, lifeTime);
    }

    void Update()
    {
        if (isArrived) return;

        transform.position = Vector3.MoveTowards(transform.position, targetPosition, speed * Time.deltaTime);
        if (Vector3.Distance(transform.position, targetPosition) < 0.1f)
        {
            isArrived = true;
        }
    }
}
