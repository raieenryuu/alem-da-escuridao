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
        Fleeing,
        Attacking // --- NEW STATE ---
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

    [Header("Combat")]
    public float attackDistance = 9.0f; // --- The agent will now stop at this distance ---
    public float attackRate = 1.5f; 
    private float nextAttackTime = 0f;

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
    private Collider enemyCollider; 
    
    private Vector3 patrolPoint;
    private float patrolTimer;

    // --- THIS FUNCTION IS THE FIX ---
    private void SetState(State newState)
    {
        if (currentState == newState) return; 

        Debug.LogWarning($"ENEMY ({name}): State changing from {currentState} -> {newState}");
        currentState = newState;

        // Stop agent before changing settings (safer)
        agent.isStopped = true;

        switch (newState)
        {
            case State.Patrolling:
                agent.speed = patrolSpeed;
                agent.stoppingDistance = 0f; // Go all the way to patrol points
                agent.isStopped = false; 
                break;
            case State.Chasing:
                agent.speed = chaseSpeed;
                agent.stoppingDistance = attackDistance; // Tell the agent to stop at attack range
                agent.isStopped = false; 
                break;
            case State.Fleeing:
                agent.speed = fleeSpeed;
                agent.stoppingDistance = 0f; // Go all the way to flee points
                agent.isStopped = false; 
                break;
            case State.Attacking:
                agent.stoppingDistance = attackDistance; // Ensure it stays stopped at range
                agent.isStopped = true; // Stop moving
                agent.velocity = Vector3.zero; 
                break;
        }
    }
    // --- END FIX ---

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponentInChildren<Animator>(); 
        
        enemyCollider = GetComponent<Collider>();
        if (enemyCollider == null) Debug.LogError($"ENEMY ({name}): No Collider found!");

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
        // --- TYPO FIX from last context ---
        SetNewPatrolPoint(); 
    }

    // --- THIS FUNCTION IS UPDATED ---
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
                

                Debug.Log("Distance from player before setting attacking state: " + Vector3.Distance(transform.position, playerTarget.position));
                Debug.Log("Validated to: " + (Vector3.Distance(transform.position, playerTarget.position) <= attackDistance ));
                
                if (isLit)
                {
                    SetState(State.Fleeing);
                }
                // --- FIX #1: Check distance directly, not velocity ---
                // If we are close enough to attack, ATTACK. Don't wait to stop.
                
                else if (Vector3.Distance(transform.position, playerTarget.position) <= attackDistance)
                {
                    SetState(State.Attacking);
                }
                // --- END FIX #1 ---
                else if (Vector3.Distance(transform.position, playerTarget.position) > chaseDistance)
                {
                    SetState(State.Patrolling);
                }
                break;

            case State.Attacking:
                AttackBehavior();

                if (isLit) 
                {
                    SetState(State.Fleeing);
                }
                // --- FIX #2: Add a "buffer" to stop jittering ---
                // Only start chasing again if the player is *fully* out of attack range.
                else if (Vector3.Distance(transform.position, playerTarget.position) > attackDistance + 0.5f) // 0.5f is the buffer
                {
                    SetState(State.Chasing);
                }
                // --- END FIX #2 ---
                break;

            case State.Fleeing:
                Flee();
                
                if (!isLit && !agent.pathPending) 
                {
                    if (agent.hasPath) 
                    {
                        SetState(State.Patrolling);
                    }
                }
                break;
        }
    }
    // --- END UPDATE ---

    void AttackBehavior()
    {
        // 1. Rotate to look at player (since agent is stopped)
        Vector3 direction = (playerTarget.position - transform.position).normalized;
        direction.y = 0; // Don't look up/down
        if (direction != Vector3.zero)
        {
            Quaternion lookRotation = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * 5f);
        }

        // 2. Check cooldown
        if (Time.time >= nextAttackTime)
        {
            if (animator != null)
            {
                animator.SetTrigger("Attack");
            }
            nextAttackTime = Time.time + attackRate;
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

        Vector3 rayOrigin = light.transform.position;
        Vector3 targetPoint = enemyCollider.bounds.center; 
        Vector3 dirToEnemy = (targetPoint - rayOrigin).normalized;

        float angleToEnemy = Vector3.Angle(light.transform.forward, dirToEnemy);

        // --- THIS IS THE FIX ---
        if (angleToEnemy <= light.spotAngle / 2f) // Was 'fs'
        // --- END FIX ---
        {
            if (Physics.Raycast(rayOrigin, dirToEnemy, out RaycastHit hit, light.range))
            {
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

        Vector3 momentumDirection = agent.velocity;
        
        if (agent != null)
        {
            agent.isStopped = true;
            agent.enabled = false; 
        }
        if (animator != null)
        {
            animator.enabled = false; 
        }
        this.enabled = false; 

        Rigidbody rb = GetComponent<Rigidbody>();
        Collider col = GetComponent<Collider>(); 

        if (rb != null && col != null)
        {
            rb.isKinematic = false; 
            rb.useGravity = true;
            rb.WakeUp(); 

            Vector3 pushDir;
            if (momentumDirection.magnitude > 0.1f)
            {
                pushDir = momentumDirection.normalized;
                Debug.Log($"Enemy died while moving. Pushing in direction {pushDir}");
            }
            else
            {
                pushDir = -transform.forward;
                Debug.Log($"Enemy died while still. Plling backwards.");
            }

            Vector3 chestPoint = col.bounds.center + new Vector3(0, col.bounds.extents.y * 0.75f, 0);
            pushDir.y = 0.2f; 

            rb.AddForceAtPosition(pushDir.normalized * 10f, chestPoint, ForceMode.Impulse);
        }

        if (healthBarSlider != null)
        {
            healthBarSlider.transform.parent.gameObject.SetActive(false);
        }
        
        Destroy(gameObject, 5f); 
    }

    void Patrol()
    {
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
        agent.SetDestination(playerTarget.position);
    }

    void Flee()
    {
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