using UnityEngine;

public class Stage1BossMissile : Bullet
{
    [SerializeField] private float turnSpeed = 180f;
    [SerializeField] private float lifetime = 5f;

    private Transform target;
    private Transform bossTransform;
    private float elapsed;
    private Vector3 lastForward;

    public void Launch(Transform playerTarget, Transform boss)
    {
        target = playerTarget;
        bossTransform = boss;
        elapsed = 0f;
        lastForward = transform.forward;
    }

    void Update()
    {
        elapsed += Time.deltaTime;
        if (elapsed >= lifetime)
        {
            Remove();
            return;
        }

        DetectAndSwitchTarget();

        if (target != null)
        {
            Vector3 direction = target.position - transform.position;
            direction.y = 0f;
            if (direction != Vector3.zero)
            {
                Quaternion targetRotation = Quaternion.LookRotation(direction);
                transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, turnSpeed * Time.deltaTime);
            }
        }

        transform.Translate(Vector3.forward * Speed * Time.deltaTime);
        lastForward = transform.forward;
    }

    private void DetectAndSwitchTarget()
    {
        Vector3 currentForwardH = new Vector3(transform.forward.x, 0f, transform.forward.z).normalized;
        Vector3 lastForwardH = new Vector3(lastForward.x, 0f, lastForward.z).normalized;
        if (Vector3.Dot(currentForwardH, lastForwardH) < 0.95f)
            target = bossTransform;
    }
}
