using System;

using UnityEngine;
using UnityEngine.AI;

public class Enemy : MonoBehaviour, IDamageable
{
    [SerializeField] private Transform weaponPoint;
    private Weapon weapon;
    private Transform target;
    [SerializeField] private float turnSpeed = 10f;
    [SerializeField] private float hp = 100f;

    private Vector3 previousTargetPosition;

    [SerializeField] private float moveSpeed = 3f;
    [SerializeField] private bool useNavMesh = true;
    private NavMeshAgent agent;

    private enum State { Idle, MoveToTarget, Attack }
    private State currentState = State.Idle;

    public event Action<GameObject> OnDeath;

    private bool isDead = false;

    public void Hit(Bullet bullet)
    {
        if (bullet == null) return;

        hp -= bullet.Damage;

        if (hp <= 0)
        {
            Die();
        }
    }

    public void Die()
    {
        if (isDead) return;
        isDead = true;
        OnDeath?.Invoke(gameObject);
        Destroy(gameObject);
    }

    public void EquipWeapon(Weapon w)
    {
        if (w == null) return;
        weapon = w;
        weapon.transform.SetParent(weaponPoint, false);
        weapon.transform.localPosition = Vector3.zero;
    }

    public void SetTarget(Transform t)
    {
        target = t;
    }

    void Start()
    {
        if (target != null) previousTargetPosition = target.position;
        if (useNavMesh)
        {
            agent = GetComponent<NavMeshAgent>();
            if (agent != null) agent.updateRotation = false;
        }
    }

    void Update()
    {
        if (target == null)
        {
            currentState = State.Idle;
            return;
        }

        LookTarget();
        FSM();
    }

    private void LookTarget()
    {
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

    private void FSM()
    {
        switch (currentState)
        {
            case State.Idle:
                currentState = State.MoveToTarget;
                break;

            case State.MoveToTarget:
                if (IsTargetInRange())
                {
                    currentState = State.Attack;
                    if (agent != null) agent.ResetPath();
                }
                else
                {
                    MoveTowardsTarget();
                }
                break;

            case State.Attack:
                if (!IsTargetInRange())
                {
                    currentState = State.MoveToTarget;
                }
                else
                {
                    Attack();
                }
                break;
        }
    }

    public bool IsTargetInRange()
    {
        if (weapon == null) return false;
        float distance = Vector3.Distance(transform.position, target.position);
        return distance <= weapon.Range;
    }

    private void MoveTowardsTarget()
    {
        if (useNavMesh && agent != null)
        {
            agent.SetDestination(target.position);
        }
        else
        {
            Vector3 dir = (target.position - transform.position).normalized;
            dir.y = 0f;
            transform.position += moveSpeed * Time.deltaTime * dir;
        }
    }

    private void Attack()
    {
        if (weapon == null) return;
        if (weapon is GunWeapon gun)
        {
            if (gun.CanFire())
            {
                gun.TryUse();
            }
            else
            {
                gun.Reload();
            }
        }
        else
        {
            weapon.TryUse();
        }
    }
}
