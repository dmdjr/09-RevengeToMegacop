using UnityEngine;

public class UIController : MonoBehaviour
{
    [SerializeField] private GameObject hp;
    [SerializeField] private GameObject executionGauge;
    [SerializeField] private GameObject stamina;
    [SerializeField] private PlayerStateController playerStateController;

    void Start()
    {
        if (hp == null || executionGauge == null || stamina == null || playerStateController == null)
        {
            Debug.LogError("One or more required components are not assigned in UIController.");
            return;
        }
        playerStateController.OnHpChanged += UpdateHp;
        playerStateController.OnExecutionGaugeChanged += UpdateExecutionGauge;
        playerStateController.OnStaminaChanged += UpdateStamina;
        UpdateHp(playerStateController.MaxHp > 0f ? playerStateController.Hp / playerStateController.MaxHp : 0f);
        UpdateExecutionGauge(playerStateController.MaxExecutionGauge > 0f ? playerStateController.ExecutionGauge / playerStateController.MaxExecutionGauge : 0f);
        UpdateStamina(playerStateController.MaxStamina > 0f ? playerStateController.Stamina / playerStateController.MaxStamina : 0f);
    }

    void OnDestroy()
    {
        if (playerStateController != null)
        {
            playerStateController.OnHpChanged -= UpdateHp;
            playerStateController.OnExecutionGaugeChanged -= UpdateExecutionGauge;
            playerStateController.OnStaminaChanged -= UpdateStamina;
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

    private void UpdateStamina(float ratio)
    {
        stamina.transform.localScale = new Vector3(ratio, 1, 1);
    }
}
