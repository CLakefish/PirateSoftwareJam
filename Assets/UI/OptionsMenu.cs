using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class OptionsMenu : SubMenu
{
    [SerializeField] private Transform settingsPanel;
    [SerializeField] private Transform settingHolder;
    [SerializeField] private Toggle togglePrefab;
    [SerializeField] private Slider sliderPrefab;

    [Header("Levels")]
    [SerializeField] private List<SettingsScriptableObject> settings = new();

    [Header("Interpolation")]
    [SerializeField] private RectTransform openPosition;
    [SerializeField] private RectTransform closePosition;
    [SerializeField] private float smoothingTime;

    private readonly Dictionary<SettingsScriptableObject, GameObject> loadedSettings = new();
    private Coroutine anim;

    private void Awake()
    {
        settingsPanel.position = closePosition.position;
        settingsPanel.gameObject.SetActive(false);
    }

    public override void OnEnter()
    {
        if (loadedSettings.Count > 0)
        {
            RunAnimation(true);
            return;
        }

        foreach (var setting in settings)
        {
            GameObject temp = null;

            switch (setting.Type)
            {
                case SettingsScriptableObject.SettingType.Toggle:
                    Toggle t = Instantiate(togglePrefab, settingHolder);

                    var toggleSetting = setting as ToggleSettingScriptableObject;

                    SettingsToggle toggleData = t.GetComponent<SettingsToggle>();
                    toggleData.settings = setting as ToggleSettingScriptableObject;
                    toggleData.SetName();

                    t.isOn = toggleSetting.Load() == 1;
                    t.onValueChanged.AddListener((bool on) => { 
                        toggleSetting.Save(on ? 1 : 0); 
                    });

                    temp = t.gameObject;

                    break;

                case SettingsScriptableObject.SettingType.Slider:
                    Slider s = Instantiate(sliderPrefab, settingHolder);

                    var sliderSetting = setting as SliderSettingScriptableObject;

                    s.minValue     = sliderSetting.Min;
                    s.maxValue     = sliderSetting.Max;
                    s.wholeNumbers = sliderSetting.IntOnly;
                    s.value        = sliderSetting.Load();

                    var data      = s.GetComponent<SettingsSlider>();
                    data.settings = sliderSetting;
                    data.SetName();
                    data.SetValue();

                    s.onValueChanged.AddListener((float value) => { 
                        SettingsSlider sliderData = s.GetComponent<SettingsSlider>();
                        sliderData.SetValue();
                        sliderSetting.Save(value); 
                    });

                    temp = s.gameObject;

                    break;
            }

            if (temp == null) continue;
            loadedSettings.Add(setting, temp);
        }

        RunAnimation(true);
    }

    public override void OnExit()
    {
        RunAnimation(false);
    }

    private void RunAnimation(bool active)
    {
        if (anim != null) StopCoroutine(anim);
        anim = StartCoroutine(OpenAnimation(active));
    }

    private IEnumerator OpenAnimation(bool active)
    {
        Vector3 desiredPos = active ? openPosition.position : closePosition.position;
        Vector3 posVel = Vector3.zero;

        if (active) settingsPanel.gameObject.SetActive(true);

        while (Vector3.Distance(settingsPanel.position, desiredPos) > 0.01f)
        {
            settingsPanel.position = Vector3.SmoothDamp(settingsPanel.position, desiredPos, ref posVel, smoothingTime, Mathf.Infinity, Time.unscaledDeltaTime);
            yield return null;
        }

        settingsPanel.position = desiredPos;

        if (!active) settingsPanel.gameObject.SetActive(false);
    }
}
