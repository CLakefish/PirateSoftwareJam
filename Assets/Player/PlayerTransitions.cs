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
    [SerializeField] private float transitionSpeed;
    [SerializeField] private float transportPause;
    [SerializeField] private float snapPause;

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

    public void ToHomunculus()
    {
        if (anim != null) StopCoroutine(anim);
        anim = StartCoroutine(ToHomunculusTransition());

        LevelManager.Instance.CheckWin();
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

    private IEnumerator ToHomunculusTransition()
    {
        homunculusView.localScale = Vector3.zero;
        homunculusView.SetAsLastSibling();

        PlatformerController.Animator.HandAnim("Snap");

        yield return new WaitForSecondsRealtime(snapPause);

        Vector3 scaleVel = Vector3.zero;

        while (Vector3.Distance(homunculusView.localScale, Vector3.one) > 0.01f)
        {
            homunculusView.localScale = Vector3.SmoothDamp(homunculusView.localScale, Vector3.one, ref scaleVel, transitionSpeed, Mathf.Infinity, Time.unscaledDeltaTime);
            yield return null;
        }

        homunculusView.localScale = Vector3.one;
        Homunculus = true;
        HomunculusController.Rebound();
    }
}
