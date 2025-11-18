using UnityEngine;
using UnityEngine.AI; // Required for NavMeshAgent

[RequireComponent(typeof(NavMeshAgent))]
public class LightSensitiveEnemy : MonoBehaviour
{
    // Simple state machine for AI
    private enum State
    {
        Patrolling,
        Chasing,
        Fleeing
    }

    [Header("AI")]
    private State currentState;
    private NavMeshAgent agent;

    [Header("Targets")]
    public Transform playerTarget; // Assign the Player

    [Header("Stats")]
    public float chaseDistance = 10f;
    public float fleeDistance = 15f; // How far to flee before stopping

    [Header("Light Detection")]
    public float viewAngle = 30f; // Must match the flashlight's spot angle
    private FlashlightSystem playerFlashlight;
    private bool isLit = false;

    // Simple patrol behavior
    private Vector3 patrolPoint;
    private float patrolTimer;
    public float patrolRange = 10f;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();

        if (playerTarget == null)
        {
            Debug.LogError($"ENEMY ({name}): Player Target not assigned! The enemy will not function.");
        }
        else
        {
            // GetComponentInChildren also checks the parent object (playerTarget) itself.
            playerFlashlight = playerTarget.GetComponentInChildren<FlashlightSystem>();
            if (playerFlashlight == null)
            {
                Debug.LogError($"ENEMY ({name}): Could not find 'FlashlightSystem' script on the Player! The enemy will not react to light.");
            }
        }

        currentState = State.Patrolling;
        SetNewPatrolPoint();
    }

    void Update()
    {
        if (playerTarget == null) return;

        // 1. Check if we are being lit
        CheckIfLit();

        // 2. Run the state machine
        Debug.Log("Current state: " + currentState);
        switch (currentState)
        {
            case State.Patrolling:
                Patrol();
                // Check for transitions
                if (isLit)
                {
                    currentState = State.Fleeing;
                }
                else if (Vector3.Distance(transform.position, playerTarget.position) <= chaseDistance)
                {
                    currentState = State.Chasing;
                }
                break;

            case State.Chasing:
                Chase();
                // Check for transitions
                if (isLit)
                {
                    currentState = State.Fleeing;
                }
                else if (Vector3.Distance(transform.position, playerTarget.position) > chaseDistance)
                {
                    currentState = State.Patrolling;
                }
                break;

            case State.Fleeing:
                Flee();
                // Check for transitions
                if (!isLit && Vector3.Distance(transform.position, agent.destination) < 1.0f)
                {
                    currentState = State.Patrolling; // Cooldown / stop fleeing
                }
                break;
        }
    }

    void CheckIfLit()
    {
        // Add a safety check in case playerFlashlight was never found
        if (playerFlashlight == null || !playerFlashlight.isLightOn)
        {
            isLit = false;
            return;
        }

        Light light = playerFlashlight.flashlight;
        Vector3 dirToEnemy = (transform.position - light.transform.position).normalized;
        float angleToEnemy = Vector3.Angle(light.transform.forward, dirToEnemy);

        // 1. Check if enemy is within the flashlight's cone angle
        Debug.Log("Is withing cone?" + (angleToEnemy <= light.spotAngle / 2f));
        if (angleToEnemy <= light.spotAngle / 2f)
        {
            // 2. Raycast to see if the light is actually hitting the enemy (not blocked by a wall)
            float distanceToEnemy = Vector3.Distance(light.transform.position, transform.position);

            if (Physics.Raycast(light.transform.position, dirToEnemy, out RaycastHit hit, light.range))
            {
                // Check if the raycast hit this enemy
                if (hit.collider.gameObject == gameObject)
                {
                    isLit = true;
                    // Optional: Apply damage
                    // TakeDamage(1 * Time.deltaTime);
                    return;
                }
            }
        }

        isLit = false;
    }

    void Patrol()
    {
        agent.isStopped = false;
        // Check if we've reached the patrol point
        if (agent.remainingDistance < 0.5f)
        {
            patrolTimer += Time.deltaTime;
            // Wait for 3 seconds before finding new point
            if (patrolTimer > 3f)
            {
                SetNewPatrolPoint();
                patrolTimer = 0;
            }
        }
    }

    void SetNewPatrolPoint()
    {
        Vector3 randomPoint = transform.position + Random.insideUnitSphere * patrolRange;
        if (NavMesh.SamplePosition(randomPoint, out NavMeshHit hit, patrolRange, NavMesh.AllAreas))
        {
            patrolPoint = hit.position;
            agent.SetDestination(patrolPoint);
        }
    }

    void Chase()
    {
        agent.isStopped = false;
        agent.SetDestination(playerTarget.position);
    }

    void Flee()
    {
        agent.isStopped = false;
        // Calculate a point to run away from the player
        Vector3 runDirection = transform.position - playerTarget.position;
        Vector3 fleeTarget = transform.position + runDirection.normalized * fleeDistance;

        // Only set a new flee destination if we're not already fleeing
        if (agent.remainingDistance < 1.0f)
        {
            agent.SetDestination(fleeTarget);
        }
    }
}