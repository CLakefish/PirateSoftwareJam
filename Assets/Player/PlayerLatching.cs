using UnityEngine;

public class PlayerLatching : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private LineRenderer line;
    [SerializeField] private ParticleSystem fireParticles;
    [SerializeField] private PlayerReticle reticle;

    [Header("Latching")]
    [SerializeField] public float exitLaunch;
    [SerializeField] public AnimationCurve latchLerp;
    [SerializeField] public float latchCamInterpolate;
    [SerializeField] public float latchLaunchGraceTime;
    [SerializeField] public float launchBufferTime;
    [SerializeField] public float latchLaunchTimeSlow;

    [Header("Interpolation")]
    [SerializeField] private float lineInterpolation;
    [SerializeField] private float loops;
    [SerializeField] private float intensity;

    [Header("SFX")]
    [SerializeField] private AudioClip lunge;

    private Transform camera;
    private Vector3 posVel;
    private Vector3 startPosition;
    private float totalDist;

    public void SetReticle(PlayerReticle reticle)
    {
        this.reticle = reticle;
        ClearParticles();
    }

    public void SetActive(bool active)
    {
        line.gameObject.SetActive(active);
        line.enabled = active;
    }

    public void InitLine(PlayerCamera cam)
    {
        camera = cam.CamComponent.transform;
        startPosition = cam.CamComponent.transform.position - Vector3.up + cam.CamComponent.transform.right;

        line.enabled       = true;
        line.positionCount = 2;
        line.SetPosition(0, startPosition);
        line.SetPosition(1, startPosition);

        ClearParticles();
        SetActive(true);

        fireParticles.transform.position = startPosition;
        fireParticles.Play();

        posVel    = Vector3.zero;
        //totalDist = GetDist();
    }

    public void InterpolateLine()
    {
/*        float dist = 1.0f - (GetDist() / totalDist);
        float sin  = Mathf.Sin(dist * loops) * intensity;
        float cos  = Mathf.Cos(dist * loops) * intensity;

        Vector3 offset = camera.right * cos + camera.forward * sin;*/
        line.SetPosition(1, Vector3.SmoothDamp(line.GetPosition(1), reticle.LatchObject.transform.position /*+ offset*/, ref posVel, lineInterpolation));
        fireParticles.transform.position = line.GetPosition(1);
    }

    public void ResetLine()
    {
        line.SetPosition(0, Vector3.zero);
        line.SetPosition(1, Vector3.zero);
    }

    public void ClearParticles()
    {
        fireParticles.Stop();
        fireParticles.Clear();
    }

    private float GetDist()
    {
        return Vector2.Distance(new Vector2(startPosition.x, startPosition.z), new Vector2(reticle.LatchObject.transform.position.x, reticle.LatchObject.transform.position.z));
    }
}
