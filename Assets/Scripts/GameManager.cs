using TMPro;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;
    public GameObject gameOverUI;
    public TextMeshProUGUI reasonText;

    void Awake()
    {
        Instance = this;
    }

    public void GameOver(string reason)
    {
        Time.timeScale = 0f;

        if (reasonText != null)
            reasonText.text = reason;

        gameOverUI.SetActive(true);
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
