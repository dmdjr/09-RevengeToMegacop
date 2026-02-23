using UnityEngine;

[RequireComponent(typeof(PlayerExecutionController))]
[RequireComponent(typeof(PlayerHitController))]
[RequireComponent(typeof(PlayerMovementController))]
[RequireComponent(typeof(PlayerShurikenController))]
[RequireComponent(typeof(PlayerStateController))]
[RequireComponent(typeof(PlayerSwordController))]
public class PlayerController : MonoBehaviour
{
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
