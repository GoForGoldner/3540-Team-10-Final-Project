using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class SceneSwitcher : MonoBehaviour {
    [SerializeField] private int scene1BuildIndex, scene2BuildIndex;

    private bool isScene1Active = true;
    private Scene scene1, scene2;
    private bool isSwitching = false;
    private Material scene1Skybox, scene2Skybox;

    private void Awake() {
       if (scene1BuildIndex == scene2BuildIndex) {
            Debug.LogWarning("Disabled scene switcher because the scene's build index for a and b are the same. Switch one of them.");
            gameObject.SetActive(false);
       }
    }

    private void Start() {
        // Load both scenes
        StartCoroutine(LoadBothScenes());
    }

    private void Update() {
        if (Input.GetKeyDown("q") && !isSwitching && scene1.isLoaded && scene2.isLoaded) {
            SwitchActiveScene();
        }
    }

    void SwitchActiveScene() {
        isSwitching = true;

        if (isScene1Active) {
            // Switch from scene1 to scene2
            SetSceneActive(scene1, false);
            SetSceneActive(scene2, true);
            RenderSettings.skybox = scene2Skybox;
            isScene1Active = false;
        }
        else {
            // Switch from scene2 to scene1
            SetSceneActive(scene2, false);
            SetSceneActive(scene1, true);
            RenderSettings.skybox = scene1Skybox;
            isScene1Active = true;
        }

        // Update the lighting (and skybox)
        DynamicGI.UpdateEnvironment();

        Debug.Log("Switched to " + (isScene1Active ? scene1.name : scene2.name));
        isSwitching = false;
    }

    /* Loads both scenes into memory and gets
     * a memory reference to both scenes */
    IEnumerator LoadBothScenes() {
        // Load scene1 (shouldn't have to load anything because it's the active scene)
        scene1 = SceneManager.GetSceneByBuildIndex(scene1BuildIndex);
        // Get scene1's skybox
        scene1Skybox = RenderSettings.skybox;

        // Store the original active scene to return to it
        Scene originalActiveScene = SceneManager.GetActiveScene();

        // Additivly load scene 2
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(scene2BuildIndex, LoadSceneMode.Additive);
        // Wait for scene 2 to load
        yield return asyncLoad;

        // Get a reference for scene 2 now that it's loaded properly
        scene2 = SceneManager.GetSceneByBuildIndex(scene2BuildIndex);

        // Quickly load and unload scene 2 to get it's skybox
        if (SceneManager.SetActiveScene(scene2)) {
            scene2Skybox = RenderSettings.skybox;
            SceneManager.SetActiveScene(originalActiveScene);
        }

        // Make all objects in scene2 inactive
        SetSceneActive(scene2, false);

        Debug.Log("Scene switcher is ready for switching!");
    }


    /* Enables or deactivates all objects in a scene 
     (could potentially need to be improved later on) */
    private void SetSceneActive(Scene scene, bool active) {
        // Get all root GameObjects in the scene
        GameObject[] rootObjects = scene.GetRootGameObjects();

        // Activate/deactivate each root GameObject
        foreach (GameObject obj in rootObjects) {
            // Only enable + disable objects with no tag on them
            // Could be iterated on later
            if (obj.tag == "Untagged") obj.SetActive(active);
        }
    }
}