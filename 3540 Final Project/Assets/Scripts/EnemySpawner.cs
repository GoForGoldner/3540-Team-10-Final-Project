using UnityEngine;
using System.Collections;
using NUnit.Framework;
using System.Collections.Generic;

public class EnemySpawner : MonoBehaviour {
    [SerializeField] private float spawnRadius = 20f;
    [SerializeField] private int maxEnemyCount = 20;
    [SerializeField] private float enemySpawnTimer = 1f;
    [SerializeField] private int initialEnemyCount = 5;
    [SerializeField] private GameObject enemyPrefab;

    private static List<GameObject> enemies = new();

    private void Start() {
        if (enemyPrefab == null)
            Debug.LogError("[EnemySpawner] enemyPrefab is NULL in Inspector!");

        SpawnInitialEnemies();
        StartCoroutine(SpawnEnemies());
    }

    private void SpawnEnemy() {
        if (enemies.Count >= maxEnemyCount)
            return;

        bool enemySpawned = false;

        while (!enemySpawned) {
            float randomX = Random.Range(-spawnRadius, spawnRadius);
            float randomZ = Random.Range(-spawnRadius, spawnRadius);

            // Make sure the point is within the circle (not the square)
            while (new Vector2(randomX, randomZ).magnitude > spawnRadius) {
                randomX = Random.Range(-spawnRadius, spawnRadius);
                randomZ = Random.Range(-spawnRadius, spawnRadius);
            }

            Vector3 randomOffset = new Vector3(randomX, 0f, randomZ);

            // Raycast down to find the ground
            RaycastHit hit;
            // Start the raycast from high enough
            Vector3 raycastStart = randomOffset + Vector3.up * 50f;

            Debug.DrawRay(raycastStart, Vector3.down * 100f, Color.red, 1f);

            if (Physics.Raycast(raycastStart, Vector3.down, out hit, 300f)) {

                // Spawn the enemy at the hit point (on the ground)
                randomOffset.y = hit.point.y;

                GameObject newEnemy = Instantiate(enemyPrefab, randomOffset + Vector3.up * 5, Quaternion.identity);
                newEnemy.SetActive(true);
                newEnemy.name = $"Zombie {enemies.Count}";

                // Set parent to keep hierarchy clean
                newEnemy.transform.SetParent(transform);

                // Add to our list
                enemies.Add(newEnemy);

                enemySpawned = true;
            }
            else {
                Debug.Log($"No ground found at position {randomOffset}.");
            }
        }
    }

    private void SpawnInitialEnemies() {
        for (int i = 0; i < initialEnemyCount; i++) {
            SpawnEnemy();
        }
    }

    IEnumerator SpawnEnemies() {
        while (true) {
            yield return new WaitForSeconds(enemySpawnTimer);
            Debug.Log($"Spawn enemy called! {enemies.Count} ");
            SpawnEnemy();
        }
    }

    public static void KillAllEnemies() {
        while (enemies.Count > 0) {
            GameObject enemy = enemies[0];

            // Destroy the enemy GameObject
            Destroy(enemy);

            // Remove from the list
            enemies.RemoveAt(0);
        }
    }

    private void OnDrawGizmosSelected() {
        Gizmos.DrawWireSphere(transform.position, spawnRadius);
    }
}
