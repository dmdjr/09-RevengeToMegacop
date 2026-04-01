using UnityEngine;
using UnityEngine.AI;

public class Stage1Boss : BossEnemy
{
    [SerializeField] private BasicShotPattern basicShotPattern;
    [SerializeField] private GuidedMissilePattern guidedMissilePattern;
    [SerializeField] private BombPattern bombPattern;
    [SerializeField] private WavePattern wavePattern;

    [SerializeField] private Transform player;
    [SerializeField] private Stage1BossShield shield;
    [SerializeField] private float attackRange = 10f;
    [SerializeField] private float bossMoveSpeed = 3f;

    private NavMeshAgent bossAgent;
    private bool isPatternExecuting;

    public void NotifyPatternStart() => isPatternExecuting = true;
    public void NotifyPatternEnd() => isPatternExecuting = false;

    protected override void Start()
    {
        base.Start();
        bossAgent = GetComponent<NavMeshAgent>();
        if (player != null) ActivateBoss(player);
        if (shield != null) shield.Initialize(player);
    }

    protected override void Update()
    {
        if (Target == null) return;

        float distance = Vector3.Distance(transform.position, Target.position);
        if (distance <= attackRange)
            base.Update(); // 사정거리 안에서만 패턴 실행

        MoveTowardTarget(distance);

        Vector3 direction = Target.position - transform.position;
        direction.y = 0f;
        if (direction != Vector3.zero)
            transform.rotation = Quaternion.LookRotation(direction);
    }

    private void MoveTowardTarget(float distance)
    {
        if (isPatternExecuting) return; // 패턴 실행 중에는 이동 안 함

        if (distance <= attackRange)
        {
            if (bossAgent != null) bossAgent.ResetPath();
            return;
        }

        if (bossAgent != null)
        {
            bossAgent.SetDestination(Target.position);
        }
        else
        {
            Vector3 dir = (Target.position - transform.position).normalized;
            dir.y = 0f;
            transform.position += bossMoveSpeed * Time.deltaTime * dir;
        }
    }

    public override void Hit(Bullet bullet)
    {
        if (shield != null && shield.gameObject.activeSelf)
            return;

        base.Hit(bullet);
        bullet.Remove();
        Debug.Log($"Boss hit! Remaining HP: {Hp}");
    }

    protected override BossPattern[] GetPatternsForPhase(int phaseIndex)
    {
        return new BossPattern[]
        {
            basicShotPattern,
            guidedMissilePattern,
            bombPattern,
            wavePattern
        };
    }

    protected override void OnPhaseChanged(int phaseIndex, BossPhaseData data) { }
}
