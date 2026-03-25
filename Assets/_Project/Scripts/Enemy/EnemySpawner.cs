using System.Collections.Generic;

using UnityEngine;
using UnityEngine.AI;

public class EnemySpawner : MonoBehaviour
{
    [SerializeField] private GameObject enemyPrefab;
    [SerializeField] private List<GameObject> weaponPrefabs = new List<GameObject>();
    [SerializeField] private Transform target;

    [SerializeField] private float spawnInterval = 10f;
    [SerializeField] private float spawnArea = 100f;
    [SerializeField] private int maxEnemies = 10;

    private float timer = 0f;
    private HashSet<GameObject> spawnedEnemies = new HashSet<GameObject>();

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
        Enemy enemy = enemyObj.GetComponent<Enemy>();
        if (enemy == null)
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
                Weapon weaponComponent = weaponInstance.GetComponent<Weapon>();
                if (weaponComponent != null)
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
        if (target != null)
        {
            enemy.SetTarget(target);
        }

        spawnedEnemies.Add(enemyObj);
    }

    private Vector3 GenerateSpawnPosition()
    {
        Vector3 pos;
        float x = Random.Range(-spawnArea, spawnArea);
        float z = Random.Range(-spawnArea, spawnArea);
        pos = new Vector3(transform.position.x + x, 0f, transform.position.z + z);
        if (target == null) return pos;

        float dist = Vector3.Distance(pos, target.position);
        if (dist < 10f)
        {
            Vector3 dir = (pos - target.position).normalized;
            pos = target.position + dir * 10.1f;
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
        foreach (GameObject enemyObj in spawnedEnemies)
        {
            if (enemyObj == null) continue;
            Enemy enemy = enemyObj.GetComponent<Enemy>();
            if (enemy != null) enemy.OnDeath -= OnDeath;
        }
    }

    private void OnDeath(GameObject enemyObj)
    {
        spawnedEnemies.Remove(enemyObj);
    }
}
