using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using System.Collections;

public class Health : MonoBehaviour
{
    public static bool IsAlive { get; private set; }

    [Header("Health Settings")]
    public int startingHealth = 100;
    private int currentHealth;

    [Header("UI Settings")]
    public TextMeshProUGUI healthText;
    public Color fullHealthColor = Color.green;
    public Color lowHealthColor = Color.red;

    private LevelManager levelManager;
    
    
    void Awake()
    {
        levelManager = GameObject.FindGameObjectWithTag("Level Manager").GetComponent<LevelManager>();

        currentHealth = startingHealth;
        IsAlive = true;

        UpdateHealthDisplay();
    }

    public void TakeDamage(int damage)
    {
        if (!IsAlive) return;

        currentHealth -= damage;
        currentHealth = Mathf.Clamp(currentHealth, 0, startingHealth);
        UpdateHealthDisplay();

        if (currentHealth <= 0) levelManager.Lose();
    }

    public void Heal(int amount)
    {
        if (!IsAlive) return;

        currentHealth += amount;
        currentHealth = Mathf.Clamp(currentHealth, 0, startingHealth);
        UpdateHealthDisplay();
    }

    private void UpdateHealthDisplay()
    {
        if (healthText == null) return;

        healthText.text = $"HP: {currentHealth}";

        float t = currentHealth / (float)startingHealth;
        t = Mathf.Clamp01(t);
        healthText.color = Color.Lerp(lowHealthColor, fullHealthColor, t);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("KillPlane")) levelManager.Lose();
    }
}
