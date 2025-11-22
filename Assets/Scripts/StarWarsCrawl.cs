using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro; // Retained for both text types

public class StarWarsCrawl : MonoBehaviour
{
    [Header("Text Configuration")]
    [Tooltip("Drag the TextMeshPro object here that contains the crawl text.")]
    public TextMeshPro crawlText;

    [Tooltip("Drag the TextMeshPro object here to show controls at the bottom.")]
    public TextMeshPro controlsText;

    public string nextSceneName = "Nave";

    [Header("Crawl Speed")]
    public float baseScrollSpeed = 0.5f;
    public float fastScrollMultiplier = 3.0f;

    private float currentScrollSpeed;
    private bool isSkipping = false;
    private bool hasStarted = false;

    // Unused position variables are removed for clarity

    // Controls Text Content
    private const string CONTROLS_TEXT = "[SPACE] or [CLICK] to Speed Up   |   [ENTER/RETURN] to Skip Story";


    void Start()
    {
        if (crawlText == null)
        {
            Debug.LogError("CrawlText is not assigned in the StarWarsCrawl script!");
            return;
        }

        if (controlsText != null)
        {
            controlsText.text = CONTROLS_TEXT;
            controlsText.gameObject.SetActive(true);
        }

        // Removed complex calculations from Start() as they were confusing and unused.

        currentScrollSpeed = baseScrollSpeed;
        hasStarted = true;
    }

    void Update()
    {
        if (!hasStarted) return;

        // 1. Check for Speed-Up Input (Hold Space or Left Mouse Button)
        if (Input.GetKey(KeyCode.Space) || Input.GetKey(KeyCode.Mouse0))
        {
            currentScrollSpeed = baseScrollSpeed * fastScrollMultiplier;
            isSkipping = true;
        }
        else
        {
            currentScrollSpeed = baseScrollSpeed;
            isSkipping = false;
        }

        // 2. Continuous Scrolling
        // Move the text up relative to time and the current speed.
        crawlText.rectTransform.Translate(Vector3.up * currentScrollSpeed * Time.deltaTime, Space.World);

        // 3. Check for End of Crawl (Scrolls off-screen) or Immediate Skip (Press Return/Enter)

        // Calculate the world Y position of the TOP edge of the text block.
        // position.y is the center, so we add half the height.
        float textTopY = crawlText.rectTransform.position.y + (crawlText.rectTransform.rect.height / 2f);

        // FIX: The exit threshold is increased to 100f to accommodate large amounts of text.
        const float OFF_SCREEN_EXIT_Y = 100f;

        if (textTopY > OFF_SCREEN_EXIT_Y || Input.GetKeyDown(KeyCode.Return))
        {
            LoadNextScene();
        }
    }

    public void LoadNextScene()
    {
        // Prevent multiple loads
        if (SceneManager.GetActiveScene().name == nextSceneName) return;

        // Disable controls text when loading scene
        if (controlsText != null)
        {
            controlsText.gameObject.SetActive(false);
        }

        // Load the main game scene
        SceneManager.LoadScene(nextSceneName);
    }
}