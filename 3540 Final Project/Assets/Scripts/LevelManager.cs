using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;


public class LevelManager : MonoBehaviour {

    [SerializeField] private float portalDistance = 3f;

    [Header("UI References")]
    [SerializeField] TMP_Text crystalCountText;

    [Header("Scene References")]
    [SerializeField] private int nextLevelSceneBuildIndex;
    [SerializeField] GameObject portal;

    [Header("Death Settings")]
    [SerializeField] private int sceneBuildIndexToReload = 0;
    public GameObject deadMessageUI;
    public float respawnDelay = 5f;

    private GameObject player;

    private void Awake() {
        player = GameObject.FindGameObjectWithTag("Player");

        if (deadMessageUI) deadMessageUI.SetActive(false);
    }

    private void Start() {
        UpdateCurrentCrystalText();
        portal.SetActive(false);
    }
    public void CheckCrystalCount() {
        UpdateCurrentCrystalText();
        if (CrystalBehavior.CurrentCrystalCount == 0) {
            OpenPortal();
        }
    }

    private void OpenPortal() {
        portal.SetActive(true);

        // Remove all enemies!
        EnemySpawner.KillAllEnemies();

        // Spawn the portal X meters forward in front of the player
        portal.transform.position = player.transform.position + portalDistance * player.transform.forward;
    }

    public void UpdateCurrentCrystalText() => crystalCountText.text = $"{CrystalBehavior.CurrentCrystalCount} crystals left!";

    public void Lose() {
        // Display death message
        if (deadMessageUI) deadMessageUI.SetActive(true);

        // Kill all enemies

        // Start reload scene coroutine
        StartCoroutine(ReloadAfterDelay());
    }

    private IEnumerator ReloadAfterDelay() {
        yield return new WaitForSeconds(respawnDelay);

        // Remove all enemies!
        EnemySpawner.KillAllEnemies();

        SceneManager.LoadScene(sceneBuildIndexToReload);
    }
}
