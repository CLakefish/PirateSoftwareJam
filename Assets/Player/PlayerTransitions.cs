using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class PlayerTransitions : PlayerManager.PlayerController
{
    [Header("Transitions")]
    [SerializeField] private CanvasScaler canvas;
    [SerializeField] private Canvas worldSpaceCanvas;
    [SerializeField] private RectTransform homunculusView;
    [SerializeField] private RectTransform platformerView;

    [Header("Fire Pulse")]
    [SerializeField] private Material fireVignette;
    [SerializeField] private float fireVigentteStandard;
    [SerializeField] private float fireVignetteInterpolate;
    [SerializeField] private float firePulseEnter;
    [SerializeField] private float firePulseExit;
    [SerializeField] private float firePulseSnap;

    [Header("Portal")]
    [SerializeField] private GameObject teleporter;
    [SerializeField] private float teleportDist = 0.2f;
    [SerializeField] private float heightOffset = 0.25f;

    [Header("Hand")]
    [SerializeField] private PlatformerAnimator hand;
    [SerializeField] private Vector3 snapRecoil;
    [SerializeField] private float snapPause;
    [SerializeField] private float snapFOVPulse;

    private Coroutine anim;
    private Coroutine grab;

    private const string fireVignetteName = "_Intensity";
    private float fireVignetteVelocity;

    #region Transitions

    private bool Homunculus {
        set {
            HomunculusController.gameObject.SetActive(value);
            PlatformerController.gameObject.SetActive(!value);

            PlayerLatching.SetReticle(value ? HomunculusController.Reticle : PlatformerController.Reticle);
        }
    }

    public void ToPlayer(Area area)
    {
        player.SetPlayerPosition(area.SpawnPosition);

        if (anim != null) StopCoroutine(anim);
        anim = StartCoroutine(ToPlayerTransition(area));
    }

    public void ToHomunculus(Area area)
    {
        if (anim != null) StopCoroutine(anim);
        anim = StartCoroutine(ToHomunculusTransition(area));
    }

    private IEnumerator ToPlayerTransition(Area area)
    {
        PlatformerController.Camera.LockCamera = true;
        PlatformerController.Camera.CamComponent.GetComponent<AudioListener>().enabled = false;
        PlatformerController.Camera.SetForward(Vector3.forward);

        PlatformerController.Camera.CamComponent.enabled = false;
        PlatformerController.Reticle.enabled = false;

        PlatformerController.Reticle.TurnOffAll();
        PlatformerController.gameObject.SetActive(true);

        StartCoroutine(HandGrab());

        Vector3 pos = area.EnemyController.transform.position + area.EnemyController.offset + (Vector3.up * heightOffset);
        Vector3 dir = HomunculusController.Camera.CamComponent.transform.forward * teleportDist;

        PulseFire(firePulseEnter);

        teleporter.transform.position = pos - dir;
        teleporter.transform.forward = (teleporter.transform.position - HomunculusController.Rigidbody.position).normalized;
        teleporter.SetActive(true);

        while (Vector3.Distance(HomunculusController.Rigidbody.position, HomunculusController.LatchPos) > 0.1f)
        {
            PlatformerController.Camera.CamComponent.fieldOfView = HomunculusController.Camera.CamComponent.fieldOfView;
            PlatformerController.Camera.CamComponent.Render();

            if (Vector3.Distance(HomunculusController.Rigidbody.position, HomunculusController.LatchPos) <= 1.5f)
            {
                area.SetEnemyRenderer(false);
                PlatformerController.PlatformerController.enabled = true;
                PlatformerController.Reticle.enabled              = true;
            }

            yield return new WaitForEndOfFrame();
        }

        Homunculus = false;

        teleporter.SetActive(false);
        PlayerLatching.ResetLine();

        PlatformerController.Camera.CamComponent.fieldOfView = HomunculusController.Camera.CamComponent.fieldOfView;
        PlatformerController.Camera.CamComponent.enabled     = true;
        HomunculusController.Rigidbody.position = HomunculusController.LatchPos;

        platformerView.transform.localPosition = Vector3.zero;
        platformerView.transform.localScale    = Vector3.one;
        platformerView.SetAsLastSibling();

        PlatformerController.Camera.LockCamera = false;
        PlatformerController.Camera.CamComponent.GetComponent<AudioListener>().enabled = true;
    }
    private IEnumerator ToHomunculusTransition(Area area)
    {
        platformerView.localScale = Vector3.zero;
        area.TurnOff();

        PulseFire(firePulseExit);

        Vector3 fwd = area.GetComponentInChildren<MindExitPortal>().Forward;
        Vector3 vel = PlatformerController.Rigidbody.linearVelocity;
        vel.y = 0;

        Homunculus = true;
        HomunculusController.Camera.CamComponent.fieldOfView = PlatformerController.Camera.CamComponent.fieldOfView;
        HomunculusController.Camera.SetForward(fwd);
        HomunculusController.Exit(fwd * 10);

        yield return new WaitForSecondsRealtime(0.1f);

        area.SetEnemyRenderer(true);

        if (LevelManager.Instance.CheckWin())
        {
/*            if (LevelManager.Instance.LastLevel)
            {
                //anim = StartCoroutine(CompletedCutscene());
                yield break;
            }*/

            yield return new WaitForSecondsRealtime(snapPause);

            PlayerComplete.Activate();
        }
    }

    #endregion

    #region VFX

    #region Hands

    public void Snap()
    {
        hand.HandAnim("Snap");
        PlatformerController.Camera.FOVPulse(snapFOVPulse);
        PlatformerController.Camera.Recoil(new Vector3(snapRecoil.x, snapRecoil.y, snapRecoil.z * Mathf.Sign(Random.Range(-1, 1))));
        PulseFire(firePulseSnap);
    }

    public void Grab()
    {
        if (grab != null) StopCoroutine(grab);
        grab = StartCoroutine(HandGrab());
    }

    private IEnumerator HandGrab()
    {
        hand.gameObject.SetActive(true);
        hand.HandAnim("Grab");

        while (Vector3.Distance(HomunculusController.Rigidbody.position, HomunculusController.LatchPos) > 0.1f)
        {
            yield return null;
        }
    }

    #endregion

    public void PulseFire(float val)
    {
        fireVignette.SetFloat(fireVignetteName, val);
    }

    #endregion


    private void Update()
    {
        float val = Mathf.SmoothDamp(fireVignette.GetFloat(fireVignetteName), fireVigentteStandard, ref fireVignetteVelocity, fireVignetteInterpolate, Mathf.Infinity, Time.unscaledDeltaTime);

        fireVignette.SetFloat(fireVignetteName, val);
    }

    private void LateUpdate()
    {
        worldSpaceCanvas.worldCamera = Camera.main;
    }

    /*   [SerializeField] private AnimationCurve transitionCurve;
 [SerializeField] private AnimationCurve pullOutCurve;
 [SerializeField] private Transform endPos;
 [SerializeField] private DialogueScriptableObject endDialogue;
 [SerializeField] private GameObject fire;*/
    /*    [SerializeField] private float transitionSpeed;
        [SerializeField] private float returnSpeed;*/
    /*    private IEnumerator CompletedCutscene()
        {
            Vector3 vel    = Vector3.zero;
            HomunculusController.enabled = false;
            HomunculusController.Rigidbody.linearVelocity = Vector3.zero;

            DialogueManager.Instance.DisplayDialogue(endDialogue);

            var fires = GameObject.FindGameObjectsWithTag("Lightable");

            foreach (var f in fires)
            {
                GameObject obj = Instantiate(fire, f.transform);
                obj.transform.localScale *= 2;
                obj.transform.localPosition = Vector3.zero;
            }

            while (Vector3.Distance(HomunculusController.Rigidbody.position, endPos.position) > 1.0f)
            {
                if (PlayerInputs.Jump) yield break;

                HomunculusController.Rigidbody.MovePosition(Vector3.SmoothDamp(HomunculusController.Rigidbody.position, endPos.position, ref vel, 3, Mathf.Infinity, Time.unscaledDeltaTime));
                yield return null;
            }

            PlayerComplete.Activate();
        }*/
}
