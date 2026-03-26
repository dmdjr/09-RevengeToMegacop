using UnityEngine;

public class Stage1Boss : BossEnemy
{
    private GuidedMissilePattern _guidedMissilePattern;
    private BombPattern _bombPattern;
    private WavePattern _wavePattern;

    protected override BossPattern[] GetPatternsForPhase(int phaseIndex)
    {
        if (phaseIndex == 1)
        {
            return new BossPattern[]
            {
                _guidedMissilePattern,
                _bombPattern,
                _wavePattern
            };
        }

        return new BossPattern[0];
    }

    protected override void OnPhaseChanged(int phaseIndex, BossPhaseData data)
    {
        throw new System.NotImplementedException();
    }
}
