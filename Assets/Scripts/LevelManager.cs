using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelManager : MonoBehaviour
{
    public static LevelManager Instance { get; private set; }

    [SerializeField] private string nextScene;
    private readonly HashSet<EnemyController> enemies = new();

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;

            var sceneEnemies = FindObjectsByType<EnemyController>(FindObjectsSortMode.None);

            foreach (var enemy in sceneEnemies) enemies.Add(enemy);

            return;
        }

        Destroy(this);
    }

    public void ReloadScene()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void CheckWin()
    {
        foreach (var enemy in enemies)
        {
            if (!enemy.HasTriggered) return;
        }

        SceneManager.LoadScene(nextScene);
    }
}
