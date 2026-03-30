using System;

using UnityEngine;
using UnityEngine.AI;

public class Enemy : MonoBehaviour, IDamageable
{
    [SerializeField] private Transform weaponPoint;
    private Weapon weapon;
    private Transform target;
    [SerializeField] private float turnSpeed = 10f;
    [SerializeField] private float maxHp = 100f;
    private float hp;

    private Vector3 previousTargetPosition;

    [SerializeField] private float moveSpeed = 3f;
    [SerializeField] private bool useNavMesh = true;
    private NavMeshAgent agent;

    private enum State { Idle, MoveToTarget, Attack }
    private State currentState = State.Idle;

    public event Action<Enemy> OnDeath;
    public event Action<float> OnHpChanged;

    private bool isDead = false;

    public float MaxHp => maxHp;
    public float Hp => hp;
    public float HpRatio => maxHp > 0f ? hp / maxHp : 0f;
    /// <summary>
    /// 자식 클래스에서 현재 추적 대상(플레이어)에 접근할 때 사용한다.
    /// </summary>
    public Transform Target => target;
    /// <summary>
    /// NavMesh 사용 시 캐싱된 NavMeshAgent. useNavMesh가 false이거나 컴포넌트가 없으면 null.
    /// </summary>
    public NavMeshAgent NavAgent => agent;

    /// <summary>
    /// 자식 클래스에서 HP를 직접 변경할 때 사용한다. OnHpChanged 이벤트를 자동으로 발행한다.
    /// </summary>
    protected void SetHp(float value)
    {
        hp = value;
        OnHpChanged?.Invoke(HpRatio);
    }

    /// <summary>
    /// 피격 처리. 자식 클래스에서 override하여 피격 로직을 변경할 수 있다.
    /// </summary>
    public virtual void Hit(Bullet bullet)
    {
        if (bullet == null) return;

        SetHp(Mathf.Max(0f, hp - bullet.Damage));

        if (hp <= 0)
        {
            Die();
        }
    }

    /// <summary>
    /// 사망 처리. 자식 클래스에서 override하여 사망 연출을 추가할 수 있다.
    /// override 시 base.Die()를 호출해야 OnDeath 이벤트 발행 및 오브젝트 파괴가 수행된다.
    /// </summary>
    public virtual void Die()
    {
        if (isDead) return;
        isDead = true;
        OnDeath?.Invoke(this);
        Destroy(gameObject);
    }

    public void EquipWeapon(Weapon w)
    {
        if (w == null) return;
        if (weapon != null) Destroy(weapon.gameObject);
        weapon = w;
        weapon.transform.SetParent(weaponPoint, false);
        weapon.transform.localPosition = Vector3.zero;
    }

    public void SetTarget(Transform t)
    {
        target = t;
        if (t != null) previousTargetPosition = t.position;
    }

    /// <summary>
    /// 초기화. 자식 클래스에서 override할 경우 base.Start()를 호출해야 HP 초기화 등이 수행된다.
    /// </summary>
    protected virtual void Start()
    {
        hp = maxHp;
        if (target != null) previousTargetPosition = target.position;
        if (useNavMesh)
        {
            agent = GetComponent<NavMeshAgent>();
            if (agent != null) agent.updateRotation = false;
        }
    }

    /// <summary>
    /// 매 프레임 타겟 조준 및 FSM을 실행한다. 자식 클래스에서 override하여 독자적인 업데이트 루프를 사용할 수 있다.
    /// </summary>
    protected virtual void Update()
    {
        if (target == null)
        {
            currentState = State.Idle;
            return;
        }

        LookTarget();
        FSM();
    }

    protected void LookTarget()
    {
        if (weapon is GunWeapon gun)
        {
            AimToTarget(gun);
        }
        else
        {
            transform.LookAt(new Vector3(target.position.x, transform.position.y, target.position.z));
        }
    }

    private void AimToTarget(GunWeapon gun)
    {
        Vector3 targetVelocity = Time.deltaTime > 0f
            ? (target.position - previousTargetPosition) / Time.deltaTime
            : Vector3.zero;
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
        if (gun.BulletSpeed <= 0f) return target.position;
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
