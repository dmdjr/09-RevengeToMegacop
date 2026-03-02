using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(PlayerMovementController))]
[RequireComponent(typeof(PlayerStateController))]
public class PlayerExecutionController : MonoBehaviour
{
    private PlayerMovementController playerMovementController;
    private PlayerStateController playerStateController;

    [SerializeField] private LayerMask enemyLayerMask;
    [SerializeField] private float executionRange = 50f;

    private InputAction attackAction;
    private Camera mainCamera;

    void Awake()
    {
        playerMovementController = GetComponent<PlayerMovementController>();
        playerStateController = GetComponent<PlayerStateController>();
        mainCamera = Camera.main;
    }

    public void Initialize(InputAction attackAction)
    {
        this.attackAction = attackAction;
    }

    public void HandleExecution()
    {
        if (attackAction.WasPressedThisFrame() && playerStateController.CanExecute())
        {
            TryExecute();
        }
    }

    private void TryExecute()
    {
        Ray ray = mainCamera.ScreenPointToRay(Mouse.current.position.ReadValue());
        if (Physics.Raycast(ray, out RaycastHit hit, executionRange, enemyLayerMask))
        {
            Execute(hit.collider.attachedRigidbody != null ? hit.collider.attachedRigidbody.gameObject : hit.collider.gameObject);
        }
    }

    private void Execute(GameObject enemy)
    {
        if (enemy == null) return;
        Vector3 enemyPosition = enemy.transform.position;
        if(enemy.TryGetComponent<Enemy>(out var e)) e.Die();
        playerMovementController.Teleport(enemyPosition);
        playerStateController.Executed();
    }
}
