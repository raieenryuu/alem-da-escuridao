using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    [Header("Target")]
    public Transform target; // The player to follow

    [Header("Settings")]
    public float smoothSpeed = 0.125f; // How fast the camera follows (lower is slower)

    private Vector3 offset;
    private bool offsetCalculated = false;

    void Start()
    {
        // We wait for the target to be set before calculating the offset
    }

    void LateUpdate()
    {
        if (target == null)
        {
            Debug.LogWarning("CameraFollow script has no target assigned.");
            return;
        }

        // Calculate the initial offset on the first frame a target is available
        // This makes sure our camera's initial position in the Scene editor is respected
        if (!offsetCalculated)
        {
            offset = transform.position - target.position;
            offsetCalculated = true;
        }

        // The camera's desired position is the player's position + the original offset
        Vector3 desiredPosition = target.position + offset;

        // Use Lerp for a simple, smooth follow.
        // For an instant, "hard-locked" follow, use this instead:
        // transform.position = desiredPosition;

        Vector3 smoothedPosition = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed);
        transform.position = smoothedPosition;
    }
}