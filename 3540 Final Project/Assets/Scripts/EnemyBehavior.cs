using System.Collections;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(Animator), typeof(NavMeshAgent))]
public class EnemyBehavior : MonoBehaviour
{
    [System.Serializable]
    private enum EnemyState {
        Idle, Walking, Attacking, Dying
    }

    [SerializeField] private Transform player;
    [SerializeField] private int maxHealth = 100;

    [Header("Walking Settings")]
    [SerializeField] private float moveSpeed = 3.5f;
    [SerializeField] private float turnSpeed = 5f;

    [Header("Attack Settings")]
    [SerializeField] private int attackDamage = 20;
    [SerializeField] private float attackInterval = 1.5f;
    [SerializeField] private float attackRadius = 2f;

    [Header("Detection Settings")]
    [SerializeField] private float chaseRadius = 10f;

    private EnemyState enemyState = EnemyState.Idle;

    private int currentHealth;
    
    private static readonly int stateHash = Animator.StringToHash("EnemyState");
    private float nextAttackTime = 0f;

    private Animator animator;
    private NavMeshAgent navMeshAgent;
    private Health playerHealth;

    private void Start()
    {
        DontDestroyOnLoad(gameObject);
        currentHealth = maxHealth;

        animator = GetComponent<Animator>();
        navMeshAgent = GetComponent<NavMeshAgent>();
        
        if (!player) player = GameObject.FindGameObjectWithTag("Player")?.transform;
        playerHealth = player.GetComponent<Health>();
    }

    private void Update()
    {
        if (player == null) return;

        UpdateEnemyState();

        switch (enemyState) {
            case EnemyState.Idle:
                // Do nothing intentionally
                break;

            case EnemyState.Walking:
                navMeshAgent.SetDestination(player.position);
                break;

            case EnemyState.Attacking:            
                if (Time.time >= nextAttackTime) {
                    if (playerHealth != null && Health.IsAlive)
                        playerHealth.TakeDamage(attackDamage);
                    nextAttackTime = Time.time + attackInterval;
                }
                break;

            case EnemyState.Dying:
                break;

            default:
                break;
        }
    }

    // Updates the state of the enemy based on the distance they are from the player
    private void UpdateEnemyState() {
        // Don't update state during death animation!
        if (enemyState == EnemyState.Dying) return;

        float distance = Vector3.Distance(transform.position, player.position);
        EnemyState newEnemyState;

        if (distance <= attackRadius) {
            newEnemyState = EnemyState.Attacking;
        }
        else if (distance <= chaseRadius) {
            newEnemyState = EnemyState.Walking;
        }
        else {
            newEnemyState = EnemyState.Idle;
        }

        if (newEnemyState != enemyState) {
            enemyState = newEnemyState;
            animator.SetInteger(stateHash, (int) enemyState);
        }
    }

    public void TakeDamage(int damage)
    {
        Debug.Log($"[EnemyBehavior] {name} is got shot for at {damage} damage");

        currentHealth -= damage;
        if (currentHealth <= 0 && enemyState != EnemyState.Dying) Die();
    }

    private void Die()
    {
        Debug.Log($"[EnemyBehavior] {name} is dying at {transform.position}");
        
        enemyState = EnemyState.Dying;

        // Stop the agent from moving
        navMeshAgent.isStopped = true;

        // Play the death animation
        animator.SetInteger(stateHash, (int) enemyState);
        
        StartCoroutine(DestroyAfterDelay());
    }

    private void OnDestroy() {
        Debug.Log($"[EnemyBehavior] OnDestroy() called for {name}");
        Debug.Log($"[EnemyBehavior] STACK TRACE: {System.Environment.StackTrace}");
    }

    private IEnumerator DestroyAfterDelay()
    {
        yield return new WaitForSeconds(3f);
        Destroy(gameObject);
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRadius);
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, chaseRadius);
    }
}
