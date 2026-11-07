using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class UpgradeCardsUI : MonoBehaviour
{
    [SerializeField] PlayerXP playerXP;
    [SerializeField] PlayerRuntimeStats stats;
    [SerializeField] Health playerHealth;
    [SerializeField] GameObject panel;
    [SerializeField] UpgradeCardView[] cards;
    [SerializeField] List<UpgradeData> pool;
    [SerializeField] float inputDelay = 0.4f;

    public event System.Action<UpgradeData> OnUpgradeChosen;

    int pendingLevelUps;
    bool isOpen;

    bool IsAvailable(UpgradeData u) => stats == null || stats.CanApply(u);

    void OnEnable()  => playerXP.OnLevelUp += HandleLevelUp;
    void OnDisable() => playerXP.OnLevelUp -= HandleLevelUp;

    void HandleLevelUp(int level)
    {
        if (playerHealth != null && playerHealth.IsDead) return;
        if (pool == null || !pool.Exists(IsAvailable)) return;

        pendingLevelUps++;
        if (!isOpen) Open();
    }

    void Open()
    {
        if (cards == null || cards.Length == 0)
        {
            Debug.LogWarning("UpgradeCardsUI: falta asignar las cartas en Cards.");
            pendingLevelUps = 0;
            return;
        }

        isOpen = true;
        pendingLevelUps--;
        Time.timeScale = 0f;
        panel.SetActive(true);
        ShowRandomCards();
        StartCoroutine(EnableInputAfterDelay());
    }

    void ShowRandomCards()
    {
        var options = pool.FindAll(IsAvailable);
        for (int i = options.Count - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);
            (options[i], options[j]) = (options[j], options[i]);
        }

        for (int i = 0; i < cards.Length; i++)
        {
            bool hasOption = i < options.Count;
            cards[i].gameObject.SetActive(hasOption);
            if (!hasOption) continue;

            cards[i].Setup(options[i], Choose);
            cards[i].Button.interactable = false;
        }
    }

    IEnumerator EnableInputAfterDelay()
    {
        yield return new WaitForSecondsRealtime(inputDelay);

        foreach (var card in cards)
            if (card.gameObject.activeSelf) card.Button.interactable = true;

        EventSystem.current.SetSelectedGameObject(null);
        EventSystem.current.SetSelectedGameObject(cards[0].gameObject);
    }

    void Choose(UpgradeData upgrade)
    {
        if (stats != null) stats.Apply(upgrade);
        OnUpgradeChosen?.Invoke(upgrade);

        panel.SetActive(false);
        isOpen = false;

        if (pendingLevelUps > 0 && pool.Exists(IsAvailable)) Open();
        else
        {
            pendingLevelUps = 0;
            Time.timeScale = 1f;
        }
    }

    [ContextMenu("Probar: abrir cartas")]
    void TestOpen() => HandleLevelUp(0);
}