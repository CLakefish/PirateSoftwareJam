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

    [Header("Hand")]
    [SerializeField] private PlatformerAnimator hand;
    [SerializeField] private Vector3 snapRecoil;
    [SerializeField] private float snapPause;
    [SerializeField] private float snapFOVPulse;

    private GameObject homunculusPlatforming;
    private Coroutine anim;

    public bool Homunculus {
        set {
            HomunculusController.gameObject.SetActive(value);
            PlatformerController.gameObject.SetActive(!value);
        }
    }

    private void Start()
    {
        homunculusPlatforming = GameObject.FindGameObjectWithTag("HomunculusPlatforming");
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

    public void IdleSnap()
    {
        StartCoroutine(Snap(true));
    }

    private IEnumerator Snap(bool turnOff = false)
    {
        hand.gameObject.SetActive(true);
        hand.HandAnim("Grab");

        yield return new WaitForSecondsRealtime(transportPause);

        while (Vector3.Distance(HomunculusController.Rigidbody.position, HomunculusController.LatchObject.transform.position) > 0.1f) {
            yield return null;
        }

        if (turnOff) {
            hand.gameObject.SetActive(false);
        }
    }

    private IEnumerator ToPlayerTransition()
    {
        PlatformerController.Camera.LockCamera = true;
        PlatformerController.Camera.CamComponent.GetComponent<AudioListener>().enabled = false;

        PlatformerController.gameObject.SetActive(true);

        StartCoroutine(Snap());

        Vector3 vpPos = HomunculusController.Camera.CamComponent.WorldToViewportPoint(HomunculusController.LatchObject.transform.position);
        Vector2 targetCanvasPos = new(
            (vpPos.x * canvas.referenceResolution.x) - (canvas.referenceResolution.x * 0.5f),
            (vpPos.y * canvas.referenceResolution.y) - (canvas.referenceResolution.y * 0.5f)
        );

        platformerView.localScale    = Vector3.zero;
        platformerView.localPosition = targetCanvasPos;
        platformerView.SetAsLastSibling();

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
            platformerView.localPosition = Vector3.Lerp(targetCanvasPos, Vector3.zero, e);

            yield return null;
        }

        if (homunculusPlatforming != null) homunculusPlatforming.SetActive(false);

        platformerView.transform.localScale    = Vector3.one;
        platformerView.transform.localPosition = Vector3.zero;

        Homunculus = false;

        PlatformerController.Camera.LockCamera = false;
        PlatformerController.Camera.CamComponent.GetComponent<AudioListener>().enabled = true;
    }

    private IEnumerator ToHomunculusTransition(Area area)
    {
        hand.HandAnim("Snap");
        PlatformerController.Camera.FOVPulse(snapFOVPulse);
        PlatformerController.Camera.Recoil(new Vector3(snapRecoil.x, snapRecoil.y, snapRecoil.z * Mathf.Sign(Random.Range(-1, 1))));
        PlatformerController.Rigidbody.linearVelocity = Vector3.zero;
        PlatformerController.enabled = false;

        area.EnemyController.OnExit();

        yield return new WaitForSecondsRealtime(snapPause);

        hand.gameObject.SetActive(false);

        Vector3 posVel = Vector3.zero;

        while (Vector3.Distance(PlatformerController.Rigidbody.position, area.SpawnPosition.position) > 0.1f)
        {
            Vector3 newPos = Vector3.SmoothDamp(PlatformerController.Rigidbody.position, area.SpawnPosition.position, ref posVel, returnSpeed, Mathf.Infinity, Time.unscaledDeltaTime);
            PlatformerController.Rigidbody.MovePosition(newPos);
            yield return null;
        }
    }

    private IEnumerator PullOut(Area area)
    {
        yield return new WaitForSecondsRealtime(snapPause);

        platformerView.localScale = Vector3.one;
        platformerView.SetAsLastSibling();

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

        if (homunculusPlatforming != null) homunculusPlatforming.SetActive(true);
        area.TurnOff();

        platformerView.localScale = Vector3.zero;
        Homunculus = true;
        HomunculusController.Rebound();

        if (LevelManager.Instance.CheckWin())
        {
            yield return new WaitForSecondsRealtime(snapPause);

            PlayerComplete.Activate();
        }
    }
}
