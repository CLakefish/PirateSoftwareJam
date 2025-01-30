using TMPro;
using UnityEngine;

public class PlayerSpeedrunClock : MonoBehaviour
{
    [SerializeField] private TMP_Text displayTime;
    [SerializeField] private TMP_Text bestTime;
    private LevelManager levelManager;

    private void Start()
    {
        levelManager = LevelManager.Instance;

        if (levelManager.BestTime == 0)
        {
            gameObject.SetActive(false);
            return;
        }

        bestTime.text = "BEST TIME: " + levelManager.BestTime.ToString("00:00.00");
    }

    private void Update()
    {
        displayTime.text = levelManager.CurrentTime.ToString("00:00.00");
    }
}
