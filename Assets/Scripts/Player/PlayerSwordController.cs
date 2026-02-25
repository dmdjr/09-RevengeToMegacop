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
        if (throwSwordAction.WasPressedThisFrame())
        {
            if (InCooldown()) return;

            lastThrowTime = Time.time;

            if (!Instantiate(swordPrefab, transform.position, Quaternion.identity).TryGetComponent<SwordController>(out var sword)) return;

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
