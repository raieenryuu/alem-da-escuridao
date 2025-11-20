using UnityEngine;

[RequireComponent(typeof(Collider))]
public class RechargeStation : MonoBehaviour
{
    [Header("StationLight")] public Light StationLight;
    
    
    // This script assumes the collider is set to "Is Trigger"
    // It will recharge any player that stays within its trigger zone


    private bool hasRecharge = true;

    void Start()
    {
        // Ensure the collider is a trigger
        GetComponent<Collider>().isTrigger = true;
    }

    void OnTriggerStay(Collider other)
    {
        // Check if the object inside the trigger is the Player
        if (other.CompareTag("Player"))
        {
            // Find the flashlight system on the player
            FlashlightSystem flashlight = other.GetComponent<FlashlightSystem>();
            if (flashlight != null && hasRecharge)
            {
                // Call the recharge method
                flashlight.Recharge();
                hasRecharge = false;
                this.StationLight.enabled = false;
            }
        }
    }

    // Optional: Add visual feedback
    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            // e.g., Play a "power up" sound or turn on a light
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            // e.g., Stop the "power up" sound
        }
    }
}