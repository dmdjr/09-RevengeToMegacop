using UnityEngine;

public abstract class Weapon : MonoBehaviour
{
    [SerializeField] private float useDelay;
    private float previousTime;

    protected virtual void Awake()
    {
        previousTime = -useDelay;
    }

    public void TryUse()
    {
        float currentTime = Time.time;
        if (useDelay <= currentTime - previousTime)
        {
            previousTime = currentTime;
            Use();
        }
    }

    protected abstract void Use();
}