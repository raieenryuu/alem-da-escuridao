using TMPro;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [Header("Game Over UI")]
    public GameObject gameOverUI;
    public TextMeshProUGUI reasonText;

    [Header("Game Win UI")]
    public GameObject gameWinUI;
    public TextMeshProUGUI winReasonText;

    void Awake()
    {
        Instance = this;
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

    public void Restart()
    {
        Time.timeScale = 1f;
        UnityEngine.SceneManagement.SceneManager.LoadScene(
            UnityEngine.SceneManagement.SceneManager.GetActiveScene().buildIndex
        );
    }

    public void Quit()
    {
        Application.Quit();
    }
}
