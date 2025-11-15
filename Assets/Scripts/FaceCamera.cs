using UnityEngine;

/// <summary>
/// Attach this script to a World Space Canvas (like an enemy health bar)
/// to make it always face the main camera.
/// </summary>
public class FaceCamera : MonoBehaviour
{
    private Camera mainCamera;

    void Start()
    {
        // Find the main camera in the scene and store a reference to it
        mainCamera = Camera.main;
    }

    void LateUpdate()
    {
        if (mainCamera == null)
        {
            Debug.Log("FaceCamera: Main Camera not found!");
            return;
        }

        // Make this object's "forward" direction point towards the camera
        // We use LateUpdate so it happens AFTER the camera has finished moving for the frame
        transform.LookAt(transform.position + mainCamera.transform.rotation * Vector3.forward,
            mainCamera.transform.rotation * Vector3.up);
    }
}