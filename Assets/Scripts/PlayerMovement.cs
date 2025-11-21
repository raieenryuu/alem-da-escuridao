using UnityEngine;

// This script handles isometric player movement, aiming, animations, AND Footsteps
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
        if (mainCamera == null)
        {
            mainCamera = Camera.main;
            Debug.LogWarning("Main Camera not assigned. Defaulting to Camera.main.");
        }
    }

    void Update()
    {
        ProcessInput();
        HandleAnimations();
        Aim();
        HandleFootsteps(); // <-- Added this back in
    }

    void FixedUpdate()
    {
        Move();
    }

    void ProcessInput()
    {
        float horizontal = Input.GetAxisRaw("Horizontal"); // A/D keys
        float vertical = Input.GetAxisRaw("Vertical");     // W/S keys

        // Get camera's forward and right vectors
        Vector3 camForward = mainCamera.transform.forward;
        Vector3 camRight = mainCamera.transform.right;

        // We want to move along the ground plane, so we zero out the Y component
        camForward.y = 0;
        camRight.y = 0;

        // Normalize to ensure consistent speed
        camForward.Normalize();
        camRight.Normalize();

        // Calculate the move direction relative to the camera
        moveDirection = (camForward * vertical + camRight * horizontal).normalized;
    }

    void Move()
    {
        // Apply movement to the Rigidbody
        rb.MovePosition(rb.position + moveDirection * moveSpeed * Time.fixedDeltaTime);
    }

    void HandleAnimations()
    {
        if (anim == null) return;

        // If the player is moving
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

                    
                SoundFXManager.instance.PlayRandomSoundEffectClip(footstepsSounds, transform, stepVolume);
            }
        }
        else
        {
            // If we stop moving, reset the timer so the next step is immediate when we start again
            footstepTimer = 0;
        }
    }
}