using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

public class GameOverFlow : MonoBehaviour
{
    [SerializeField] Health playerHealth;
    [SerializeField] GameObject panel;
    [SerializeField] GameObject restartButton;

    void OnEnable() => playerHealth.OnDied += ShowGameOver;
    void OnDisable() => playerHealth.OnDied -= ShowGameOver;

    void ShowGameOver()
    {
        panel.SetActive(true);
        Time.timeScale = 0f;
        EventSystem.current.SetSelectedGameObject(restartButton);
    }

    public void Restart()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}
