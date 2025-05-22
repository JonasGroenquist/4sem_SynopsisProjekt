using UnityEngine;
using UnityEngine.AI;

public class EnemyChase : MonoBehaviour
{
    [Header("Chase Settings")]
    [SerializeField] private float detectionRange = 10f;
    [SerializeField] private float chaseSpeed = 3.5f;

    [Header("Target")]
    [SerializeField] private Transform player;

    // Components
    private NavMeshAgent navAgent;
    private Animator animator;

    // State
    private bool isChasing = false;

    void Start()
    {
        // Get components
        navAgent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();

        if (navAgent == null)
        {
            Debug.LogError("NavMeshAgent component missing on " + gameObject.name);
            return;
        }

        // Set agent speed
        navAgent.speed = chaseSpeed;

        // Find player if not assigned
        if (player == null)
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null)
            {
                player = playerObj.transform;
            }
            else
            {
                Debug.LogError("Player not found! Make sure player has 'Player' tag or assign manually.");
            }
        }
    }

    void Update()
    {
        if (player == null || navAgent == null) return;

        // Calculate distance to player
        float distanceToPlayer = Vector3.Distance(transform.position, player.position);

        // Check if player is in detection range
        if (distanceToPlayer <= detectionRange)
        {
            // Start chasing
            if (!isChasing)
            {
                isChasing = true;
                Debug.Log(gameObject.name + " started chasing player!");
            }

            // Move toward player
            navAgent.isStopped = false;
            navAgent.SetDestination(player.position);

            // Animation is controlled by the Animator Controller automatically
            // No need to set parameters since it's working!
        }
        else
        {
            // Player out of range - stop chasing
            if (isChasing)
            {
                isChasing = false;
                navAgent.isStopped = true;
                Debug.Log(gameObject.name + " stopped chasing player.");

                // Animation stops automatically when movement stops
            }
        }
    }

    // Visualize detection range in Scene view
    void OnDrawGizmosSelected()
    {
        // Detection range (yellow)
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRange);
    }
}