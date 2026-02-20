using UnityEngine;

public class Enemy : MonoBehaviour, IDamageable
{
    [SerializeField] private Weapon weapon;
    [SerializeField] private Transform target;
    [SerializeField] private float turnSpeed = 10f;
    [SerializeField] private float hp = 100f;

    private Vector3 previousTargetPosition;

    public void Hit(Bullet bullet)
    {
        if (bullet == null) return;

        hp -= bullet.Damage;

        if (hp <= 0)
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        if (target != null) previousTargetPosition = target.position;
    }

    void Update()
    {
        LookTarget();
        UseWeapon();
    }

    private void LookTarget()
    {
        if (target == null) return;

        if (weapon is GunWeapon gun)
        {
            AimToTarget(gun);
        }
        else
        {
            transform.LookAt(target);
        }
    }

    private void AimToTarget(GunWeapon gun)
    {
        Vector3 targetVelocity = (target.position - previousTargetPosition) / Time.deltaTime;
        previousTargetPosition = target.position;

        Vector3 leadPosition = CalculateLeadPosition(gun, targetVelocity);

        Vector3 directionToLead = leadPosition - gun.transform.position;
        directionToLead.y = 0;

        if (directionToLead != Vector3.zero)
        {
            Quaternion lookRotation = Quaternion.LookRotation(directionToLead);
            transform.rotation = Quaternion.Lerp(transform.rotation, lookRotation, Time.deltaTime * turnSpeed);
        }
    }

    private Vector3 CalculateLeadPosition(GunWeapon gun, Vector3 targetVelocity)
    {
        float distance = Vector3.Distance(gun.transform.position, target.position);
        float timeToTarget = distance / gun.BulletSpeed;
        return target.position + (targetVelocity * timeToTarget);
    }

    private void UseWeapon()
    {
        if (weapon == null) return;

        if (weapon is GunWeapon) UseGunWeapon(weapon as GunWeapon);
        else weapon.TryUse();
    }

    private void UseGunWeapon(GunWeapon gun)
    {
        if (!IsTargetInRange(gun)) return;

        if (gun.CanFire()) gun.TryUse(); else gun.Reload();
    }

    
    public bool IsTargetInRange(GunWeapon gun)
    {
        if (target == null) return false;

        float distance = Vector3.Distance(transform.position, target.position);
        return distance <= gun.Range;
    }
}