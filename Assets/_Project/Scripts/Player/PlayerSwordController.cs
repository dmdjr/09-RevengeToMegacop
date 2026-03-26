using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerSwordController : MonoBehaviour
{
    [SerializeField] private GameObject swordPrefab;
    [SerializeField] private float throwCooldown = 3f;

    private float lastThrowTime;

    private InputAction throwSwordAction;

    public void Initialize(InputAction throwSwordAction)
    {
        this.throwSwordAction = throwSwordAction;
    }

    public void HandleSword()
    {
        if (!SkillManager.Instance.IsUnlocked(SkillId.SwordThrow)) return;
        if (throwSwordAction.WasPressedThisFrame())
        {
            if (InCooldown()) return;
            if (swordPrefab == null)
            {
                Debug.LogWarning("PlayerSwordController: swordPrefab is not assigned.");
                return;
            }

            lastThrowTime = Time.time;

            GameObject swordObj = Instantiate(swordPrefab, transform.position, Quaternion.identity);
            if (!swordObj.TryGetComponent<SwordController>(out var sword))
            {
                Destroy(swordObj);
                return;
            }

            Vector3 mousePos = MousePositionGetter.GetMousePositionInWorld(sword.transform.position);
            sword.Throw(mousePos);
        }
    }

    private bool InCooldown()
    {
        return Time.time - lastThrowTime < throwCooldown;
    }

    void Start()
    {
        lastThrowTime = -throwCooldown;
    }
}
