using UnityEngine;

public class SettingsScriptableObject : ScriptableObject
{
    public enum SettingType
    {
        Toggle,
        Slider,
        SubMenu,
    };

    [SerializeField] private string saveName;
    [SerializeField] private SettingType type;

    public string SaveName  => saveName;
    public SettingType Type => type;
}
