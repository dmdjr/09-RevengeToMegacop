using UnityEngine;

public class Disruptor : EliteEnemy
{
    private enum DisruptorState
    {
        Approach,
        PrepareDisrupt,
        Disrupt,
        Shoot,
        Cooldown
    }

    [Header("Disruptor")]
    [SerializeField] private float preferredMinRange = 9f;
    [SerializeField] private float preferredMaxRange = 16f;

    [SerializeField] private float disruptMoveSpeed = 4.2f;

    [SerializeField] private float prepareDuration = 0.8f;
    [SerializeField] private float disruptDuration = 1.0f;
    [SerializeField] private float shootDuration = 2.4f;
    [SerializeField] private float cooldownDuration = 1.1f;

    [SerializeField] private bool usePredictiveAimWhileShooting = true;

    private DisruptorState disruptorState = DisruptorState.Approach;
    private float stateTimer = 0f;

    protected override void Start()
    {
        base.Start();
        ChangeState(DisruptorState.Approach);
    }

    protected override void HandleBehavior()
    {
        switch (disruptorState)
        {
            case DisruptorState.Approach:
                HandleApproach();
                break;

            case DisruptorState.PrepareDisrupt:
                HandlePrepareDisrupt();
                break;

            case DisruptorState.Disrupt:
                HandleDisrupt();
                break;

            case DisruptorState.Shoot:
                HandleShoot();
                break;

            case DisruptorState.Cooldown:
                HandleCooldown();
                break;
        }
    }

    private void ChangeState(DisruptorState newState)
    {
        disruptorState = newState;

        switch (disruptorState)
        {
            case DisruptorState.Approach:
                stateTimer = 0f;
                break;

            case DisruptorState.PrepareDisrupt:
                stateTimer = prepareDuration;
                break;

            case DisruptorState.Disrupt:
                stateTimer = disruptDuration;
                OnDisruptTriggered();
                break;

            case DisruptorState.Shoot:
                stateTimer = shootDuration;
                break;

            case DisruptorState.Cooldown:
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
            MoveTowardsTarget(disruptMoveSpeed);
            return;
        }

        if (distance < preferredMinRange)
        {
            Vector3 awayDirection = GetDirectionAwayFromTarget();
            MoveInDirection(awayDirection, disruptMoveSpeed);
            return;
        }

        ChangeState(DisruptorState.PrepareDisrupt);
    }

    private void HandlePrepareDisrupt()
    {
        SetAimMode(AimMode.FaceTarget);

        stateTimer -= Time.deltaTime;
        if (stateTimer <= 0f)
        {
            ChangeState(DisruptorState.Disrupt);
        }
    }

    private void HandleDisrupt()
    {
        SetAimMode(AimMode.FaceTarget);

        stateTimer -= Time.deltaTime;
        if (stateTimer <= 0f)
        {
            ChangeState(DisruptorState.Shoot);
        }
    }

    private void HandleShoot()
    {
        SetAimMode(usePredictiveAimWhileShooting ? AimMode.Predictive : AimMode.FaceTarget);
        RequestShoot();

        stateTimer -= Time.deltaTime;
        if (stateTimer <= 0f)
        {
            ChangeState(DisruptorState.Cooldown);
        }
    }

    private void HandleCooldown()
    {
        SetAimMode(AimMode.FaceTarget);

        float distance = DistanceToTarget();

        if (distance > preferredMaxRange + 2f || distance < preferredMinRange - 2f)
        {
            ChangeState(DisruptorState.Approach);
            return;
        }

        stateTimer -= Time.deltaTime;
        if (stateTimer <= 0f)
        {
            ChangeState(DisruptorState.PrepareDisrupt);
        }
    }

    private void OnDisruptTriggered()
    {
        Debug.Log($"{name}: Disrupt triggered.");
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
}