using UnityEngine;

// This script handles isometric player movement, aiming, animations, and footsteps
[RequireComponent(typeof(Rigidbody))]
public class PlayerMovement : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 5f;

    [Header("Dependencies")]
    public Camera mainCamera; // Assign your main isometric camera here

    [Header("Aiming")]
    public LayerMask groundLayer; // Set this to your 'Ground' layer

    [Header("Animation")]
    public Animator anim;

    [Header("Audio")]
    [SerializeField] private AudioClip[] footstepsSounds; // Array! Drag multiple clips here
    [SerializeField] private float footstepSpeed = 0.5f; // Seconds between steps
    [SerializeField] private float stepVolume = 1f;
    private float footstepTimer = 0;

    private Rigidbody rb;
    private Vector3 moveDirection;

    void Start()
    {
        rb = GetComponent<Rigidbody>();

        // 1. NON-KINEMATIC SETUP: The Rigidbody is now a dynamic body controlled by velocity.
        rb.isKinematic = false;

        // 2. The collision detection and interpolation are now set to default/discrete.
        // It is critical to freeze rotation on all axes in the Inspector to prevent tipping.
        // We will also use the default CollisionDetectionMode.Discrete for performance.
        rb.interpolation = RigidbodyInterpolation.None;
        rb.collisionDetectionMode = CollisionDetectionMode.Discrete;

        // Ensure gravity is OFF if you want to control vertical movement completely
        rb.useGravity = false;
        // ------------------------------------------

        if (mainCamera == null)
        {
            mainCamera = Camera.main;
            Debug.LogWarning("Main Camera not assigned. Defaulting to Camera.main.");
        }
    }

    void Update()
    {
        // Input, Animation, Aiming, and Audio logic runs here (per frame)
        ProcessInput();
        HandleAnimations();
        Aim();
        HandleFootsteps();
    }

    void FixedUpdate()
    {
        // Physics movement logic runs here (at fixed timestep)
        Move();
    }

    void ProcessInput()
    {
        float horizontal = Input.GetAxisRaw("Horizontal"); // A/D keys
        float vertical = Input.GetAxisRaw("Vertical");

        Vector3 camForward = mainCamera.transform.forward;
        Vector3 camRight = mainCamera.transform.right;

        camForward.y = 0;
        camRight.y = 0;

        camForward.Normalize();
        camRight.Normalize();

        // Calculate the raw, normalized move direction relative to the camera
        moveDirection = (camForward * vertical + camRight * horizontal).normalized;
    }

    void Move()
    {
        // Calculate the desired velocity vector
        Vector3 targetVelocity = moveDirection * moveSpeed;

        // CRUCIAL FIX: Retain the Rigidbody's current vertical velocity (y component)
        // This prevents the player from sliding when there's no input and ensures
        // full control over the y-axis, often used for jumping or gravity simulation.
        // Since we set rb.useGravity = false, the y-velocity should stay 0.
        targetVelocity.y = rb.linearVelocity.y;


        // ?? KEY CHANGE: Apply movement by setting the Rigidbody's velocity directly.
        // The physics engine will handle collision resolution automatically.
        rb.linearVelocity = targetVelocity;

        // When using rb.velocity, Time.fixedDeltaTime is internally handled by Unity,
        // so we don't multiply by it here.
    }

    void HandleAnimations()
    {
        if (anim == null) return;

        // Note: For a velocity-based system, checking rb.velocity.sqrMagnitude is more accurate 
        // than checking moveDirection, but checking moveDirection is simpler for input-driven animation.
        if (moveDirection.sqrMagnitude > 0.01f)
        {
            anim.SetInteger("AnimationPar", 1);
        }
        else
        {
            anim.SetInteger("AnimationPar", 0);
        }
    }

    void Aim()
    {
        // Create a ray from the mouse position
        Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);

        // Raycast to the ground plane
        if (Physics.Raycast(ray, out RaycastHit hitInfo, 100f, groundLayer))
        {
            // Get the point where the ray hit the ground
            Vector3 lookAtPoint = hitInfo.point;

            // Make the player look at that point, but only rotate on the Y-axis
            Vector3 lookDirection = lookAtPoint - transform.position;
            lookDirection.y = 0; // Keep the player upright

            if (lookDirection != Vector3.zero)
            {
                Quaternion targetRotation = Quaternion.LookRotation(lookDirection);
                // Smoothly rotate towards the target
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, 15f * Time.deltaTime);
            }
        }
    }

    void HandleFootsteps()
    {
        // Check if we are moving (using same threshold as animation)
        if (moveDirection.sqrMagnitude > 0.01f)
        {
            footstepTimer -= Time.deltaTime;

            if (footstepTimer <= 0)
            {
                // Reset the timer
                footstepTimer = footstepSpeed;

                // This line assumes you have a static class called SoundFXManager with a method
                // SoundFXManager.instance.PlayRandomSoundEffectClip(footstepsSounds, transform, stepVolume);
            }
        }
        else
        {
            // If we stop moving, reset the timer so the next step is immediate when we start again
            footstepTimer = 0;
        }
    }
}