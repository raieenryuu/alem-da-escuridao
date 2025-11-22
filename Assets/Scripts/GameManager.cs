using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement; // Added for scene loading

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [Header("Game Over UI")]
    public GameObject gameOverUI;
    public TextMeshProUGUI reasonText;

    [Header("Game Win UI")]
    public GameObject gameWinUI;
    public TextMeshProUGUI winReasonText;

    [Header("Pause UI")]
    public GameObject pauseUI;

    private bool isPaused = false;

    // NOTE: Ensure your Main Menu scene is set as Build Index 0
    private const int MAIN_MENU_SCENE_INDEX = 0;

    void Awake()
    {
        Instance = this;
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            this.TogglePause();
        }
    }

    public void StartGame()
    {
        // CHANGED: Load the new introductory scene first
        Time.timeScale = 1f;
        SceneManager.LoadScene("StarWarsIntro");
    }

    public void GameOver(string reason)
    {
        Time.timeScale = 0f;

        if (reasonText != null)
            reasonText.text = reason;

        if (gameOverUI != null)
            gameOverUI.SetActive(true);
    }

    public void GameWin(string reason = "You Win!")
    {
        Time.timeScale = 0f;

        if (winReasonText != null)
            winReasonText.text = reason;

        if (gameWinUI != null)
            gameWinUI.SetActive(true);
    }

    public void TogglePause()
    {
        if (isPaused)
            Resume();
        else
            Pause();
    }

    public void Pause()
    {
        Time.timeScale = 0f;
        isPaused = true;

        if (pauseUI != null)
            pauseUI.SetActive(true);
    }

    public void Resume()
    {
        Time.timeScale = 1f;
        isPaused = false;

        if (pauseUI != null)
            pauseUI.SetActive(false);
    }

    // NEW FUNCTION: Quits the application entirely
    public void GameQuit()
    {
        Application.Quit();
        // NOTE: Application.Quit() only works in built executables. 
        // In the Unity Editor, Debug.Log is often used as a confirmation.
    }

    // RENAMED FUNCTION: Navigates to the main menu scene (Build Index 0)
    public void GoToMainMenu()
    {
        Time.timeScale = 1f; // Ensure time is running before scene change
        SceneManager.LoadScene(MAIN_MENU_SCENE_INDEX);
    }

    public void Restart()
    {
        Time.timeScale = 1f;
        UnityEngine.SceneManagement.SceneManager.LoadScene(
            UnityEngine.SceneManagement.SceneManager.GetActiveScene().buildIndex
        );
    }
}