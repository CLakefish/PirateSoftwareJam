using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelManager : MonoBehaviour
{
    public static LevelManager Instance { get; private set; }

    [SerializeField] private LevelScriptableObject currentLevel;
    [SerializeField] private LevelScriptableObject nextScene;
    [SerializeField] private bool lastLevel;
    [SerializeField] private float currentTime;

    public bool AllowTimeIncrement { get; set; } = true;
    public bool LastLevel => lastLevel;

    public float CurrentTime => currentTime;
    public float BestTime {
        get {
            if (currentLevel == null) return 0;
            return currentLevel.GetTime();
        }
    }

    private readonly HashSet<EnemyController> enemies = new();

    private void Awake()
    {
        if (Instance != null) return;

        Instance = this;

        var sceneEnemies = FindObjectsByType<EnemyController>(FindObjectsSortMode.None);

        foreach (var enemy in sceneEnemies) enemies.Add(enemy);
    }

    private void Update()
    {
        if (AllowTimeIncrement) currentTime += Time.unscaledDeltaTime;
    }

    public void ReloadScene()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void NextScene()
    {
        float t = currentLevel.GetTime();

        t = t == 0 ? currentTime : Mathf.Min(currentTime, t);

        currentLevel.SaveTime(t);
        nextScene.Load();
    }

    public bool CheckWin()
    {
        foreach (var enemy in enemies)
        {
            if (!enemy.HasTriggered) return false;
        }

        AllowTimeIncrement = false;

        return true;
    }
}
