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
        PauseControl.Reset();
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
        PauseControl.Push(this);
        EventSystem.current.SetSelectedGameObject(null);
        EventSystem.current.SetSelectedGameObject(restartButton);
    }

    public void Restart()
    {
        PauseControl.Reset();
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}