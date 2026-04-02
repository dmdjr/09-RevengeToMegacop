using System;
using UnityEngine;

namespace Boss3
{
    

    public class Boss3SmokeBomb : MonoBehaviour
{
    [Header("연막탄 설정")]
    [SerializeField] private float lifeTime = 5f;
    [SerializeField] private GameObject smokeEffectPrefab; // 나중에 연막 생성용

    private Rigidbody _rb;
    private bool _hasLanded = false;
    private Action _onExplode;


    private void Awake()
    {
        _rb = GetComponent<Rigidbody>();
    }

    private void OnEnable()
    {
        _hasLanded = false;

        if (_rb != null)
        {
            _rb.linearVelocity = Vector3.zero;
            _rb.angularVelocity = Vector3.zero;
        }

        CancelInvoke();
        Invoke(nameof(ForceExplode), lifeTime);
    }

    public void Throw(Vector3 startPosition, Vector3 throwVelocity, Action onExplode = null)
    {
        transform.position = startPosition;
        transform.rotation = Quaternion.identity;

        _onExplode = onExplode;
        _hasLanded = false;

        if (_rb != null)
        {
            _rb.linearVelocity = Vector3.zero;
            _rb.angularVelocity = Vector3.zero;
            _rb.AddForce(throwVelocity, ForceMode.VelocityChange);
        }
    }

    

        void OnTriggerEnter(Collider other)
        {
            if (_hasLanded) return;

        // 바닥이나 벽에 닿으면 착지로 처리
        _hasLanded = true;
        Explode();
        }

        private void ForceExplode()
    {
        if (_hasLanded) return;

        _hasLanded = true;
        Explode();
    }

    private void Explode()
    {
        Debug.Log("연막탄 착지 / 폭발");

        // 나중에 연막 생성
        if (smokeEffectPrefab != null)
        {
            GameObject smokeObj = Instantiate(smokeEffectPrefab, transform.position, Quaternion.identity);
            
        }

        _onExplode?.Invoke();
        Destroy(gameObject);
    }
}
}