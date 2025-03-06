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
    [SerializeField] private float firePulseWin;

    [Header("Portal")]
    [SerializeField] private GameObject teleporter;
    [SerializeField] private float teleportDist = 0.2f;
    [SerializeField] private float heightOffset = 0.25f;

    [Header("Hand")]
    [SerializeField] private Vector3 snapRecoil;
    [SerializeField] private float snapPause;
    [SerializeField] private float snapFOVPulse;

    private Coroutine anim;
    private Coroutine grab;

    private GameObject course;

    private const string fireVignetteName = "_Intensity";
    private const string decorName = "HomunculusPlatforming";
    private const float enemyVisiblePause = 0.1f;
    private float fireVignetteVelocity;

    #region Transitions

    private void Awake()
    {
        course = GameObject.FindGameObjectWithTag(decorName);
    }

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
        PlatformerController.Camera.SetForward(area.SpawnPosition.forward);
        PlatformerController.SetActive(false);
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
            if (Vector3.Distance(HomunculusController.Rigidbody.position, HomunculusController.LatchPos) <= 1.5f)
            {
                area.SetEnemyRenderer(false);
                PlatformerController.enabled = true;
                PlatformerController.Reticle.enabled = true;
            }

            yield return new WaitForEndOfFrame();
        }

        Homunculus = false;

        teleporter.SetActive(false);
        PlayerLatching.ResetLine();
        PlayerLatching.ClearParticles();

        HomunculusController.Rigidbody.position = HomunculusController.LatchPos;

        course.SetActive(false);

        platformerView.transform.localPosition = Vector3.zero;
        platformerView.transform.localScale    = Vector3.one;
        platformerView.SetAsLastSibling();

        PlatformerController.SetActive(true);
        PlatformerController.Camera.CamComponent.fieldOfView = HomunculusController.Camera.CamComponent.fieldOfView;
        PlatformerController.Reticle.GetAllLatchables();
        PlatformerController.gameObject.SetActive(true);
    }

    private IEnumerator ToHomunculusTransition(Area area)
    {
        platformerView.localScale = Vector3.zero;
        area.TurnOff();

        PulseFire(firePulseExit);

        Vector3 fwd = area.Exit.Forward;
        Vector3 vel = PlatformerController.Rigidbody.linearVelocity;
        vel.y = 0;

        Homunculus = true;
        HomunculusController.Camera.CamComponent.fieldOfView = PlatformerController.Camera.CamComponent.fieldOfView;
        HomunculusController.Camera.SetForward(fwd);
        HomunculusController.Exit(fwd * Mathf.Max(HomunculusController.exitLaunchForce, vel.magnitude * 0.5f));

        if (LevelManager.Instance.CheckWin())
        {
            PulseFire(firePulseWin);

            if (LevelManager.Instance.LastLevel)
            {
                anim = StartCoroutine(CompletedCutscene());
                yield break;
            }

            yield return new WaitForSecondsRealtime(snapPause);

            HomunculusController.Camera.enabled = false;
            PlayerComplete.Activate();
            yield break;
        }

        yield return new WaitForSecondsRealtime(enemyVisiblePause);

        area.SetEnemyRenderer(true);
    }

    #endregion

    #region VFX

    #region Hands

    public void Snap()
    {
        PlatformerHand.HandAnim("Snap");
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
        PlatformerHand.gameObject.SetActive(true);
        PlatformerHand.HandAnim("Grab");

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

    [Header("Dw bout this")]
    [SerializeField] private Transform endPos;
    [SerializeField] private DialogueScriptableObject endDialogue;
    [SerializeField] private GameObject fire;
    [SerializeField] private float returnSpeed;

    private IEnumerator CompletedCutscene()
    {
        Vector3 vel = Vector3.zero;
        HomunculusController.enabled = false;
        HomunculusController.Rigidbody.linearVelocity = Vector3.zero;

        var fires = GameObject.FindGameObjectsWithTag("Lightable");

        foreach (var f in fires)
        {
            GameObject obj = Instantiate(fire, f.transform);
            obj.transform.localScale *= 5;
            obj.transform.localPosition = Vector3.zero;
            obj.transform.localRotation = Quaternion.identity;
        }

        yield return new WaitForEndOfFrame();
        DialogueManager.Instance.DisplayDialogue(endDialogue);

        while (Vector3.Distance(HomunculusController.Rigidbody.position, endPos.position) > 10)
        {
            if (PlayerInputs.Jump)
            {
                PlayerComplete.Activate();
                yield break;
            }

            HomunculusController.Rigidbody.MovePosition(Vector3.SmoothDamp(HomunculusController.Rigidbody.position, endPos.position, ref vel, returnSpeed, Mathf.Infinity, Time.unscaledDeltaTime));
            yield return null;
        }

        PlayerComplete.Activate();
    }
}
