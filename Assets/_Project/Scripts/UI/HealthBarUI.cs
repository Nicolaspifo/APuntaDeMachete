using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Slider))]
public class HealthBarUI : MonoBehaviour
{
    [SerializeField] Health target;
    Slider slider;

    void Awake()
    {
        slider = GetComponent<Slider>();
        if (target == null) Debug.LogError("HealthBarUI: falta asignar Health del jugador.", this);
    }

    void OnEnable()
    {
        if (target == null) return;
        target.Changed += Refresh;
        Refresh(target.Current, target.Max);
    }

    void Start()
    {
        if (target != null) Refresh(target.Current, target.Max);
    }

    void OnDisable()
    {
        if (target != null) target.Changed -= Refresh;
    }

    void Refresh(float current, float max)
    {
        slider.maxValue = max;
        slider.value = current;
    }
}