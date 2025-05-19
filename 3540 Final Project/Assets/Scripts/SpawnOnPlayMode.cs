using UnityEngine;
using UnityEditor;
using UnityEngine.SceneManagement;

[ExecuteInEditMode]
public class SpawnOnPlayMode : MonoBehaviour {
    [SerializeField] private GameObject prefabToSpawn;

    [SerializeField] private string prefabTag;
    [SerializeField] private bool spawnOnSceneLoad = true;

#if UNITY_EDITOR
    private void OnEnable() {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable() {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void SpawnPrefab() {
        if (prefabToSpawn == null) return;

        GameObject instance = Instantiate(prefabToSpawn, Vector3.zero, Quaternion.identity);

        Debug.Log($"Spawned {prefabToSpawn.name}");
    }

    // Called when play mode state changes
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode) {
        if (spawnOnSceneLoad && !PrefabInstanceExists()) {
            SpawnPrefab();
        }
    }

    private bool PrefabInstanceExists() {
        if (!string.IsNullOrEmpty(prefabTag)) {
            GameObject[] taggedObjects = GameObject.FindGameObjectsWithTag(prefabTag);
            if (taggedObjects.Length > 0) {
                return true;
            }
        }

        return false;
    }

#endif
}