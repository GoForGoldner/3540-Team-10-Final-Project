using UnityEngine;

[RequireComponent(typeof(Collider))]
public class CrystalBehavior : MonoBehaviour
{
    public static int TotalCrystalCount { get; private set; }
    public static int CurrentCrystalCount { get; private set; }

    private LevelManager levelManager;

    private void Awake() {
        TotalCrystalCount = 0;
        CurrentCrystalCount = 0;
        levelManager = GameObject.FindGameObjectWithTag("Level Manager").GetComponent<LevelManager>();
    }

    private void Start() {
        TotalCrystalCount++;
        CurrentCrystalCount++;
        levelManager.UpdateCurrentCrystalText();
    }

    private void OnTriggerEnter(Collider other) {
        // If the player collided with a crystal
        if (other.CompareTag("Player")) {
            CurrentCrystalCount--;
            if (levelManager) levelManager.CheckCrystalCount();
            Destroy(gameObject);
        }
    }
}
