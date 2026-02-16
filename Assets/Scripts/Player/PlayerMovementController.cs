using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [SerializeField] private Camera mainCamera;
    [SerializeField] private CharacterController controller;
    [SerializeField] private float speed;

    private float realSpeed = 0f;

    private float gravity = -9.81f;
    private Vector3 velocity;

    void Awake()
    {
        realSpeed = speed;
    }

    // Update is called once per frame
    void Update()
    {
        InputDash();
        InputMove();
        InputRotation();
        Gravity();
    }

    void InputDash()
    {
        if (Input.GetKey(KeyCode.LeftShift))
        {
            realSpeed = speed * 2;
        }
        else
        {
            realSpeed = speed;
        }
    }

    void InputMove()
    {
        var h = Input.GetAxisRaw("Horizontal");
        var v = Input.GetAxisRaw("Vertical");
        Vector3 dir = (Vector3.right * h + Vector3.forward * v).normalized;
        controller.Move(dir * (realSpeed * Time.deltaTime));
    }

    void InputRotation()
    {
        transform.LookAt(MousePositionGetter.GetMousePositionInWorld(transform.position));
    }

    void Gravity()
    {
        if (controller.isGrounded && velocity.y < 0)
        {
            velocity.y = -2f;
        }

        velocity.y += gravity * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);
    }
}