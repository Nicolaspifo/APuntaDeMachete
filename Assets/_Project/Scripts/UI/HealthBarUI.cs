using UnityEngine;
using UnityEngine.UI;

public class HealthBarUI : MonoBehaviour
{
    [SerializeField] Health target;
    Slider slider;

    void Awake() => slider = GetComponent<Slider>();

    void OnEnable()
    {
        target.Changed += Refresh;
        Refresh(target.Current, target.Max);
    }

    void OnDisable() => target.Changed -= Refresh;

    void Refresh(float current, float max)
    {
        slider.maxValue = max;
        slider.value = current;
    }
}
