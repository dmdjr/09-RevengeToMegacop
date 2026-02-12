using UnityEngine;

public class Enemy : MonoBehaviour
{
    [SerializeField] private Weapon weapon;
    [SerializeField] private Transform target;

    void Update()
    {
        LookTarget();
        weapon?.TryUse();
    }

    private void LookTarget()
    {
        if (target is null) return;

        transform.LookAt(target);
        if (weapon is GunWeapon)
        {
            weapon.transform.LookAt(target);
        }
    }
}