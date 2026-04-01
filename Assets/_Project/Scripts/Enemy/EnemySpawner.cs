using System.Collections.Generic;

using UnityEngine;
using UnityEngine.AI;

public class EnemySpawner : MonoBehaviour
{
    [SerializeField] private GameObject enemyPrefab;
    [SerializeField] private List<GameObject> weaponPrefabs = new List<GameObject>();
    [SerializeField] private Transform target;
    [SerializeField] private CameraShakeListener cameraShakeListener;

    [SerializeField] private float spawnInterval = 10f;
    [SerializeField] private float spawnArea = 100f;
    [SerializeField] private int maxEnemies = 10;
    [SerializeField] private float minSpawnDistance = 10f;

    private float timer = 0f;
    private HashSet<Enemy> spawnedEnemies = new HashSet<Enemy>();

    void Update()
    {
        timer += Time.deltaTime;
        if (timer < spawnInterval) return;
        timer = 0f;

        if (spawnedEnemies.Count >= maxEnemies) return;

        SpawnEnemyWithWeapon();
    }

    private void SpawnEnemyWithWeapon()
    {
        if (enemyPrefab == null) return;
        Vector3 pos = GenerateSpawnPosition();
        Quaternion rot = Quaternion.identity;
        GameObject enemyObj = Instantiate(enemyPrefab, pos, rot, transform);
        if (!enemyObj.TryGetComponent<Enemy>(out Enemy enemy))
        {
            Debug.LogWarning("EnemySpawner: Spawned prefab is missing Enemy component. Destroying.");
            Destroy(enemyObj);
            return;
        }

        if (weaponPrefabs != null && weaponPrefabs.Count > 0)
        {
            int index = Random.Range(0, weaponPrefabs.Count);
            GameObject weaponPrefab = weaponPrefabs[index];
            if (weaponPrefab != null)
            {
                GameObject weaponInstance = Instantiate(weaponPrefab);
                if (weaponInstance.TryGetComponent<Weapon>(out Weapon weaponComponent))
                {
                    enemy.EquipWeapon(weaponComponent);
                }
                else
                {
                    Destroy(weaponInstance);
                }
            }
        }

        enemy.OnDeath += OnDeath;
        cameraShakeListener?.RegisterEnemy(enemy);
        if (target != null)
        {
            enemy.SetTarget(target);
        }

        spawnedEnemies.Add(enemy);
    }

    private Vector3 GenerateSpawnPosition()
    {
        Vector3 pos;
        float x = Random.Range(-spawnArea, spawnArea);
        float z = Random.Range(-spawnArea, spawnArea);
        pos = new Vector3(transform.position.x + x, 0f, transform.position.z + z);
        if (target == null) return pos;

        float dist = Vector3.Distance(pos, target.position);
        if (dist < minSpawnDistance)
        {
            Vector3 dir = (pos - target.position).normalized;
            pos = target.position + dir * (minSpawnDistance + 0.1f);
            pos.y = 0f;
        }

        if (NavMesh.SamplePosition(pos, out NavMeshHit hit, 10f, NavMesh.AllAreas))
        {
            pos = hit.position;
        }

        return pos;
    }

    void OnDestroy()
    {
        foreach (Enemy enemy in spawnedEnemies)
        {
            if (enemy != null) enemy.OnDeath -= OnDeath;
        }
    }

    private void OnDeath(Enemy enemy)
    {
        spawnedEnemies.Remove(enemy);
    }
}
