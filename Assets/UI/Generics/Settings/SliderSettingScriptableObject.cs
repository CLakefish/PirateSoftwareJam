using UnityEngine;

[CreateAssetMenu(menuName = "Settings/Slider Setting")]
public class SliderSettingScriptableObject : SettingsScriptableObject
{
    [SerializeField] private float min;
    [SerializeField] private float max;
    [SerializeField] private bool intOnly;

    [Header("Catch-all")]
    [SerializeField] private float startValue;

    public float Min => min;
    public float Max => max;
    public bool IntOnly => intOnly;

    public void Save(float val)
    {
        PlayerPrefs.SetFloat(SaveName, val);
    }

    public float Load()
    {
        if (!PlayerPrefs.HasKey(SaveName))
        {
            Save(startValue);
        }

        return PlayerPrefs.GetFloat(SaveName);
    }
}
