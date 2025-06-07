using UnityEngine;

public class KillPlaneBehavior : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Enemy"))
        {
            var enemy = other.GetComponent<EnemyBehavior>();
            if (enemy != null)
                enemy.TakeDamage(0 /*enemy.maxHealth*/);
            else
                Destroy(other.gameObject);
        }
        else if (other.CompareTag("Player"))
        {
            var playerHealth = other.GetComponent<Health>();
            if (playerHealth != null)
                playerHealth.TakeDamage(playerHealth.startingHealth);
            else
                Destroy(other.gameObject);
        }
    }
}
