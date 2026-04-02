using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(PlayerMovementController))]
public class PlayerShurikenController : PlayerSkillController
{
    [SerializeField] private GameObject shurikenPrefab;
    [SerializeField] private float cooldown = 3f;

    private PlayerMovementController controller;

    private float currentCooldown;

    private GameObject shuriken = null;
    private bool isShurikenThrown = false;

    private InputAction shurikenAction;

    public override SkillId SkillId => SkillId.ShurikenThrow;

    void Awake()
    {
        controller = GetComponent<PlayerMovementController>();
    }

    public override void InitializeSkill(InputActionMap playerMap)
    {
        shurikenAction = playerMap.FindAction("Shuriken", throwIfNotFound: true);
    }

    public override void Tick()
    {
        if (0 < currentCooldown) currentCooldown -= Time.deltaTime;

        if (!HasFlyingShuriken() && isShurikenThrown)
        {
            isShurikenThrown = false;
            currentCooldown = cooldown;
        }
    }

    public override void Handle()
    {
        if (shurikenAction.WasPressedThisFrame())
        {
            if (InCooldown()) return;
            if (shurikenPrefab == null)
            {
                Debug.LogWarning("PlayerShurikenController: shurikenPrefab is not assigned.");
                return;
            }

            if (HasFlyingShuriken()) Teleport();
            else ThrowShuriken();
        }
    }

    private bool InCooldown()
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
        currentCooldown = cooldown;
        controller.Teleport(teleportPos);
    }

    private void ThrowShuriken()
    {
        shuriken = Instantiate(shurikenPrefab);
        shuriken.transform.position = transform.position;
        shuriken.transform.forward = transform.forward;
        isShurikenThrown = true;
    }
}
