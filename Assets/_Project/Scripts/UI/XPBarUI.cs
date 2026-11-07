using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class XPBarUI : MonoBehaviour
{
    [SerializeField] PlayerXP target;
    [SerializeField] TMP_Text levelLabel;
    Slider slider;

    void Awake() => slider = GetComponent<Slider>();

    void OnEnable()
    {
        target.OnXPChanged += Refresh;
        Refresh(target.CurrentXP, target.XpToNextLevel, target.Level);
    }

    void OnDisable() => target.OnXPChanged -= Refresh;

    void Refresh(int current, int needed, int level)
    {
        slider.maxValue = needed;
        slider.value = current;
        if (levelLabel != null) levelLabel.text = $"Nv. {level}";
    }
}