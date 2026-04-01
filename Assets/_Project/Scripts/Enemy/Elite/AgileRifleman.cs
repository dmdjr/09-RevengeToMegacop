using UnityEngine;

public class AgileRifleman : EliteEnemy
{
    private enum AgileRiflemanState
    {
        Approach,
        Reposition,
        ShootBurst,
        Cooldown
    }

    [Header("Agile Rifleman")]
    [SerializeField] private float preferredMinRange = 7f;
    [SerializeField] private float preferredMaxRange = 14f;

    [SerializeField] private float approachSpeed = 5.5f;
    [SerializeField] private float repositionSpeed = 8f;
    [SerializeField] private float repositionDuration = 0.6f;

    [SerializeField] private float burstDuration = 1.8f;
    [SerializeField] private float cooldownDuration = 0.7f;

    private AgileRiflemanState riflemanState = AgileRiflemanState.Approach;
    private float stateTimer = 0f;

    private Vector3 moveDirection;
    private bool repositionToRight = true;

    protected override void Start()
    {
        base.Start();
        ChangeState(AgileRiflemanState.Approach);
    }

    protected override void HandleBehavior()
    {
        switch (riflemanState)
        {
            case AgileRiflemanState.Approach:
                HandleApproach();
                break;

            case AgileRiflemanState.Reposition:
                HandleReposition();
                break;

            case AgileRiflemanState.ShootBurst:
                HandleShootBurst();
                break;

            case AgileRiflemanState.Cooldown:
                HandleCooldown();
                break;
        }
    }

    private void ChangeState(AgileRiflemanState newState)
    {
        riflemanState = newState;

        switch (riflemanState)
        {
            case AgileRiflemanState.Approach:
                stateTimer = 0f;
                break;

            case AgileRiflemanState.Reposition:
                stateTimer = repositionDuration;
                repositionToRight = Random.value > 0.5f;
                moveDirection = GetStrafeDirection(repositionToRight);
                break;

            case AgileRiflemanState.ShootBurst:
                stateTimer = burstDuration;
                break;

            case AgileRiflemanState.Cooldown:
                stateTimer = cooldownDuration;
                break;
        }
    }

    private void HandleApproach()
    {
        SetAimMode(AimMode.FaceTarget);

        float distance = DistanceToTarget();

        if (distance > preferredMaxRange)
        {
            MoveTowardsTarget(approachSpeed);
            return;
        }

        if (distance < preferredMinRange)
        {
            Vector3 awayDirection = GetDirectionAwayFromTarget();
            MoveInDirection(awayDirection, approachSpeed);
            return;
        }

        ChangeState(AgileRiflemanState.Reposition);
    }

    private void HandleReposition()
    {
        SetAimMode(AimMode.FaceTarget);

        float distance = DistanceToTarget();

        if (distance < preferredMinRange * 0.8f)
        {
            Vector3 awayDirection = GetDirectionAwayFromTarget();
            MoveInDirection(awayDirection, repositionSpeed);
        }
        else
        {
            RotateTowardsDirection(moveDirection);
            MoveInDirection(moveDirection, repositionSpeed);
        }

        stateTimer -= Time.deltaTime;
        if (stateTimer <= 0f)
        {
            ChangeState(AgileRiflemanState.ShootBurst);
        }
    }

    private void HandleShootBurst()
    {
        SetAimMode(AimMode.Predictive);
        RequestShoot();

        stateTimer -= Time.deltaTime;
        if (stateTimer <= 0f)
        {
            ChangeState(AgileRiflemanState.Cooldown);
        }
    }

    private void HandleCooldown()
    {
        SetAimMode(AimMode.FaceTarget);

        float distance = DistanceToTarget();

        if (distance > preferredMaxRange + 2f || distance < preferredMinRange - 2f)
        {
            ChangeState(AgileRiflemanState.Approach);
            return;
        }

        stateTimer -= Time.deltaTime;
        if (stateTimer <= 0f)
        {
            ChangeState(AgileRiflemanState.Reposition);
        }
    }

    private Vector3 GetDirectionToTarget()
    {
        if (Target == null) return transform.forward;

        Vector3 dir = Target.position - transform.position;
        dir.y = 0f;

        if (dir.sqrMagnitude < 0.001f)
        {
            return transform.forward;
        }

        return dir.normalized;
    }

    private Vector3 GetDirectionAwayFromTarget()
    {
        return -GetDirectionToTarget();
    }

    private Vector3 GetStrafeDirection(bool toRight)
    {
        Vector3 toTarget = GetDirectionToTarget();
        Vector3 sideDirection = Vector3.Cross(Vector3.up, toTarget).normalized;

        return toRight ? sideDirection : -sideDirection;
    }
}