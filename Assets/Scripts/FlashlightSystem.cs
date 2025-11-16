using UnityEngine;
using UnityEngine.Events; // For potential UI events

public class FlashlightSystem : MonoBehaviour
{
    [Header("Flashlight")]
    public Light flashlight; // Assign your Spot Light component here
    public bool isLightOn = true;

    [Header("Battery")]
    public float maxBattery = 100f;
    public float currentBattery;
    public float drainRate = 2f; // Battery units per second
    public float rechargeRate = 10f; // Battery units per second

    [Header("Audio (Optional)")]
    public AudioClip toggleSound;
    private AudioSource audioSource;

    // Optional: For UI updates
    public UnityEvent<float> OnBatteryChanged; 

    void Start()
    {
        currentBattery = maxBattery;
        audioSource = GetComponent<AudioSource>(); // Add an AudioSource component if you want sound

        // Ensure light is in the correct state at start
        ToggleLight(isLightOn);
    }

    void Update()
    {
        // Toggle light with 'F' key
        if (Input.GetKeyDown(KeyCode.F))
        {
            ToggleLight(!isLightOn);
        }

        // Drain battery if the light is on
        if (isLightOn)
        {
            currentBattery -= drainRate * Time.deltaTime;
            currentBattery = Mathf.Max(currentBattery, 0f); // Clamp at 0

            OnBatteryChanged?.Invoke(currentBattery / maxBattery); // Update UI

            if (currentBattery <= 0)
            {
                ToggleLight(false); // Force light off
                GameManager.Instance.GameOver("Your flashlight ran out of energy!");
            }
        }
    }
    

    public void ToggleLight(bool state)
    {
        // Don't allow turning on if battery is dead
        if (state && currentBattery <= 0)
        {
            isLightOn = false;
            flashlight.enabled = false;
            return;
        }

        isLightOn = state;
        flashlight.enabled = isLightOn;

        if (audioSource && toggleSound)
        {
            audioSource.PlayOneShot(toggleSound);
        }
    }

    // Public method to be called by recharge stations
    public void Recharge()
    {
        if (currentBattery < maxBattery)
        {
            currentBattery += rechargeRate * Time.deltaTime;
            currentBattery = Mathf.Min(currentBattery, maxBattery); // Clamp at max
            OnBatteryChanged?.Invoke(currentBattery / maxBattery); // Update UI
        }
    }
    
    public bool IsLightOn() { return isLightOn; }
}