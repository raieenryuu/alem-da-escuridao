using UnityEngine;

// This script handles isometric player movement and aiming.
// It assumes a Rigidbody for physics-based movement.

[RequireComponent(typeof(Rigidbody))]
public class PlayerMovement : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 5f;

    [Header("Dependencies")]
    public Camera mainCamera; // Assign your main isometric camera here

    [Header("Aiming")]
    public LayerMask groundLayer; // Set this to your 'Ground' layer

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
        Aim();
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
}