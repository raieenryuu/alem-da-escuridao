using UnityEngine;
using UnityEngine.AI; // Required for NavMeshAgent
using UnityEngine.UI; // Required for the Slider

[RequireComponent(typeof(NavMeshAgent))]
public class LightSensitiveEnemy : MonoBehaviour
{
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
    public Transform playerTarget; 

    [Header("Stats")]
    public float patrolSpeed = 1.5f; 
    public float chaseSpeed = 3.5f; 
    public float fleeSpeed = 5.0f; 
    public float chaseDistance = 10f;
    public float fleeDistance = 15f; 
    public float patrolRange = 10f;

    [Header("Health")]
    public float maxHealth = 100f;
    public float damageRate = 20f; 
    public Slider healthBarSlider; 
    private float currentHealth;
    private bool isDead = false;

    [Header("Light Detection")]
    public float viewAngle = 30f; 
    private FlashlightSystem playerFlashlight;
    private bool isLit = false;

    private Animator animator; 
    private Collider enemyCollider; // Reference to collider for center-mass checks
    
    private Vector3 patrolPoint;
    private float patrolTimer;

    private void SetState(State newState)
    {
        if (currentState == newState) return; 

        Debug.LogWarning($"ENEMY ({name}): State changing from {currentState} -> {newState}");
        currentState = newState;

        switch (newState)
        {
            case State.Patrolling:
                agent.speed = patrolSpeed;
                break;
            case State.Chasing:
                agent.speed = chaseSpeed;
                break;
            case State.Fleeing:
                agent.speed = fleeSpeed;
                break;
        }
    }

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponentInChildren<Animator>(); 
        
        // --- NEW: Get the collider to find center mass ---
        enemyCollider = GetComponent<Collider>();
        if (enemyCollider == null) Debug.LogError($"ENEMY ({name}): No Collider found!");
        // --- END NEW ---

        currentHealth = maxHealth;
        if (healthBarSlider != null)
        {
            healthBarSlider.minValue = 0;
            healthBarSlider.maxValue = 1; 
            healthBarSlider.value = 1;
            healthBarSlider.transform.parent.gameObject.SetActive(true);
        }

        if (playerTarget != null)
        {
            playerFlashlight = playerTarget.GetComponentInChildren<FlashlightSystem>();
        }

        SetState(State.Patrolling);
        SetNewPatrolPoint();
    }

    void Update()
    {
        if (isDead || playerTarget == null) return;

        if (animator != null)
        {
            animator.SetFloat("Speed", agent.velocity.magnitude);
        }

        CheckIfLit(); 

        switch (currentState)
        {
            case State.Patrolling:
                Patrol();
                if (isLit)
                {
                    SetState(State.Fleeing);
                }
                else if (Vector3.Distance(transform.position, playerTarget.position) <= chaseDistance)
                {
                    SetState(State.Chasing);
                }
                break;

            case State.Chasing:
                Chase();
                if (isLit)
                {
                    SetState(State.Fleeing);
                }
                else if (Vector3.Distance(transform.position, playerTarget.position) > chaseDistance)
                {
                    SetState(State.Patrolling);
                }
                break;

            case State.Fleeing:
                Flee();
                
                // Velocity check (Robust against stopping distance bugs)
                if (!isLit && !agent.pathPending) 
                {
                    if (agent.hasPath) 
                    {
                        Debug.LogWarning($"ENEMY ({name}): Flee complete. Returning to patrol.");
                        SetState(State.Patrolling);
                    }
                }
                break;
        }
    }

    void CheckIfLit()
    {
        if (playerFlashlight == null || enemyCollider == null || !playerFlashlight.IsLightOn())
        {
            isLit = false;
            return;
        }

        Light light = playerFlashlight.flashlight; 
        if (light == null)
        {
            isLit = false;
            return; 
        }

        // --- FIX: Raycast from light to Enemy Center (Chest), not Pivot (Feet) ---
        Vector3 rayOrigin = light.transform.position;
        Vector3 targetPoint = enemyCollider.bounds.center; 
        Vector3 dirToEnemy = (targetPoint - rayOrigin).normalized;
        // --- END FIX ---

        float angleToEnemy = Vector3.Angle(light.transform.forward, dirToEnemy);

        if (angleToEnemy <= light.spotAngle / 2f)
        {
            if (Physics.Raycast(rayOrigin, dirToEnemy, out RaycastHit hit, light.range))
            {
                // Check if we hit this enemy OR any child parts
                if (hit.collider.transform.root == this.transform)
                {
                    if (!isLit) Debug.Log($"ENEMY ({name}): Light is ON ME!"); 
                    isLit = true;
                    TakeDamage(damageRate * Time.deltaTime);
                    return;
                }
            }
        }

        isLit = false;
    }

    void TakeDamage(float amount)
    {
        if (isDead) return;

        currentHealth -= amount;
        currentHealth = Mathf.Max(currentHealth, 0f); 

        if (healthBarSlider != null)
        {
            healthBarSlider.value = currentHealth / maxHealth;
        }

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    void Die()
    {
        if (isDead) return;
        
        isDead = true;
        Debug.LogWarning($"ENEMY ({name}): Has DIED!");

        // --- THIS IS THE FIX ---
        // 1. Capture the agent's current momentum (velocity) BEFORE we disable it.
        Vector3 momentumDirection = agent.velocity;
        
        // 2. Stop all AI and Animation control
        if (agent != null)
        {
            agent.isStopped = true;
            agent.enabled = false; // Disable the agent completely
        }
        if (animator != null)
        {
            animator.enabled = false; // Stop the animator from fighting physics
        }
        this.enabled = false; // Disable this script

        // 3. Setup the "Stiff Ragdoll"
        Rigidbody rb = GetComponent<Rigidbody>();
        Collider col = GetComponent<Collider>(); 

        if (rb != null && col != null)
        {
            rb.isKinematic = false; // Turn on physics
            rb.useGravity = true;
            rb.WakeUp(); // Tell physics to start working NOW

            // 4. Check if we actually had any momentum
            Vector3 pushDir;
            if (momentumDirection.magnitude > 0.1f)
            {
                // We were moving. Use that as the direction.
                pushDir = momentumDirection.normalized;
                Debug.Log($"Enemy died while moving. Pushing in direction {pushDir}");
            }
            else
            {
                // We were standing still. Fall backwards as a fallback.
                pushDir = -transform.forward;
                Debug.Log($"Enemy died while still. Plling backwards.");
            }

            // 5. Find a "chest" point to apply the force
            Vector3 chestPoint = col.bounds.center + new Vector3(0, col.bounds.extents.y * 0.75f, 0);

            // Add a slight upward lift to help it topple over
            pushDir.y = 0.2f; 

            // 6. Apply the force AT THAT CHEST POINT
            rb.AddForceAtPosition(pushDir.normalized * 10f, chestPoint, ForceMode.Impulse);
        }

        // 7. Hide Health Bar
        if (healthBarSlider != null)
        {
            healthBarSlider.transform.parent.gameObject.SetActive(false);
        }
        
        // 8. Cleanup
        Destroy(gameObject, 5f); 
    }

    void Patrol()
    {
        agent.isStopped = false;
        
        if (!agent.pathPending && agent.velocity.magnitude < 0.1f && agent.hasPath)
        {
            patrolTimer += Time.deltaTime;
            
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
        
        Vector3 runDirection = (transform.position - playerTarget.position).normalized;
        Vector3 fleeTarget = transform.position + runDirection * fleeDistance;

        if (NavMesh.SamplePosition(fleeTarget, out NavMeshHit hit, fleeDistance, NavMesh.AllAreas))
        {
            if (Vector3.Distance(agent.destination, hit.position) > 1.5f)
            {
                agent.SetDestination(hit.position);
            }
        }
        else
        {
            agent.SetDestination(transform.position - runDirection * 2f);
        }
    }
}