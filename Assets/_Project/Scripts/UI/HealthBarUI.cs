using UnityEngine;
using UnityEngine.UI;

public class HealthBarUI : MonoBehaviour
{
    [SerializeField] Health target;
    Slider slider;

    void Awake() => slider = GetComponent<Slider>();

    void OnEnable()
    {
        target.OnHealthChanged += Refresh;
        Refresh(target.Current, target.Max);
    }

    void OnDisable() => target.OnHealthChanged -= Refresh;

    void Refresh(int current, int max)
    {
        slider.maxValue = max;
        slider.value = current;
    }
}
