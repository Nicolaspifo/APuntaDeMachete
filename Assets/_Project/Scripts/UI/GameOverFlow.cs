using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

public class GameOverFlow : MonoBehaviour
{
    [SerializeField] Health playerHealth;
    [SerializeField] GameObject panel;
    [SerializeField] GameObject restartButton;   // el que queda seleccionado al abrir
    [SerializeField] GameObject quitButton;
    [SerializeField] string menuSceneName = "MenuPrincipal";

    bool isGameOver;

    void Start()
    {
        // En la versión web no se puede cerrar el juego: se oculta el botón.
        if (quitButton != null && Application.platform == RuntimePlatform.WebGLPlayer)
            quitButton.SetActive(false);
    }

    void Update()
    {
        if (isGameOver || playerHealth == null) return;

        if (playerHealth.Current <= 0f) ShowGameOver();
    }

    void ShowGameOver()
    {
        isGameOver = true;
        panel.SetActive(true);
        Time.timeScale = 0f;
        EventSystem.current.SetSelectedGameObject(null);
        EventSystem.current.SetSelectedGameObject(restartButton);
    }

    public void Restart()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void GoToMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(menuSceneName);
    }

    public void Quit()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}