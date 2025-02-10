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
    [SerializeField] public Vector3 recoil;

    [Header("Interpolation")]
    [SerializeField] private float lineInterpolation;

    [Header("SFX")]
    [SerializeField] private AudioClip lunge;

    private Vector3 posVel;

    public void InitLine(PlayerCamera cam)
    {
        Vector3 startPosition = cam.CamComponent.transform.position - Vector3.up + cam.CamComponent.transform.right;

        line.enabled       = true;
        line.positionCount = 2;
        line.SetPosition(0, startPosition);
        line.SetPosition(1, startPosition);

        ClearLine();
        SetActive(true);

        fireParticles.transform.position = startPosition;
        fireParticles.Play();

        posVel = Vector3.zero;
    }

    public void ClearLine()
    {
        fireParticles.Stop();
        fireParticles.Clear();
    }

    public void SetActive(bool active)
    {
        line.gameObject.SetActive(active);
        line.enabled = active;
    }

    public void InterpolateLine()
    {
        line.SetPosition(1, Vector3.SmoothDamp(line.GetPosition(1), reticle.LatchObject.transform.position, ref posVel, lineInterpolation));
        fireParticles.transform.position = line.GetPosition(1);
    }
}
