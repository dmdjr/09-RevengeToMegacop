using UnityEngine;

public abstract class GunWeapon : Weapon
{
    [field: SerializeField] public int MaxAmmo { get; private set; }

    [field: SerializeField] public int Ammo { get; private set; }

    [SerializeField] protected GameObject bulletPrefab;

    [SerializeField] protected Transform firePoint;

    protected override void Awake()
    {
        base.Awake();
        if (MaxAmmo < Ammo) Ammo = MaxAmmo;
    }

    protected override void Use()
    {
        if (CanFire()) Fire();
    }
    
    protected bool CanFire()
    {
        return 0 < Ammo;
    }

    private void Fire()
    {
        Instantiate(bulletPrefab, firePoint.position, firePoint.rotation);
    }
}