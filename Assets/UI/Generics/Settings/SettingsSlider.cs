using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SettingsSlider : MonoBehaviour
{
    [SerializeField] private TMP_Text displayName;
    [SerializeField] private TMP_Text displayValue;
    [SerializeField] private Slider slider;

    public SliderSettingScriptableObject settings;

    public void SetName()
    {
        displayName.text = settings.SaveName;
    }

    public void SetValue()
    {
        displayValue.text = slider.value.ToString(settings.IntOnly ? "0" : "0.00");
    }
}
