using UnityEngine;

/// <summary>
/// PlayerHitController.OnDamaged 이벤트를 구독하여 DamageFlash.Flash()를 호출하는 컴포넌트.
/// </summary>
public class DamageFlashListener : MonoBehaviour
{
    [SerializeField] private DamageFlash damageFlash;
    [SerializeField] private PlayerHitController playerHitController;

    [Header("피격")]
    [SerializeField] private float flashIntensity = 0.8f;
    [SerializeField] private float flashDuration = 0.3f;

    void Start()
    {
        if (playerHitController != null)
        {
            playerHitController.OnDamaged += OnDamaged;
        }
    }

    void OnDestroy()
    {
        if (playerHitController != null)
        {
            playerHitController.OnDamaged -= OnDamaged;
        }
    }

    private void OnDamaged()
    {
        if (damageFlash != null)
            damageFlash.Flash(flashIntensity, flashDuration);
    }
}
