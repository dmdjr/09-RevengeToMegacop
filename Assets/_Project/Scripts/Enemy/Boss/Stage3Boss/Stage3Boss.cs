using UnityEngine;

public class Stage3Boss : BossEnemy
{
    private FireAtRandom _fireAtRandomPattern;
    private ScopePattern _scopePattern;
    private SmokeBomb _SmokeBombPattern;
    
    protected override BossPattern[] GetPatternsForPhase(int phaseIndex)
    {
        if (phaseIndex == 1)
        {
            return new BossPattern[] 
            {
                _fireAtRandomPattern,
                _scopePattern,
                _SmokeBombPattern
            };
        }

        return new BossPattern[0];
    }

    protected override void OnPhaseChanged(int phaseIndex, BossPhaseData data)
    {
        throw new System.NotImplementedException();
    }
}
