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

        bestTime.text = "BEST TIME: " + GetFormattedText(levelManager.BestTime);
    }

    private void Update()
    {
        displayTime.text = GetFormattedText(levelManager.CurrentTime);
    }

    private string GetFormattedText(float time)
    {
        float seconds = time % 60;
        int minutes   = (int)time / 60;

        return new string(minutes.ToString("00") + ":" + seconds.ToString("00.00"));
    }
}
