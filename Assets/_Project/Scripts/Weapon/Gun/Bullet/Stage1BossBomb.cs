using UnityEngine;

public class Stage1BossBomb : Bullet
{
    [SerializeField] private float bombSpeed = 8f;
    [SerializeField] private float arcHeight = 4f;
    [SerializeField] private float fuseTime = 4f;
    [SerializeField] private float explosionRadius = 3f;

    private Vector3 startPos;
    private Vector3 targetPos;
    private float flightTime;
    private float launchDistance;
    private float elapsed;
    private float totalElapsed;
    private bool isLaunched;
    private Vector3 lastForward;

    public void Launch(Vector3 start, Vector3 target)
    {
        startPos = start;
        targetPos = target;
        flightTime = CalculateFlightTime(start, target);
        launchDistance = Vector3.Distance(new Vector3(start.x, 0f, start.z), new Vector3(target.x, 0f, target.z));
        elapsed = 0f;
        totalElapsed = 0f;
        isLaunched = true;

        Vector3 horizontal = new Vector3(target.x - start.x, 0f, target.z - start.z);
        if (horizontal.sqrMagnitude > 0.01f)
            transform.forward = horizontal.normalized;
        lastForward = transform.forward;
    }

    private float CalculateFlightTime(Vector3 from, Vector3 to)
    {
        float distance = Vector3.Distance(new Vector3(from.x, 0f, from.z), new Vector3(to.x, 0f, to.z));
        return distance / bombSpeed;
    }

    void Update()
    {
        if (!isLaunched) return;

        elapsed += Time.deltaTime;
        totalElapsed += Time.deltaTime;

        DetectAndApplyReflect();
        ApplyParabolicMovement();

        if (elapsed >= flightTime || totalElapsed >= fuseTime)
            Explode();
    }

    private void DetectAndApplyReflect()
    {
        Vector3 currentForwardH = new Vector3(transform.forward.x, 0f, transform.forward.z).normalized;
        Vector3 lastForwardH = new Vector3(lastForward.x, 0f, lastForward.z).normalized;
        if (Vector3.Dot(currentForwardH, lastForwardH) < 0.95f)
        {
            startPos = transform.position;
            targetPos = transform.position + currentForwardH * launchDistance;
            targetPos.y = 0f;
            flightTime = launchDistance / bombSpeed;
            elapsed = 0f;
        }
    }

    private void ApplyParabolicMovement()
    {
        float t = Mathf.Clamp01(elapsed / flightTime);
        Vector3 pos = Vector3.Lerp(startPos, targetPos, t);
        pos.y = Mathf.Lerp(startPos.y, targetPos.y, t) + arcHeight * Mathf.Sin(Mathf.PI * t);
        transform.position = pos;

        lastForward = transform.forward;
    }

    void OnDrawGizmos()
    {
        Gizmos.color = new Color(1f, 0.3f, 0f, 0.3f);
        Gizmos.DrawSphere(transform.position, explosionRadius);
        Gizmos.color = new Color(1f, 0.3f, 0f, 1f);
        Gizmos.DrawWireSphere(transform.position, explosionRadius);
    }

    private void Explode()
    {
        isLaunched = false;

        Collider[] hits = Physics.OverlapSphere(transform.position, explosionRadius);
        foreach (var hit in hits)
        {
            GameObject obj = hit.attachedRigidbody ? hit.attachedRigidbody.gameObject : hit.gameObject;
            PlayerStateController playerState = obj.GetComponent<PlayerStateController>();
            if (playerState != null)
            {
                playerState.TakeDamage(Damage);
                break;
            }
        }

        Remove();
    }
}
