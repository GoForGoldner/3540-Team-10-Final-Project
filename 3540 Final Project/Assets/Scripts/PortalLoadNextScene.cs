using UnityEngine;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(Collider))]
public class PortalLoadNextScene : MonoBehaviour
{
    [SerializeField] private int sceneToLoadByBuildIndex;

    private void OnTriggerEnter(Collider other) {
        if (other.CompareTag("Player")) {
            loadScene();
        }
    }

    private void loadScene() => SceneManager.LoadScene(sceneToLoadByBuildIndex);
}
