using UnityEngine;

public class UIController : MonoBehaviour
{
    [SerializeField] private GameObject hp;
    [SerializeField] private GameObject executionGauge;
    [SerializeField] private PlayerStateController playerStateController;

    void Start()
    {
        if (hp == null || executionGauge == null || playerStateController == null)
        {
            Debug.LogError("One or more required components are not assigned in UIController.");
            return;
        }
        playerStateController.OnHpChanged += UpdateHp;
        playerStateController.OnExecutionGaugeChanged += UpdateExecutionGauge;
        UpdateHp(playerStateController.Hp / playerStateController.MaxHp);
        UpdateExecutionGauge(playerStateController.ExecutionGauge / playerStateController.MaxExecutionGauge);
    }

    void OnDestroy()
    {
        if (playerStateController != null)
        {
            playerStateController.OnHpChanged -= UpdateHp;
            playerStateController.OnExecutionGaugeChanged -= UpdateExecutionGauge;
        }
    }

    private void UpdateHp(float ratio)
    {
        hp.transform.localScale = new Vector3(ratio, 1, 1);
    }

    private void UpdateExecutionGauge(float ratio)
    {
        executionGauge.transform.localScale = new Vector3(ratio, 1, 1);
    }
}
