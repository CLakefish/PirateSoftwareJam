using System.Collections;
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
    [SerializeField] private int totalTicks = 8;
    [SerializeField] private float loops;
    [SerializeField] private float intensity;

    [Header("SFX")]
    [SerializeField] private AudioClip lunge;

    private Transform camera;
    private Vector3 startPosition;
    private Coroutine routine;

    public void SetReticle(PlayerReticle reticle)
    {
        this.reticle = reticle;
        ClearParticles();
    }

    public void SetActive(bool active)
    {
        line.enabled = active;

        if (!active) {
            if (routine != null) StopCoroutine(routine);
        }
    }

    public void InitLine(PlayerCamera cam)
    {
        camera        = cam.CamComponent.transform;
        startPosition = cam.CamComponent.transform.position - Vector3.up + cam.CamComponent.transform.right;

        if (routine != null) StopCoroutine(routine);
        routine = StartCoroutine(LatchAnim());

        ClearParticles();
        SetActive(true);

        fireParticles.transform.position = startPosition;
        fireParticles.Play();
    }

    public void ResetLine()
    {
        if (routine != null) StopCoroutine(routine);

        line.positionCount = 2;
        line.SetPosition(0, Vector3.zero);
        line.SetPosition(1, Vector3.zero);
    }

    public void ClearParticles()
    {
        fireParticles.Stop();
        fireParticles.Clear();
    }

    private IEnumerator LatchAnim()
    {
        line.enabled = true;

        line.positionCount = 1;
        line.SetPosition(0, startPosition);

        for (int i = 1; i < totalTicks; i++)
        {
            float d   = (float)(i - 1.0f) / totalTicks;
            float sin = Mathf.Sin(d * Mathf.PI) * intensity;
            Vector3 offset = camera.right * sin;
            Vector3 end    = Vector3.Lerp(startPosition, reticle.LatchObject.transform.position, d) + offset;
            Vector3 vel    = Vector3.zero;
            Vector3 pos    = line.GetPosition(i - 1);

            line.positionCount = i + 1;
            line.SetPosition(i, pos);

            while (Vector3.Distance(line.GetPosition(i), end) > 0.1f)
            {
                pos = Vector3.SmoothDamp(pos, end, ref vel, lineInterpolation, Mathf.Infinity, Time.unscaledDeltaTime);
                line.SetPosition(i, pos);
                fireParticles.transform.position = pos;
                yield return null;
            }
        }

        line.SetPosition(totalTicks - 1, reticle.LatchObject.transform.position);
        fireParticles.transform.position = reticle.LatchObject.transform.position;
    }

    public void InterpolateEnd()
    {
        if (line.positionCount < totalTicks - 1) return;

        line.positionCount = totalTicks;
        line.SetPosition(totalTicks - 1, reticle.LatchObject.transform.position);
        //fireParticles.transform.position = reticle.LatchObject.transform.position;
    }
}
