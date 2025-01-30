using UnityEngine;

[CreateAssetMenu(menuName = "Settings/Slider Setting")]
public class SliderSettingScriptableObject : SettingsScriptableObject
{
    [SerializeField] private float min;
    [SerializeField] private float max;
    [SerializeField] private bool intOnly;

    public float Min => min;
    public float Max => max;
    public bool IntOnly => intOnly;

    public void Save(float val)
    {
        PlayerPrefs.SetFloat(SaveName, val);
    }

    public float Load()
    {
        return PlayerPrefs.GetFloat(SaveName);
    }
}
