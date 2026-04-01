using System.Collections;
using UnityEngine;

namespace Boss3
{
    public class Stage3Boss : BossEnemy
{
    
    [SerializeField] private ScopePattern _scopePattern;
    [SerializeField] private SmokeBomb _smokeBombPattern;
    [SerializeField] private OscillatingBulletPattern _OscillatingBulletPattern;
    [SerializeField] private MovePattern _movePattern;

        

        protected override BossPattern[] GetPatternsForPhase(int phaseIndex)
    {
        if (phaseIndex == 0)
        {
            return new BossPattern[] 
            {
                
                _OscillatingBulletPattern,
                _movePattern
                
            };
        }
        if (phaseIndex == 1)
        {
            return new BossPattern[]
            {
                _OscillatingBulletPattern,
                _smokeBombPattern,
                _scopePattern,
                _movePattern
            };
        }

        return new BossPattern[0];
    }

    protected override void OnPhaseChanged(int phaseIndex, BossPhaseData data)
    {
        Debug.Log($"페이즈 {phaseIndex + 1} 진입!");
    }

    public override void Hit(Bullet bullet)
    {
            

            base.Hit(bullet);
            Debug.Log("hit" + bullet);
            
    }

    protected override IEnumerator OnBossIntro()
    {
        FindAnyObjectByType<BossUI>().Initialize(this);
        return base.OnBossIntro();
    }
}
}