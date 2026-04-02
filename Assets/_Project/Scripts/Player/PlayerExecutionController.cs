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
    [SerializeField] private ExecutionSliceEffect executionSliceEffect;
    [SerializeField] private ExecutionSlashVfx executionSlashVfx;

    private InputAction attackAction;
    private Camera mainCamera;

    private Enemy executionTarget;

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
        if (Mouse.current == null) return;
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

        executionTarget = enemy.TryGetComponent<Enemy>(out var enemyComponent) ? enemyComponent : null;

        playerMovementController.ExecutionDash(enemyPosition, OnExecutionDashComplete);
        playerStateController.Executed();
    }

    private void OnExecutionDashComplete()
    {
        if (executionTarget != null)
        {
            // 슬라이스 방향: 플레이어 진행 방향의 수직 (좌우 절단)
            Vector3 sliceNormal = transform.right;
            Vector3 slicePosition = executionTarget.transform.position;

            if (executionSlashVfx != null)
            {
                executionSlashVfx.Play(slicePosition, transform.forward);
            }

            if (executionSliceEffect != null)
            {
                executionSliceEffect.Slice(executionTarget.gameObject, slicePosition, sliceNormal);
            }

            executionTarget.Die();
            executionTarget = null;
        }
    }
}
