using UnityEngine;

public class Stage1Boss : BossEnemy
{
    [SerializeField] private BasicShotPattern basicShotPattern;
    [SerializeField] private GuidedMissilePattern guidedMissilePattern;
    [SerializeField] private BombPattern bombPattern;
    [SerializeField] private WavePattern wavePattern;

    [SerializeField] private Transform player;
    [SerializeField] private Stage1BossShield shield;

    protected override void Start()
    {
        base.Start();
        if (player != null) ActivateBoss(player);
        if (shield != null) shield.Initialize(player);
    }

    protected override void Update()
    {
        base.Update();
        if (Target == null) return;

        Vector3 direction = Target.position - transform.position;
        direction.y = 0f;
        if (direction != Vector3.zero)
            transform.rotation = Quaternion.LookRotation(direction);
    }

    public override void Hit(Bullet bullet)
    {
        base.Hit(bullet);
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
