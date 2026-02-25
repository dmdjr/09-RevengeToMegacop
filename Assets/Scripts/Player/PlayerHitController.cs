using System;

using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(PlayerStateController))]
public class PlayerHitController : MonoBehaviour, IDamageable
{
    [Range(-1f, 1f)]
    [SerializeField]
    private float parryThreshold = 0.5f;

    [SerializeField]
    private float parryDuration = 0.5f;

    private PlayerStateController playerStateController;

    private ParryController parryController = new ParryController();

    private bool isGuarding = false;

    private InputAction parryAction;

    void Awake()
    {
        playerStateController = GetComponent<PlayerStateController>();
    }

    public void Initialize(InputAction parryAction)
    {
        this.parryAction = parryAction;
    }

    public void UpdateParries()
    {
        parryController.RemoveTooEarlyParries(parryDuration);
    }

    public void HandleHit()
    {
        if (parryAction.WasPressedThisFrame())
        {
            parryController.StackParry();
            isGuarding = true;
        }

        if (parryAction.WasReleasedThisFrame())
        {
            isGuarding = false;
        }
    }

    public void Hit(Bullet bullet)
    {
        if (bullet == null) return;
        if (CanParry(bullet))
        {
            Parry(bullet);
            return;
        }

        if (CanGuard(bullet))
        {
            Guard(bullet);
            return;
        }

        TakeDamage(bullet);
    }

    private bool CanParry(Bullet bullet)
    {
        return IsBulletInFront(bullet) && parryController.CanParry() && playerStateController.CanParry();
    }

    private bool IsBulletInFront(Bullet bullet)
    {
        Vector3 directionToBullet = bullet.transform.forward.normalized * -1f;

        directionToBullet.y = 0f;
        Vector3 playerForward = transform.forward.normalized;
        playerForward.y = 0f;

        float dot = Vector3.Dot(directionToBullet, playerForward);

        return parryThreshold < dot;
    }

    private void Parry(Bullet bullet)
    {
        parryController.Parry();
        bullet.Reflect(gameObject, true);
        playerStateController.OnSuccessfulParry();
    }

    private bool CanGuard(Bullet bullet)
    {
        return IsBulletInFront(bullet) && isGuarding && playerStateController.CanGuard();
    }

    private void Guard(Bullet bullet)
    {
        bullet.Reflect(gameObject, false);
        playerStateController.OnSuccessfulGuard();
    }

    private void TakeDamage(Bullet bullet)
    {
        playerStateController.TakeDamage(bullet.Damage);
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;

        Gizmos.DrawRay(transform.position, transform.forward * 2f);

        float angle = Mathf.Acos(parryThreshold) * Mathf.Rad2Deg;

        Vector3 leftDirection = Quaternion.Euler(0, -angle, 0) * transform.forward;
        Gizmos.DrawRay(transform.position, leftDirection * 2f);

        Vector3 rightDirection = Quaternion.Euler(0, angle, 0) * transform.forward;
        Gizmos.DrawRay(transform.position, rightDirection * 2f);
    }
}
