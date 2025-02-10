using UnityEngine;

public class TimeManager : MonoBehaviour
{
    public static TimeManager Instance { get; private set; }

    [SerializeField] private float timeInterpolationSpeed;
    private float timeVel;

    private void OnEnable()
    {
        Time.timeScale = 1;

        if (Instance == null || Instance == this)
        {
            Instance = this;
            return;
        }

        Destroy(this);
    }

    public bool Interpolate { get; set; } = true;
    public void SetScale(float scale) => Time.timeScale = Mathf.Clamp(scale, 0, Mathf.Infinity);

    public void StopTime()
    {
        Interpolate = false;
        Time.timeScale = 0;
        timeVel = 0;
    }

    public void ResumeTime()
    {
        Time.timeScale = 1;
        Interpolate = true;
        timeVel = 0;
    }

    public void Update()
    {
        if (!Interpolate) return;

        Time.timeScale = Mathf.Max(0, Mathf.SmoothDamp(Time.timeScale, 1, ref timeVel, timeInterpolationSpeed, Mathf.Infinity, Time.unscaledDeltaTime));
    }
}
