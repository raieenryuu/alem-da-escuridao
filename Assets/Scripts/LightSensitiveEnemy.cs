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
    public float patrolSpeed = 1.5f; // --- NEW ---
    public float chaseSpeed = 3.5f; // --- NEW ---
    public float fleeSpeed = 5.0f; // --- NEW ---
    public float chaseDistance = 10f;
    public float fleeDistance = 15f; // How far to flee before stopping
    public float patrolRange = 10f;

    [Header("Light Detection")]
    public float viewAngle = 30f; // Must match the flashlight's spot angle
    private FlashlightSystem playerFlashlight;
    private bool isLit = false;

    // --- NEW ---
    private Animator animator; // Reference to the Animator component
    // --- END NEW ---

    // Simple patrol behavior
    private Vector3 patrolPoint;
    private float patrolTimer;


    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        
        // --- NEW ---
        // Get the Animator from the child object. This is more robust.
        animator = GetComponentInChildren<Animator>(); 
        if (animator == null)
        {
            Debug.LogError($"ENEMY ({name}): Could not find Animator component on self or children.");
        }
        // --- END NEW ---

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
        agent.speed = patrolSpeed; // --- NEW ---
        SetNewPatrolPoint();
    }

    void Update()
    {
        if (playerTarget == null) return;

        // --- NEW: Animation Update ---
        // This is the magic line. It sends the agent's current speed to the Animator.
        // .magnitude turns the (x,y,z) velocity into a single speed number.
        if (animator != null)
        {
            animator.SetFloat("Speed", agent.velocity.magnitude);
        }
        // --- END NEW ---

        // 1. Check if we are being lit
        CheckIfLit(); // <-- This is YOUR correct function

        // 2. Run the state machine
        switch (currentState)
        {
            case State.Patrolling:
                Patrol();
                // Check for transitions
                if (isLit)
                {
                    currentState = State.Fleeing;
                    agent.speed = fleeSpeed; // --- NEW ---
                }
                else if (Vector3.Distance(transform.position, playerTarget.position) <= chaseDistance)
                {
                    currentState = State.Chasing;
                    agent.speed = chaseSpeed; // --- NEW ---
                }
                break;

            case State.Chasing:
                Chase();
                // Check for transitions
                if (isLit)
                {
                    currentState = State.Fleeing;
                    agent.speed = fleeSpeed; // --- NEW ---
                }
                else if (Vector3.Distance(transform.position, playerTarget.position) > chaseDistance)
                {
                    currentState = State.Patrolling;
                    agent.speed = patrolSpeed; // --- NEW ---
                }
                break;

            case State.Fleeing:
                Flee();
                // Check for transitions
                if (!isLit && Vector3.Distance(transform.position, agent.destination) < 1.0f)
                {
                    currentState = State.Patrolling; // Cooldown / stop fleeing
                    agent.speed = patrolSpeed; // --- NEW ---
                }
                break;
        }
    }

    // This is YOUR CheckIfLit function, which is the correct one to use.
    void CheckIfLit()
    {
        // Add a safety check in case playerFlashlight was never found
        // Use the public IsLightOn() function from FlashlightSystem.cs
        if (playerFlashlight == null || !playerFlashlight.IsLightOn()) 
        {
            isLit = false;
            return;
        }

        // We need to get the public 'flashlight' variable from the FlashlightSystem.
        // Let's modify FlashlightSystem.cs to make 'flashlight' public.
        // --- Assuming 'flashlight' variable in FlashlightSystem is public ---
        Light light = playerFlashlight.flashlight; 
        if (light == null)
        {
            isLit = false;
            return; // No light component to check against
        }

        Vector3 dirToEnemy = (transform.position - light.transform.position).normalized;
        float angleToEnemy = Vector3.Angle(light.transform.forward, dirToEnemy);

        // 1. Check if enemy is within the flashlight's cone angle
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