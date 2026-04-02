using UnityEngine;

public class EliteEnemy : Enemy
{
    protected enum AimMode
    {
        None,
        FaceTarget,
        Predictive
    }

    [Header("Elite Test Fallback")]
    [SerializeField] protected GameObject defaultGunWeaponPrefab;

    [Header("Elite Common")]
    [SerializeField] private float aimTurnSpeed = 10f;

    protected GunWeapon gunWeapon;

    private Vector3 trackedTargetPreviousPosition;
    private Vector3 trackedTargetVelocity;

    private AimMode currentAimMode = AimMode.FaceTarget;
    private bool shootRequested = false;

    protected override void Start()
    {
        base.Start();

        SetupTargetIfNeeded();
        SetupWeaponIfNeeded();

        if (Target != null)
        {
            trackedTargetPreviousPosition = Target.position;
        }
    }

    protected override void Update()
    {
        if (Target == null) return;

        UpdateTrackedTargetVelocity();

        currentAimMode = AimMode.FaceTarget;
        shootRequested = false;

        HandleBehavior();
        ProcessCombat();
    }

    /// <summary>
    /// 자식 클래스가 행동 패턴만 정의한다.
    /// 실제 조준/사격 실행은 EliteEnemy가 공통 처리한다.
    /// </summary>
    protected virtual void HandleBehavior()
    {
    }

    #region Target / Weapon Setup

    protected void SetupTargetIfNeeded()
    {
        if (Target != null) return;

        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            SetTarget(player.transform);
        }
    }

    protected void SetupWeaponIfNeeded()
    {
        if (gunWeapon != null) return;

        GunWeapon existingGun = GetComponentInChildren<GunWeapon>();
        if (existingGun != null)
        {
            gunWeapon = existingGun;
            return;
        }

        if (defaultGunWeaponPrefab == null) return;

        GameObject weaponObj = Instantiate(defaultGunWeaponPrefab);
        Weapon weaponComponent = weaponObj.GetComponent<Weapon>();
        if (weaponComponent == null)
        {
            Destroy(weaponObj);
            return;
        }

        EquipWeapon(weaponComponent);
        gunWeapon = weaponComponent as GunWeapon;
    }

    #endregion

    #region Combat Common

    private void UpdateTrackedTargetVelocity()
    {
        if (Target == null) return;

        float dt = Time.deltaTime;
        if (dt > 0f)
        {
            trackedTargetVelocity = (Target.position - trackedTargetPreviousPosition) / dt;
        }
        else
        {
            trackedTargetVelocity = Vector3.zero;
        }

        trackedTargetPreviousPosition = Target.position;
    }

    private void ProcessCombat()
    {
        ExecuteAim();

        if (shootRequested)
        {
            TryFire();
        }
    }

    private void ExecuteAim()
    {
        switch (currentAimMode)
        {
            case AimMode.None:
                break;

            case AimMode.FaceTarget:
                LookAtTarget();
                break;

            case AimMode.Predictive:
                if (gunWeapon != null)
                {
                    AimToTarget(gunWeapon);
                }
                else
                {
                    LookAtTarget();
                }
                break;
        }
    }

    /// <summary>
    /// 발사를 시도한다.
    /// 발사 가능하면 TryUse(), 탄이 없으면 Reload()를 호출한다.
    /// </summary>
    protected bool TryFire()
    {
        if (gunWeapon == null) return false;

        if (gunWeapon.CanFire())
        {
            gunWeapon.TryUse();
            return true;
        }

        gunWeapon.Reload();
        return false;
    }

    protected void RequestShoot()
    {
        shootRequested = true;
    }

    protected void SetAimMode(AimMode mode)
    {
        currentAimMode = mode;
    }

    #endregion

    #region Aim

    protected void LookAtTarget()
    {
        if (Target == null) return;

        Vector3 dir = Target.position - transform.position;
        dir.y = 0f;

        if (dir.sqrMagnitude < 0.001f) return;

        Quaternion targetRotation = Quaternion.LookRotation(dir);
        transform.rotation = Quaternion.Slerp(
            transform.rotation,
            targetRotation,
            aimTurnSpeed * Time.deltaTime
        );
    }

    protected void RotateTowardsDirection(Vector3 direction)
    {
        direction.y = 0f;

        if (direction.sqrMagnitude < 0.001f) return;

        Quaternion targetRotation = Quaternion.LookRotation(direction.normalized);
        transform.rotation = Quaternion.Slerp(
            transform.rotation,
            targetRotation,
            aimTurnSpeed * Time.deltaTime
        );
    }

    protected void AimToTarget(GunWeapon gun)
    {
        if (Target == null || gun == null) return;

        Vector3 leadPosition = CalculateLeadPosition(gun);

        Vector3 directionToLead = leadPosition - gun.transform.position;
        directionToLead.y = 0f;

        if (directionToLead.sqrMagnitude < 0.001f) return;

        Quaternion lookRotation = Quaternion.LookRotation(directionToLead);
        transform.rotation = Quaternion.Lerp(
            transform.rotation,
            lookRotation,
            Time.deltaTime * aimTurnSpeed
        );
    }

    protected Vector3 CalculateLeadPosition(GunWeapon gun)
    {
        if (Target == null) return transform.position;
        if (gun == null) return Target.position;
        if (gun.BulletSpeed <= 0f) return Target.position;

        float distance = Vector3.Distance(gun.transform.position, Target.position);
        float timeToTarget = distance / gun.BulletSpeed;

        return Target.position + (trackedTargetVelocity * timeToTarget);
    }

    #endregion

    #region Movement Helpers

    protected float DistanceToTarget()
    {
        if (Target == null) return Mathf.Infinity;
        return Vector3.Distance(transform.position, Target.position);
    }

    protected void MoveTowardsTarget(float speed)
    {
        if (Target == null) return;

        Vector3 dir = Target.position - transform.position;
        dir.y = 0f;

        if (dir.sqrMagnitude < 0.001f) return;

        transform.position += dir.normalized * speed * Time.deltaTime;
    }

    protected void MoveInDirection(Vector3 direction, float speed)
    {
        direction.y = 0f;

        if (direction.sqrMagnitude < 0.001f) return;

        transform.position += direction.normalized * speed * Time.deltaTime;
    }

    #endregion
}