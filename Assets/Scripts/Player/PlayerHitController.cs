using UnityEngine;

public class PlayerHitController : MonoBehaviour, IDamageable
{
    private ParryController parryController = new ParryController();

    // Update is called once per frame
    void Update()
    {
        parryController.RemoveTooEarlyParries();
        InputParry();
    }

    private void InputParry()
    {
        if (Input.GetKeyDown(KeyCode.Q))
        {
            parryController.StackParry();
        }
    }

    public void Hit(Bullet bullet)
    {
        if (parryController.CanParry())
        {
            Parry(bullet);
            return;
        }

        TakeDamage();
    }

    private void Parry(Bullet bullet)
    {
        bullet.Reflect();
        parryController.Parry();
    }

    private void TakeDamage()
    {
    }
}
