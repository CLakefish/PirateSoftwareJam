using NUnit.Framework;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HomunculusReticle : MonoBehaviour
{
    [SerializeField] private HomunculusController context;
    [SerializeField] private PlayerCamera cam;

    [Header("Canvas/VFX")]
    [SerializeField] private CanvasScaler canvas;
    [SerializeField] public RectTransform reticle;
    [SerializeField] private float reticleRotateSpeed;
    [SerializeField] private float reticleInRangeSize;
    [SerializeField] private Color reticleHighlighted;
    [SerializeField] private Color reticleObstructed;

    [Header("Collisions")]
    [SerializeField] private LayerMask latchableLayer;
    [SerializeField] private float latchDistance;

    [Header("Reticle Pulse")]
    [SerializeField] private float reticlePulseSize;
    [SerializeField] private float reticlePulseAngle;
    [SerializeField] private float reticleSmoothing;
    [SerializeField] private Color reticlePulseColor;

    private readonly Dictionary<Renderer, RectTransform> reticles = new();

    public GameObject LatchObject { get; private set; }
    public bool      CanLatch     { get; private set; }

    private void Awake()
    {
        var activeLatchables = FindObjectsByType<Latchable>(FindObjectsSortMode.None);

        foreach (var latch in activeLatchables)
        {
            Renderer      rend = latch.GetComponent<Renderer>();
            RectTransform rect = Instantiate(reticle, canvas.transform);
            rect.transform.localPosition = Vector3.zero;
            reticles.Add(rend, rect);
        }
    }

    public void Set(GameObject obj)
    {
        LatchObject = obj;
        StartCoroutine(ReticlePulseCoroutine(reticles[obj.GetComponent<Renderer>()].transform as RectTransform));
    }

    public void Reticle()
    {
        foreach (var pair in reticles)
        {
            RectTransform rect = pair.Value;

            if (!IsVisible(pair.Key) || Obstructed(pair.Key))
            {
                rect.gameObject.SetActive(false);
                continue;
            }

            bool inRange = InRange(pair.Key);

            rect.GetComponent<Image>().color = inRange ? reticleHighlighted : reticleObstructed;
            rect.transform.localScale = inRange ? Vector3.one * reticleInRangeSize : Vector3.one;

            Vector3 pos = cam.CamComponent.WorldToViewportPoint(pair.Key.transform.position);
            rect.transform.localPosition = new(
                (pos.x * canvas.referenceResolution.x) - (canvas.referenceResolution.x * 0.5f),
                (pos.y * canvas.referenceResolution.y) - (canvas.referenceResolution.y * 0.5f)
            );

            rect.gameObject.SetActive(true);
            rect.transform.localEulerAngles += new Vector3(0, 0, reticleRotateSpeed) * Time.deltaTime;
        }

        /*
        RaycastHit[] hits = Physics.SphereCastAll(cam.CamComponent.transform.position, latchRadius, cam.CamComponent.transform.forward, latchDistance, latchableLayer);

        if (hits.Length <= 0)
        {
            hit = default;
            reticle.gameObject.SetActive(false);
            return false;
        }
        else
        {
            hit = default;

            foreach (var h in hits)
            {
                if (Physics.Linecast(cam.CamComponent.transform.position, h.collider.transform.position, context.GroundLayer))
                {
                    continue;
                }

                reticle.transform.localScale = Vector3.one;
                reticle.gameObject.SetActive(true);

                Vector3 pos = cam.CamComponent.WorldToViewportPoint(h.collider.transform.position);
                reticle.transform.localPosition = new(
                    (pos.x * canvas.referenceResolution.x) - (canvas.referenceResolution.x * 0.5f),
                    (pos.y * canvas.referenceResolution.y) - (canvas.referenceResolution.y * 0.5f)
                );

                reticle.transform.localEulerAngles += new Vector3(0, 0, reticleRotateSpeed) * Time.deltaTime;

                hit = h;
                break;
            }

            return hit.collider != null;
        }*/
    }

    private bool Obstructed(Renderer renderer)
    {
        return Physics.Linecast(cam.CamComponent.transform.position, renderer.transform.position, context.GroundLayer);
    }

    private bool InRange(Renderer renderer)
    {
        return !(Vector3.Distance(renderer.transform.position, context.Rigidbody.position) >= latchDistance);
    }

    private bool IsVisible(Renderer renderer)
    {
        Plane[] planes = GeometryUtility.CalculateFrustumPlanes(cam.CamComponent);

        if (GeometryUtility.TestPlanesAABB(planes, renderer.bounds))
        {
            return true;
        }

        return false;
    }

    public (RectTransform reticle, Renderer obj) GetClosestToCenter()
    {
        (RectTransform reticle, Renderer obj) closest = new(null, null);
        float distance = Mathf.Infinity;

        foreach (var pair in reticles)
        {
            float checkDist = Vector2.Distance(pair.Value.localPosition, Vector2.zero);

            if (checkDist < distance && InRange(pair.Key) && IsVisible(pair.Key) && !Obstructed(pair.Key))
            {
                closest = new(pair.Value, pair.Key);
                distance = checkDist;
            }
        }

        CanLatch = closest.obj != null && closest.reticle != null;

        return closest;
    }

    private IEnumerator ReticlePulseCoroutine(RectTransform rect)
    {
        foreach (var r in reticles)
        {
            r.Value.gameObject.SetActive(false);
        }

        Image image = rect.GetComponent<Image>();
        image.color = reticlePulseColor;

        Vector3 scaleVel = Vector3.zero;
        float angle    = reticlePulseAngle;
        float angleVel = 0;
        float time     = 0;

        rect.gameObject.SetActive(true);
        rect.transform.localScale = Vector3.one * reticlePulseSize;

        while (angle > Mathf.Epsilon)
        {
            Vector3 pos = cam.CamComponent.WorldToViewportPoint(LatchObject.transform.position);
            rect.transform.localPosition = new(
                (pos.x * canvas.referenceResolution.x) - (canvas.referenceResolution.x * 0.5f),
                (pos.y * canvas.referenceResolution.y) - (canvas.referenceResolution.y * 0.5f)
            );

            rect.transform.localScale        = Vector3.SmoothDamp(rect.transform.localScale, Vector3.zero, ref scaleVel, reticleSmoothing, Mathf.Infinity, Time.unscaledDeltaTime);
            rect.transform.localEulerAngles += new Vector3(0, 0, angle) * Time.unscaledDeltaTime;
            image.color = Color.Lerp(image.color, Color.white, time);

            angle = Mathf.SmoothDamp(angle, 0, ref angleVel, reticleSmoothing);
            time += Time.unscaledDeltaTime;
            yield return null;
        }

        rect.gameObject.SetActive(false);
        rect.transform.localScale = Vector3.one;
    }
}
