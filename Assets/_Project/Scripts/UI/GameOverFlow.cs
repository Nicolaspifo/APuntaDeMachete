using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

public class GameOverFlow : MonoBehaviour
{
    [SerializeField] Health playerHealth;
    [SerializeField] GameObject panel;
    [SerializeField] GameObject restartButton;

    void Awake()
    {
        if (panel != null) panel.SetActive(false);
        Time.timeScale = 1f;
    }

    void OnEnable()
    {
        if (playerHealth != null) playerHealth.Died += ShowGameOver;
        else Debug.LogError("GameOverFlow: falta asignar playerHealth.", this);
    }

    void OnDisable()
    {
        if (playerHealth != null) playerHealth.Died -= ShowGameOver;
    }

    void Update()
    {
        if (panel.activeSelf && EventSystem.current.currentSelectedGameObject == null)
            EventSystem.current.SetSelectedGameObject(restartButton);
    }

    void ShowGameOver()
    {
        panel.SetActive(true);
        Time.timeScale = 0f;
        EventSystem.current.SetSelectedGameObject(null);
        EventSystem.current.SetSelectedGameObject(restartButton);
    }

    public void Restart()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}