using UnityEngine;

public class TimeManager : MonoBehaviour
{
    public static TimeManager Instance { get; private set; }

    [SerializeField] private float timeInterpolationSpeed;
    private float timeVel;
    private bool interpolate;

    private void OnEnable()
    {
        if (Instance == null || Instance == this)
        {
            Instance = this;
            return;
        }

        Destroy(this);
    }

    public void SetScale(float scale) => Time.timeScale = scale;

    public void StopTime()
    {
        interpolate = false;
        Time.timeScale = 0;
    }

    public void ResumeTime()
    {
        Time.timeScale = 1;
        interpolate = true;
    }

    public void Update()
    {
        if (!interpolate) return;

        Time.timeScale = Mathf.SmoothDamp(Time.timeScale, 1, ref timeVel, timeInterpolationSpeed, Mathf.Infinity, Time.unscaledDeltaTime);
    }
}
