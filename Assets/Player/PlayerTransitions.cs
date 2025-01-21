using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class PlayerTransitions : PlayerManager.PlayerController
{
    [Header("Transitions")]
    [SerializeField] private CanvasScaler canvas;
    [SerializeField] private RectTransform homunculusView;
    [SerializeField] private RectTransform platformerView;
    [SerializeField] private AnimationCurve transitionCurve;
    [SerializeField] private AnimationCurve pullOutCurve;
    [SerializeField] private float transitionSpeed;
    [SerializeField] private float returnSpeed;
    [SerializeField] private float transportPause;
    [SerializeField] private float snapPause;
    [SerializeField] private float snapFOVPulse;

    private Coroutine anim;

    public bool Homunculus {
        set {
            HomunculusController.gameObject.SetActive(value);
            PlatformerController.gameObject.SetActive(!value);
        }
    }

    public void SetPlayer(Transform pos)
    {
        PlatformerController.transform.position = pos.position;
        PlatformerController.transform.forward = pos.forward;
    }

    public void ToPlayer(Area area)
    {
        SetPlayer(area.SpawnPosition);

        if (anim != null) StopCoroutine(anim);
        anim = StartCoroutine(ToPlayerTransition());
    }

    public void ToHomunculus(Area area)
    {
        if (anim != null) StopCoroutine(anim);
        anim = StartCoroutine(ToHomunculusTransition(area));
        StartCoroutine(PullOut(area));
    }

    private IEnumerator ToPlayerTransition()
    {
        PlatformerController.Camera.LockCamera = true;
        PlatformerController.Camera.CamComponent.GetComponent<AudioListener>().enabled = false;

        Vector3 vpPos = HomunculusController.Camera.CamComponent.WorldToViewportPoint(HomunculusController.LatchObject.transform.position);
        Vector2 targetCanvasPos = new(
            (vpPos.x * canvas.referenceResolution.x) - (canvas.referenceResolution.x * 0.5f),
            (vpPos.y * canvas.referenceResolution.y) - (canvas.referenceResolution.y * 0.5f)
        );

        platformerView.localScale    = Vector3.zero;
        platformerView.localPosition = targetCanvasPos;
        platformerView.SetAsLastSibling();

        PlatformerController.gameObject.SetActive(true);
        PlatformerController.Animator.HandAnim("Grab");
        PlatformerController.enabled = true;

        yield return new WaitForSecondsRealtime(transportPause);

        float startDist = Vector3.Distance(HomunculusController.Rigidbody.position, HomunculusController.LatchObject.transform.position);

        while (Vector3.Distance(HomunculusController.Rigidbody.position, HomunculusController.LatchObject.transform.position) > 0.01f)
        {
            float t = 1.0f - (Vector3.Distance(HomunculusController.Rigidbody.position, HomunculusController.LatchObject.transform.position) / startDist);
            float e = transitionCurve.Evaluate(t);

            vpPos = HomunculusController.Camera.CamComponent.WorldToViewportPoint(HomunculusController.LatchObject.transform.position);
            targetCanvasPos = new(
                (vpPos.x * canvas.referenceResolution.x) - (canvas.referenceResolution.x * 0.5f),
                (vpPos.y * canvas.referenceResolution.y) - (canvas.referenceResolution.y * 0.5f)
            );

            platformerView.localScale    = Vector3.Lerp(Vector3.zero, Vector3.one, e);
            platformerView.localPosition = Vector3.Lerp(targetCanvasPos, Vector3.zero, t);

            yield return null;
        }

        Homunculus = false;

        PlatformerController.Camera.LockCamera = false;
        PlatformerController.Camera.CamComponent.GetComponent<AudioListener>().enabled = true;

        platformerView.localPosition = Vector3.zero;
        platformerView.localScale    = Vector3.one;
    }

    private IEnumerator ToHomunculusTransition(Area area)
    {
        PlatformerController.Animator.HandAnim("Snap");
        PlatformerController.Camera.FOVPulse(snapFOVPulse);
        PlatformerController.enabled = false;

        yield return new WaitForSecondsRealtime(snapPause);

        Transform pos = PlatformerController.Rigidbody.transform;
        Vector3 posVel = Vector3.zero;

        while (Vector3.Distance(pos.position, area.SpawnPosition.position) > 0.1f)
        {
            pos.position = Vector3.SmoothDamp(pos.position, area.SpawnPosition.position, ref posVel, returnSpeed, Mathf.Infinity, Time.unscaledDeltaTime);
            yield return null;
        }

        if (LevelManager.Instance.CheckWin())
        {
            PlayerComplete.Activate();
        }
    }

    private IEnumerator PullOut(Area area)
    {
        yield return new WaitForSecondsRealtime(snapPause);

        platformerView.localScale = Vector3.one;
        platformerView.SetAsLastSibling();

        HomunculusController.Rigidbody.transform.position = HomunculusController.LatchObject.transform.position;
        HomunculusController.Camera.CamComponent.Render();

        float startDist = Vector3.Distance(PlatformerController.Rigidbody.transform.position, area.SpawnPosition.position);

        while (Vector3.Distance(platformerView.localScale, Vector3.zero) > 0.01f && Vector3.Distance(PlatformerController.Rigidbody.transform.position, area.SpawnPosition.position) > 0.1f)
        {
            float t = 1.0f - (Vector3.Distance(PlatformerController.Rigidbody.transform.position, area.SpawnPosition.position) / startDist);
            float e = transitionCurve.Evaluate(t);

            Vector3 vpPos = HomunculusController.Camera.CamComponent.WorldToViewportPoint(HomunculusController.LatchObject.transform.position);
            Vector2 targetCanvasPos = new(
                (vpPos.x * canvas.referenceResolution.x) - (canvas.referenceResolution.x * 0.5f),
                (vpPos.y * canvas.referenceResolution.y) - (canvas.referenceResolution.y * 0.5f)
            );

            platformerView.localScale    = Vector3.Lerp(platformerView.localScale, Vector3.zero, e);
            platformerView.localPosition = Vector3.Lerp(Vector3.zero, targetCanvasPos, e);

            yield return null;
        }

        platformerView.localScale = Vector3.zero;
        Homunculus = true;
        HomunculusController.Rebound();
    }
}
