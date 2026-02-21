using UnityEngine;

[RequireComponent(typeof(PlayerMovementController))]
[RequireComponent(typeof(PlayerStateController))]
public class PlayerExecutionController : MonoBehaviour
{
    private PlayerMovementController playerMovementController;
    private PlayerStateController playerStateController;

    [SerializeField] private LayerMask enemyLayerMask;

    void Awake()
    {
        playerMovementController = GetComponent<PlayerMovementController>();
        playerStateController = GetComponent<PlayerStateController>();
    }

    public void HandleExecution()
    {
        if (Input.GetMouseButtonDown(0) && playerStateController.CanExecute())
        {
            TryExecute();
        }
    }

    private void TryExecute()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, enemyLayerMask))
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
