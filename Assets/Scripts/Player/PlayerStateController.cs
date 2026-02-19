using System;

using UnityEngine;

public class PlayerStateController : MonoBehaviour
{
    [SerializeField] private float hp;
    public float Hp
    {
        get => hp;
        private set
        {
            hp = value;
            if (hp < 0)
            {
                hp = 0;
            }
            NotifyUI();
        }
    }
    [SerializeField] private float maxHp = 100f;
    public float MaxHp { get => maxHp; private set => maxHp = value; }
    [SerializeField] private float executionGauge;
    public float ExecutionGauge
    {
        get => executionGauge;
        private set
        {
            executionGauge = value;
            if (executionGauge < 0)
            {
                executionGauge = 0;
            }
            NotifyUI();
        }
    }
    [SerializeField] private float maxExecutionGauge = 100f;
    public float MaxExecutionGauge { get => maxExecutionGauge; private set => maxExecutionGauge = value; }
    [SerializeField] private float executionGaugeIncreaseStep = 10f;
    public float ExecutionGaugeIncreaseStep { get => executionGaugeIncreaseStep; private set => executionGaugeIncreaseStep = value; }

    public event Action<float> OnHpChanged;
    public event Action<float> OnExecutionGaugeChanged;

    public void TakeDamage(float damage)
    {
        Hp -= damage;
    }

    public void IncreaseExecutionGauge()
    {
        ExecutionGauge += ExecutionGaugeIncreaseStep;
        if (MaxExecutionGauge < ExecutionGauge)
        {
            ExecutionGauge = MaxExecutionGauge;
        }
    }

    public bool CanExecute()
    {
        return MaxExecutionGauge <= ExecutionGauge;
    }

    public void Executed()
    {
        ExecutionGauge = 0;
    }

    private void NotifyUI()
    {
        OnHpChanged?.Invoke(Hp / MaxHp);
        OnExecutionGaugeChanged?.Invoke(ExecutionGauge / MaxExecutionGauge);
    }

    void OnValidate()
    {
        if (Application.isPlaying) NotifyUI();
    }
}
