using UnityEngine;

public class ShieldCharger : EliteEnemy
{
    private enum ShieldChargerState
    {
        Chase,
        Prepare,
        Dash,
        Recover,
        Shoot,
        Cooldown
    }

    [Header("Shield Charger")]
    [SerializeField] private float chaseSpeed = 6f;
    [SerializeField] private float dashTriggerRange = 8f;

    [SerializeField] private float prepareDuration = 0.45f;
    [SerializeField] private float dashSpeed = 15f;
    [SerializeField] private float dashDuration = 0.7f;
    [SerializeField] private float recoverDuration = 0.8f;
    [SerializeField] private float shootDuration = 2.2f;
    [SerializeField] private float cooldownDuration = 1f;

    private ShieldChargerState shieldState = ShieldChargerState.Chase;
    private float stateTimer = 0f;
    private Vector3 dashDirection;

    protected override void Start()
    {
        base.Start();
        ChangeState(ShieldChargerState.Chase);
    }

    protected override void HandleBehavior()
    {
        switch (shieldState)
        {
            case ShieldChargerState.Chase:
                HandleChase();
                break;

            case ShieldChargerState.Prepare:
                HandlePrepare();
                break;

            case ShieldChargerState.Dash:
                HandleDash();
                break;

            case ShieldChargerState.Recover:
                HandleRecover();
                break;

            case ShieldChargerState.Shoot:
                HandleShoot();
                break;

            case ShieldChargerState.Cooldown:
                HandleCooldown();
                break;
        }
    }

    private void ChangeState(ShieldChargerState newState)
    {
        shieldState = newState;

        switch (shieldState)
        {
            case ShieldChargerState.Chase:
                stateTimer = 0f;
                break;

            case ShieldChargerState.Prepare:
                stateTimer = prepareDuration;
                dashDirection = GetDirectionToTarget();
                break;

            case ShieldChargerState.Dash:
                stateTimer = dashDuration;
                break;

            case ShieldChargerState.Recover:
                stateTimer = recoverDuration;
                break;

            case ShieldChargerState.Shoot:
                stateTimer = shootDuration;
                break;

            case ShieldChargerState.Cooldown:
                stateTimer = cooldownDuration;
                break;
        }
    }

    private void HandleChase()
    {
        SetAimMode(AimMode.FaceTarget);

        float distance = DistanceToTarget();
        if (distance > dashTriggerRange)
        {
            MoveTowardsTarget(chaseSpeed);
        }
        else
        {
            ChangeState(ShieldChargerState.Prepare);
        }
    }

    private void HandlePrepare()
    {
        SetAimMode(AimMode.FaceTarget);

        dashDirection = GetDirectionToTarget();

        stateTimer -= Time.deltaTime;
        if (stateTimer <= 0f)
        {
            ChangeState(ShieldChargerState.Dash);
        }
    }

    private void HandleDash()
    {
        SetAimMode(AimMode.None);

        RotateTowardsDirection(dashDirection);
        MoveInDirection(dashDirection, dashSpeed);

        stateTimer -= Time.deltaTime;
        if (stateTimer <= 0f)
        {
            ChangeState(ShieldChargerState.Recover);
        }
    }

    private void HandleRecover()
    {
        SetAimMode(AimMode.FaceTarget);

        stateTimer -= Time.deltaTime;
        if (stateTimer <= 0f)
        {
            ChangeState(ShieldChargerState.Shoot);
        }
    }

    private void HandleShoot()
    {
        SetAimMode(AimMode.Predictive);
        RequestShoot();

        stateTimer -= Time.deltaTime;
        if (stateTimer <= 0f)
        {
            ChangeState(ShieldChargerState.Cooldown);
        }
    }

    private void HandleCooldown()
    {
        SetAimMode(AimMode.FaceTarget);

        stateTimer -= Time.deltaTime;
        if (stateTimer <= 0f)
        {
            ChangeState(ShieldChargerState.Chase);
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
}