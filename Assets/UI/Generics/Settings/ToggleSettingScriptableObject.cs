using UnityEngine;

[CreateAssetMenu(menuName = "Settings/Toggle Setting")]
public class ToggleSettingScriptableObject : SettingsScriptableObject
{
    public void Save(float val)
    {
        PlayerPrefs.SetFloat(SaveName, val);
    }

    public float Load()
    {
        return PlayerPrefs.GetFloat(SaveName);
    }
}
