using System;
using System.Collections;

using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController))]
public class PlayerMovementController : MonoBehaviour
{
    [SerializeField] private float speed = 5f;
    [SerializeField] private float executionDashSpeed = 80f;
    [SerializeField] private float groundY = 1f;
    [SerializeField] private float executionArrivalThreshold = 0.1f;

    private CharacterController controller;

    private float realSpeed = 0f;

    private bool isMoving;
    private bool isExecutionDashing;
    private bool isDashing;

    public bool IsExecutionDashing => isExecutionDashing;
    public bool IsDashing => isDashing;

    private InputAction moveAction;
    private InputAction sprintAction;

    private float gravity = -9.81f;
    private Vector3 velocity;

    void Awake()
    {
        controller = GetComponent<CharacterController>();
        realSpeed = speed;
    }

    public void Initialize(InputAction moveAction, InputAction sprintAction)
    {
        this.moveAction = moveAction;
        this.sprintAction = sprintAction;
    }

    public void Teleport(Vector3 targetPosition)
    {
        if (controller == null) return;
        if (float.IsNaN(targetPosition.x) || float.IsNaN(targetPosition.y) || float.IsNaN(targetPosition.z))
        {
            Debug.LogWarning("PlayerMovementController: Teleport targetPosition is invalid.");
            return;
        }
        targetPosition.y = groundY;
        controller.enabled = false;
        transform.position = targetPosition;
        controller.enabled = true;
    }

    public void UpdateGravity()
    {
        if (isExecutionDashing) return;

        if (controller.isGrounded && velocity.y < 0)
        {
            velocity.y = -2f;
        }

        velocity.y += gravity * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);
    }

    public void HandleMovement()
    {
        HandleMove();
        HandleDash();
        HandleRotation();
    }

    private void HandleMove()
    {
        Vector2 input = moveAction.ReadValue<Vector2>();
        isMoving = input.sqrMagnitude > 0f;
        Vector3 dir = (Vector3.right * input.x + Vector3.forward * input.y).normalized;
        controller.Move(dir * (realSpeed * Time.deltaTime));
    }

    private void HandleDash()
    {
        bool canDash = SkillManager.Instance != null && SkillManager.Instance.IsUnlocked(SkillId.Dash);
        bool sprintPressed = sprintAction.IsPressed();
        isDashing = canDash && sprintPressed && isMoving;
        realSpeed = isDashing ? speed * 2 : speed;
    }

    private void HandleRotation()
    {
        transform.LookAt(MousePositionGetter.GetMousePositionInWorld(transform.position));
    }

    public void ExecutionDash(Vector3 target, Action onComplete)
    {
        StartCoroutine(ExecutionDashCoroutine(target, onComplete));
    }

    private IEnumerator ExecutionDashCoroutine(Vector3 target, Action onComplete)
    {
        isExecutionDashing = true;
        controller.enabled = false;
        target.y = groundY;

        try
        {
            while (Vector3.Distance(transform.position, target) > executionArrivalThreshold)
            {
                transform.position = Vector3.MoveTowards(transform.position, target, executionDashSpeed * Time.unscaledDeltaTime);
                yield return null;
            }

            transform.position = target;
        }
        finally
        {
            controller.enabled = true;
            isExecutionDashing = false;
            onComplete?.Invoke();
        }
    }
}