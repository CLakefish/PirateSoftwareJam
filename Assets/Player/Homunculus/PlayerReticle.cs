using NUnit.Framework;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerReticle : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Rigidbody    Rigidbody;
    [SerializeField] private PlayerCamera cam;

    [Header("Collisions")]
    [SerializeField] private LayerMask latchableLayer;
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private float     latchDistance;
    [SerializeField] private float     maxDeviation = 0.1f;
    [SerializeField] private float     deviationIgnoreDist = 1.5f;

    [Header("Canvas/VFX")]
    [SerializeField] private CanvasScaler canvas;
    [SerializeField] private Transform reticleHolder;
    [SerializeField] public RectTransform reticle;
    [SerializeField] private float        reticleRotateSpeed;
    [SerializeField] private float        reticleInRangeSize;
    [SerializeField] private float        reticleOutsideRangeSize;
    [SerializeField] private Color        reticleHighlighted;
    [SerializeField] private Color        reticleObstructed;

    [Header("Reticle Pulse")]
    [SerializeField] private float reticlePulseSize;
    [SerializeField] private float reticlePulseAngle;
    [SerializeField] private float reticleSmoothing;
    [SerializeField] private float reticleScaleTimeMult = 4;
    [SerializeField] private Color reticlePulseColor;

    private readonly Dictionary<Renderer, RectTransform>  reticles   = new();
    private readonly Dictionary<RectTransform, Latchable> latchables = new();
    public (RectTransform reticle, Renderer obj) Closest { get; private set; }

    public GameObject LatchObject { get; private set; }
    public bool       CanLatch    { get; private set; }

    private float reticleTimeScale = 0;
    private Coroutine reticlePulse;

    private void Awake()
    {
        GetAllLatchables();
    }

    private void OnEnable()
    {
        if (reticles.Count <= 0) return;

        reticleTimeScale = 0;

        GetAllLatchables();
    }

    private void Update()
    {
        reticleTimeScale += Time.deltaTime * reticleScaleTimeMult;
    }

    public void PulseReticle(GameObject obj)
    {
        LatchObject = obj;
        if (reticlePulse != null) StopCoroutine(reticlePulse);
        reticlePulse = StartCoroutine(PulseReticleCoroutine(reticles[obj.GetComponent<Renderer>()].transform as RectTransform));
    }

    public void GetAllLatchables()
    {
        foreach (var reticle in reticles)
        {
            Destroy(reticle.Value.gameObject);
        }

        reticles.Clear();
        latchables.Clear();

        var activeLatchables = FindObjectsByType<Latchable>(FindObjectsSortMode.None);

        foreach (var latch in activeLatchables)
        {
            Renderer rend = latch.GetComponent<Renderer>();
            RectTransform rect = Instantiate(reticle, reticleHolder);
            rect.transform.localPosition = Vector3.zero;
            rect.gameObject.SetActive(false);
            reticles.Add(rend, rect);
            latchables.Add(rect, latch);
        }
    }

    public void ResetPulse()
    {
        if (reticlePulse != null) StopCoroutine(reticlePulse);
    }

    public void CheckReticle()
    {
        foreach (var pair in reticles)
        {
            RectTransform rect = pair.Value;

            if (pair.Key == null) continue;

            if (!IsVisible(pair.Key) || Obstructed(pair.Key) || Vector3.Distance(pair.Key.transform.position, Rigidbody.position) <= 1)
            {
                rect.gameObject.SetActive(false);
                continue;
            }

            bool inRange = InRange(pair.Key);

            rect.GetComponent<Image>().color = inRange ? reticleHighlighted : reticleObstructed;
            rect.transform.localScale = inRange ? Vector3.one * reticleInRangeSize : Vector3.one * reticleOutsideRangeSize;
            rect.transform.localScale *= Mathf.Min(1, reticleTimeScale);

            Vector3 pos = cam.CamComponent.WorldToViewportPoint(pair.Key.transform.position + latchables[rect].offset);
            rect.transform.localPosition = new(
                (pos.x * canvas.referenceResolution.x) - (canvas.referenceResolution.x * 0.5f),
                (pos.y * canvas.referenceResolution.y) - (canvas.referenceResolution.y * 0.5f)
            );

            rect.gameObject.SetActive(true);
            rect.transform.localEulerAngles += new Vector3(0, 0, reticleRotateSpeed) * Time.deltaTime;
        }

        GetClosestToCenter();

        if (Closest.obj == null) return;

        Closest.reticle.GetComponent<Image>().color = Color.yellow;
    }

    private bool Obstructed(Renderer renderer)
    {
        return Physics.Linecast(cam.CamComponent.transform.position, renderer.transform.position, groundLayer);
    }

    private bool InRange(Renderer renderer)
    {
        return Vector3.Distance(renderer.transform.position, Rigidbody.position) <= latchDistance;
    }

    private bool IsVisible(Renderer renderer)
    {
        Plane[] planes = GeometryUtility.CalculateFrustumPlanes(cam.CamComponent);
        return GeometryUtility.TestPlanesAABB(planes, renderer.bounds);
    }

    private void GetClosestToCenter()
    {
        (RectTransform reticle, Renderer obj) closest = new(null, null);
        float distance = Mathf.Infinity;

        foreach (var pair in reticles)
        {
            Vector3 scr = cam.CamComponent.WorldToViewportPoint(pair.Key.transform.position);
            Vector3 mid = new(0.5f, 0.5f, 0);
            scr.z = 0;
            float checkDist = Vector3.Distance(scr, mid);
            float worldDist = Vector3.Distance(pair.Key.transform.position, Rigidbody.position);

            if (checkDist > maxDeviation && worldDist > deviationIgnoreDist) continue;

            bool maxDistCheck = checkDist < distance && InRange(pair.Key) && IsVisible(pair.Key) && !Obstructed(pair.Key);
            bool minDistCheck = worldDist <= deviationIgnoreDist && !Obstructed(pair.Key);

            if (maxDistCheck || minDistCheck)
            {
                closest = new(pair.Value, pair.Key);
                distance = checkDist;
            }
        }

        CanLatch = closest.obj != null && closest.reticle != null;
        Closest = closest;
    }

    public void TurnOffAll()
    {
        ResetPulse();

        foreach (var r in reticles)
        {
            r.Value.gameObject.SetActive(false);
        }
    }

    private IEnumerator PulseReticleCoroutine(RectTransform rect)
    {
        Vector3 scaleVel = Vector3.zero;
        float angle    = reticlePulseAngle;
        float angleVel = 0;
        float time     = 0;

        Image image = rect.GetComponent<Image>();
        image.color = reticlePulseColor;

        rect.transform.localScale = Vector3.one * reticlePulseSize;

        TurnOffAll();
        rect.gameObject.SetActive(true);

        while (angle > Mathf.Epsilon)
        {
            if (LatchObject == null) yield break;

            Vector3 pos = cam.CamComponent.WorldToViewportPoint(LatchObject.transform.position + latchables[rect].offset);
            rect.transform.localPosition = new(
                (pos.x * canvas.referenceResolution.x) - (canvas.referenceResolution.x * 0.5f),
                (pos.y * canvas.referenceResolution.y) - (canvas.referenceResolution.y * 0.5f)
            );

            rect.transform.localScale        = Vector3.SmoothDamp(rect.transform.localScale, Vector3.zero, ref scaleVel, reticleSmoothing, Mathf.Infinity, Time.unscaledDeltaTime);
            rect.transform.localEulerAngles += new Vector3(0, 0, angle) * Time.unscaledDeltaTime;
            image.color = Color.Lerp(image.color, Color.white, time);

            angle = Mathf.SmoothDamp(angle, 0, ref angleVel, reticleSmoothing, Mathf.Infinity, Time.unscaledDeltaTime);
            time += Time.unscaledDeltaTime;
            yield return null;
        }

        rect.transform.localScale = Vector3.one;
        rect.gameObject.SetActive(false);
    }
}
