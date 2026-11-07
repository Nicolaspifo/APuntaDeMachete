using UnityEngine;

public class PlayerXP : MonoBehaviour
{
    [SerializeField] int baseXpToLevel = 5;
    [SerializeField] int xpIncreasePerLevel = 3;

    public int Level { get; private set; } = 1;
    public int CurrentXP { get; private set; } 
    public int XpToNextLevel => baseXpToLevel + (Level - 1) * xpIncreasePerLevel;

    public event System.Action<int, int, int> OnXPChanged;
    public event System.Action<int> OnLevelUp;

    public void AddXP(int amount)
    {
        if (amount <= 0) return;

        CurrentXP += amount;
        while (CurrentXP >= XpToNextLevel)
        {
            CurrentXP -= XpToNextLevel;
            Level++;
            OnLevelUp?.Invoke(Level);
        }
        OnXPChanged?.Invoke(CurrentXP, XpToNextLevel, Level);
    }

    [ContextMenu("Test +3 XP")]
    void TestAdd() => AddXP(3);
}
