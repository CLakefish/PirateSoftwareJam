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
    [SerializeField] private Transform endPos;
    [SerializeField] private DialogueScriptableObject endDialogue;
    [SerializeField] private GameObject fire;
    [SerializeField] private float transitionSpeed;
    [SerializeField] private float returnSpeed;
    [SerializeField] private float transportPause;

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

    private bool Homunculus {
        set {
            HomunculusController.gameObject.SetActive(value);
            PlatformerController.gameObject.SetActive(!value);
        }
    }

    public void SetPlayerPosition(Transform pos)
    {
        PlatformerController.transform.position = pos.position;
        PlatformerController.transform.forward = pos.forward;
    }

    public void Snap(Area area)
    {
        hand.HandAnim("Snap");
        PlatformerController.Camera.FOVPulse(snapFOVPulse);
        PlatformerController.Camera.Recoil(new Vector3(snapRecoil.x, snapRecoil.y, snapRecoil.z * Mathf.Sign(Random.Range(-1, 1))));
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

        yield return new WaitForSecondsRealtime(transportPause);

        while (Vector3.Distance(HomunculusController.Rigidbody.position, HomunculusController.LatchPos) > 0.1f) {
            yield return null;
        }
    }

    public void ToPlayer(Area area)
    {
        SetPlayerPosition(area.SpawnPosition);

        if (anim != null) StopCoroutine(anim);
        anim = StartCoroutine(ToPlayerTransition(area));
    }

    private IEnumerator ToPlayerTransition(Area area)
    {
        PlatformerController.Camera.LockCamera = true;
        PlatformerController.Camera.CamComponent.GetComponent<AudioListener>().enabled = false;

        PlatformerController.gameObject.SetActive(true);

        StartCoroutine(HandGrab());

        PlatformerController.enabled = true;

        Vector3 pos = area.EnemyController.transform.position + area.EnemyController.offset + (Vector3.up * heightOffset);
        Vector3 dir = HomunculusController.Camera.CamComponent.transform.forward * teleportDist;
        teleporter.transform.position = pos - dir;
        teleporter.transform.forward  = (teleporter.transform.position - HomunculusController.Rigidbody.position).normalized;
        teleporter.SetActive(true);

        yield return new WaitForSecondsRealtime(transportPause);

        while (Vector3.Distance(HomunculusController.Rigidbody.position, HomunculusController.LatchPos) > 0.1f) {
            teleporter.transform.forward = (teleporter.transform.position - HomunculusController.Rigidbody.position).normalized;
            if (Vector3.Distance(HomunculusController.Rigidbody.position, HomunculusController.LatchPos) <= 1) {
                area.SetEnemyRenderer(false);
            }

            yield return null;
        }

        Homunculus = false;
        teleporter.SetActive(false);

        platformerView.transform.localPosition = Vector3.zero;
        platformerView.transform.localScale    = Vector3.one;
        platformerView.SetAsLastSibling();

        HomunculusController.Rigidbody.position = HomunculusController.LatchPos;
        HomunculusController.Camera.SetForward(HomunculusController.Camera.ForwardNoY);
        HomunculusController.Line.SetActive(false);

        PlatformerController.Camera.LockCamera = false;
        PlatformerController.Camera.CamComponent.GetComponent<AudioListener>().enabled = true;
    }


    public void ToHomunculus(Area area)
    {
        if (anim != null) StopCoroutine(anim);
        anim = StartCoroutine(ToHomunculusTransition(area));
    }

    private IEnumerator ToHomunculusTransition(Area area)
    {
        platformerView.localScale = Vector3.zero;

        area.TurnOff();

        Homunculus = true;
        HomunculusController.Camera.CamComponent.fieldOfView = PlatformerController.Camera.CamComponent.fieldOfView;
        HomunculusController.Camera.SetForward(area.GetComponentInChildren<MindExitPortal>().Forward);
        HomunculusController.Exit(area.GetComponentInChildren<MindExitPortal>().Forward * 10);

        area.SetEnemyRenderer(true);

        if (LevelManager.Instance.CheckWin())
        {
            if (LevelManager.Instance.LastLevel)
            {
                anim = StartCoroutine(CompletedCutscene());
                yield break;
            }

            yield return new WaitForSecondsRealtime(snapPause);

            PlayerComplete.Activate();
        }
    }

    private IEnumerator CompletedCutscene()
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
    }
}
