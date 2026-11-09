using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

public class VictoryFlow : MonoBehaviour
{
    [SerializeField] GameObject panel;
    [SerializeField] GameObject continueButton;   // queda seleccionado al abrir
    [SerializeField] Health playerHealth;         // opcional: evita ganar si el jugador ya murió
    [SerializeField] float showDelay = 1.5f;
    [SerializeField] string menuSceneName = "MenuPrincipal";

    [Header("Modo infinito")]
    [Tooltip("Opcional: otros scripts a reactivar al continuar. Los EnemySpawner se reactivan solos")]
    [SerializeField] Behaviour[] enableOnEndless;

    public static bool IsEndless { get; private set; }

    bool shown;

    void Awake()
    {
        IsEndless = false;   // es estático: se reinicia al cargar la escena
        if (panel != null) panel.SetActive(false);
    }

    void OnEnable()  => GameEvents.BossDefeated += HandleDefeated;
    void OnDisable() => GameEvents.BossDefeated -= HandleDefeated;

    void HandleDefeated()
    {
        if (shown || IsEndless || !isActiveAndEnabled) return;
        StartCoroutine(ShowRoutine());
    }

    IEnumerator ShowRoutine()
    {
        yield return new WaitForSeconds(showDelay);              // deja ver la muerte y el café
        yield return new WaitUntil(() => Time.timeScale > 0f);   // espera si hay cartas abiertas

        if (playerHealth != null && playerHealth.Current <= 0f) yield break;
        Show();
    }

    void Show()
    {
        shown = true;
        panel.SetActive(true);
        Time.timeScale = 0f;
        EventSystem.current.SetSelectedGameObject(null);
        EventSystem.current.SetSelectedGameObject(continueButton);
    }

    public void ContinueEndless()
    {
        panel.SetActive(false);
        Time.timeScale = 1f;
        IsEndless = true;

        // NUEVO: reactiva los spawners que se apagaron al aparecer el jefe,
        // estén desactivados como objeto o solo como script.
        foreach (var spawner in FindObjectsByType<EnemySpawner>(FindObjectsInactive.Include))
        {
            spawner.gameObject.SetActive(true);
            spawner.enabled = true;
        }

        foreach (var b in enableOnEndless)
            if (b != null) b.enabled = true;

        EventSystem.current.SetSelectedGameObject(null);
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

    [ContextMenu("Probar: mostrar victoria")]
    void TestShow() => Show();
}