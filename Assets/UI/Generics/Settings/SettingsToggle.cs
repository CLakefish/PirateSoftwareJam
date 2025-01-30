using TMPro;
using UnityEngine;

public class SettingsToggle : MonoBehaviour
{
    [SerializeField] private TMP_Text displayName;

    public ToggleSettingScriptableObject settings;

    public void SetName()
    {
        displayName.text = settings.SaveName;
    }
}
