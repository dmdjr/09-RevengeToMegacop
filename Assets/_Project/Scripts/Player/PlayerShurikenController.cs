using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(PlayerMovementController))]
public class PlayerShurikenController : MonoBehaviour
{
    [SerializeField] private GameObject shurikenPrefab;
    [SerializeField] private float coolTime = 3f;

    private PlayerMovementController controller;

    private float currentCooldown;

    private GameObject shuriken = null;
    private bool isShurikenThrown = false;

    private InputAction shurikenAction;

    void Awake()
    {
        controller = GetComponent<PlayerMovementController>();
    }

    public void Initialize(InputAction shurikenAction)
    {
        this.shurikenAction = shurikenAction;
    }

    public void UpdateCooldown()
    {
        if (0 < currentCooldown) currentCooldown -= Time.deltaTime;

        if (shuriken == null && isShurikenThrown)
        {
            isShurikenThrown = false;
            currentCooldown = coolTime;
        }
    }

    public void HandleShuriken()
    {
        if (shurikenAction.WasPressedThisFrame())
        {
            if (InCoolTime()) return;

            if (HasFlyingShuriken()) Teleport();
            else ThrowShuriken();
        }
    }

    private bool InCoolTime()
    {
        return 0 < currentCooldown;
    }

    private bool HasFlyingShuriken()
    {
        return shuriken != null;
    }

    private void Teleport()
    {
        if (shuriken == null) return;
        Vector3 teleportPos = shuriken.transform.position;
        Destroy(shuriken);
        shuriken = null;
        isShurikenThrown = false;
        currentCooldown = coolTime;
        controller.Teleport(teleportPos);
    }

    private void ThrowShuriken()
    {
        if (shurikenPrefab == null)
        {
            Debug.LogWarning("PlayerShurikenController: shurikenPrefab is not assigned.");
            return;
        }
        shuriken = Instantiate(shurikenPrefab);
        shuriken.transform.position = transform.position;
        shuriken.transform.forward = transform.forward;
        isShurikenThrown = true;
    }
}
