using System;
using System.Collections;
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
    [SerializeField] private float deathSinkAmount = 0.7f;
    [SerializeField] private float deathSinkDuration = 1.5f;
    [SerializeField] private Animator bossAnimator;

    private NavMeshAgent bossAgent;
    private bool isPatternExecuting;
    private Action pendingFireCallback;
    private Action pendingAnimationCompleteCallback;

    public Animator BossAnimator => bossAnimator;

    public void NotifyPatternStart() => isPatternExecuting = true;
    public void NotifyPatternEnd() => isPatternExecuting = false;

    public void RegisterFireCallback(Action callback) => pendingFireCallback = callback;
    public void RegisterAnimationCompleteCallback(Action callback) => pendingAnimationCompleteCallback = callback;

    // Animation Event에서 호출 — 발사 시작 신호
    public void OnFireAnimationEvent()
    {
        pendingFireCallback?.Invoke();
        pendingFireCallback = null;
    }

    // Animation Event에서 호출 — 애니메이션 완료 신호
    public void OnAnimationCompleteEvent()
    {
        pendingAnimationCompleteCallback?.Invoke();
        pendingAnimationCompleteCallback = null;
    }

    protected override void Start()
    {
        base.Start();
        bossAgent = GetComponent<NavMeshAgent>();
        if (player != null) ActivateBoss(player);
        if (shield != null)
        {
            shield.Initialize(player);
            shield.OnShieldChanged += OnShieldChanged;
        }
        bossAnimator?.SetBool("HasShield", shield != null && shield.gameObject.activeSelf);
    }

    private void OnShieldChanged(float ratio)
    {
        if (ratio <= 0f)
            bossAnimator?.SetBool("HasShield", false);
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
        // bossAnimator?.SetTrigger("Hit");
        
        if (bullet is not Stage1BossBomb bomb)
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

    protected override IEnumerator OnBossDeath()
    {
        SetTarget(null);
        if (bossAgent != null) bossAgent.ResetPath();
        bool deathAnimComplete = false;
        RegisterAnimationCompleteCallback(() => deathAnimComplete = true);
        bossAnimator?.SetTrigger("Die");
        StartCoroutine(SinkDown());
        yield return new WaitUntil(() => deathAnimComplete);
    }

    private IEnumerator SinkDown()
    {
        yield return new WaitForSeconds(3.5f); // 애니메이션과 싱크 맞추기 위한 딜레이
        Vector3 startPos = transform.position;
        Vector3 endPos = startPos - new Vector3(0f, deathSinkAmount, 0f);
        float elapsed = 0f;
        while (elapsed < deathSinkDuration)
        {
            elapsed += Time.deltaTime;
            transform.position = Vector3.Lerp(startPos, endPos, elapsed / deathSinkDuration);
            yield return null;
        }
        transform.position = endPos;
    }
}
