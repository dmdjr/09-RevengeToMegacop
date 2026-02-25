using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(PlayerExecutionController))]
[RequireComponent(typeof(PlayerHitController))]
[RequireComponent(typeof(PlayerMovementController))]
[RequireComponent(typeof(PlayerShurikenController))]
[RequireComponent(typeof(PlayerStateController))]
[RequireComponent(typeof(PlayerSwordController))]
public class PlayerController : MonoBehaviour
{
    [SerializeField] private InputActionAsset inputActions;

    private PlayerExecutionController playerExecutionController;
    private PlayerHitController playerHitController;
    private PlayerMovementController playerMovementController;
    private PlayerShurikenController playerShurikenController;
    private PlayerStateController playerStateController;
    private PlayerSwordController playerSwordController;

    void Awake()
    {
        playerExecutionController = GetComponent<PlayerExecutionController>();
        playerHitController = GetComponent<PlayerHitController>();
        playerMovementController = GetComponent<PlayerMovementController>();
        playerShurikenController = GetComponent<PlayerShurikenController>();
        playerStateController = GetComponent<PlayerStateController>();
        playerSwordController = GetComponent<PlayerSwordController>();

        var playerMap = inputActions.FindActionMap("Player", throwIfNotFound: true);
        playerMovementController.Initialize(
            playerMap.FindAction("Move", throwIfNotFound: true),
            playerMap.FindAction("Sprint", throwIfNotFound: true));
        playerHitController.Initialize(
            playerMap.FindAction("Parry", throwIfNotFound: true));
        playerShurikenController.Initialize(
            playerMap.FindAction("Shuriken", throwIfNotFound: true));
        playerSwordController.Initialize(
            playerMap.FindAction("ThrowSword", throwIfNotFound: true));
        playerExecutionController.Initialize(
            playerMap.FindAction("Attack", throwIfNotFound: true));
    }

    void OnEnable()
    {
        inputActions.Enable();
    }

    void OnDisable()
    {
        inputActions.Disable();
    }

    void Update()
    {
        playerHitController.UpdateParries();
        playerMovementController.UpdateGravity();
        playerShurikenController.UpdateCooldown();
        playerStateController.UpdateStamina();

        playerExecutionController.HandleExecution();
        playerHitController.HandleHit();
        playerMovementController.HandleMovement();
        playerShurikenController.HandleShuriken();
        playerSwordController.HandleSword();
    }
}
