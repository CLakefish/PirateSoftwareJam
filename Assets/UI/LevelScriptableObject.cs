using UnityEngine;
using UnityEngine.SceneManagement;

[CreateAssetMenu(menuName = "Level Info")]
public class LevelScriptableObject : ScriptableObject
{
    [SerializeField] private string sceneName;
    [SerializeField] private string displayName;
    [SerializeField] private string description;

    public string SceneName   => sceneName;
    public string DisplayName => displayName;
    public string Description => description;

    public void Load()
    {
        SceneManager.LoadScene(sceneName);
    }

    public void SaveTime(float time)
    {
        PlayerPrefs.SetFloat(displayName, time);
    }

    public float GetTime()
    {
        return PlayerPrefs.GetFloat(displayName);
    }
}
