using HFSMFramework;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;

public class HomunculusController : PlayerManager.PlayerController
{
    private class BeginState : State<HomunculusController>
    {
        public BeginState(HomunculusController context) : base(context) { }

        public override void Update()
        {
            if (context.hfsm.Duration > 0.01f) context.Reticle(out RaycastHit _);
        }

        public override void FixedUpdate()
        {
            context.ApplyGravity();
        }

        public override void Exit()
        {
            context.started = true;
            context.rb.linearVelocity = (Vector3.up + context.cam.CamComponent.transform.forward).normalized * context.launchForce;
            context.cam.FOVPulse(context.jumpPulseFOV);
        }
    }

    private class LaunchState : State<HomunculusController>
    {
        public LaunchState(HomunculusController context) : base(context) { }
        public override void Update()
        {
            if (context.Reticle(out RaycastHit hit))
            {
                if (context.PlayerInputs.Jump)
                {
                    context.LatchObject = hit.collider.gameObject;
                    context.canLatch = true;
                    return;
                }
            }
            else
            {
                context.reticle.gameObject.SetActive(false);
            }

            if (context.PlayerInputs.Jump || context.launchBuffer > 0)
            {
                context.launchBuffer = 0;

                if (context.hfsm.Duration <= context.latchLaunchGraceTime && context.hfsm.PreviousState == context.Latch)
                {
                    context.cam.FOVPulse(context.jumpPulseFOV);
                    context.rb.linearVelocity = (Vector3.up + context.cam.CamComponent.transform.forward).normalized * context.launchForce;
                    return;
                }
            }
        }

        public override void FixedUpdate()
        {
            context.ApplyGravity();
        }

        public override void Exit()
        {
            context.cam.Recoil(context.recoil);

            context.canLatch = false;
            context.rb.linearVelocity = Vector3.zero;
        }
    }

    private class LatchState : State<HomunculusController>
    {
        private Vector3 latchVel;
        private Vector3 movePos;
        private Vector3 camVel;

        private Vector3 startPosition;
        private Vector3 endPosition;
        private Vector3 posVel;

        public LatchState(HomunculusController context) : base(context) { }

        public override void Enter()
        {
            context.LatchObject.GetComponent<EnemyController>().Trigger();

            context.latchFinished     = false;
            context.rb.linearVelocity = Vector3.zero;
            movePos = context.rb.position;

            context.cam.FOVPulse(context.pulseFOV);
            context.cam.LockCamera = true;
            context.ReticlePulse();

            startPosition = context.cam.CamComponent.transform.position - Vector3.up + context.cam.CamComponent.transform.right;
            endPosition   = context.LatchObject.transform.position;

            context.line.positionCount = 2;
            context.line.SetPosition(0, startPosition);
            context.line.SetPosition(1, startPosition);

            context.line.gameObject.SetActive(true);
            context.fireParticles.transform.position = startPosition;
            context.fireParticles.Clear();
        }

        public override void Update()
        {
            if (context.PlayerInputs.Jump) {
                context.launchBuffer = context.launchBufferTime;
            }

            context.rb.MovePosition(movePos);
            context.line.SetPosition(1, Vector3.SmoothDamp(context.line.GetPosition(1), endPosition, ref posVel, context.lineInterpolation));
            context.fireParticles.transform.position = context.line.GetPosition(1);
        }

        public override void FixedUpdate()
        {
            context.cam.FOVPulse(context.latchPassiveFOV);

            Vector3 pos = context.LatchObject.transform.position;
            movePos = Vector3.Lerp(context.rb.position, pos, context.latchLerp.Evaluate(context.hfsm.Duration));

            Vector3 fwd = context.cam.CamComponent.transform.forward;
            context.cam.CamComponent.transform.forward = Vector3.SmoothDamp(fwd, (pos - context.cam.CamComponent.transform.position).normalized, ref camVel, context.latchCamInterpolate);

            float dist = Vector3.Distance(context.rb.position, pos);
            context.latchFinished = dist < 0.01f;
        }

        public override void Exit()
        {
            if (!context.LatchObject.GetComponent<EnemyController>().Trigger())
            {
                context.Rebound();
            }
        }
    }

    [Header("Inputs")]
    [SerializeField] private PlayerCamera cam;

    [Header("Physics")]
    [SerializeField] private Rigidbody rb;
    [SerializeField] private LayerMask latchableLayer;
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private float latchRadius;
    [SerializeField] private float latchDistance;
    [SerializeField] private float groundCheckRadius;
    [SerializeField] private float groundCheckDistance;

    [Header("Movement Parameters")]
    [SerializeField] private float gravity;
    [SerializeField] private float launchForce;
    [SerializeField] private float jumpPulseFOV;
    [SerializeField] private float deathTime;
    [SerializeField] private float drag;

    [Header("Latching")]
    [SerializeField] private float exitLaunch;
    [SerializeField] private AnimationCurve latchLerp;
    [SerializeField] private float latchCamInterpolate;
    [SerializeField] private float latchLaunchGraceTime;
    [SerializeField] private float launchBufferTime;
    [SerializeField] private float latchBufferTime;
    [SerializeField] private Vector3 recoil;

    [Header("Canvas/VFX")]
    [SerializeField] private CanvasScaler canvas;
    [SerializeField] public RectTransform reticle;
    [SerializeField] private float reticleRotateSpeed;

    [Header("Reticle Pulse")]
    [SerializeField] private float reticlePulseSize;
    [SerializeField] private float reticlePulseAngle;
    [SerializeField] private float reticleSmoothing;
    [SerializeField] private Color reticlePulseColor;

    [Header("Latch VFX")]
    [SerializeField] private LineRenderer line;
    [SerializeField] private ParticleSystem fireParticles;
    [SerializeField] private float lineInterpolation;
    [SerializeField] private float pulseFOV;
    [SerializeField] private float latchPassiveFOV;

    public Rigidbody Rigidbody => rb;
    public PlayerCamera Camera => cam;
    private float deathCounter = 0;


    private BeginState  Begin  { get; set; }
    private LaunchState Launch { get; set; }
    private LatchState  Latch  { get; set; }
    private StateMachine<HomunculusController> hfsm { get; set; }

    public GameObject LatchObject { get; private set; }

    private float launchBuffer;
    private bool  latchFinished;
    private bool  canLatch;
    private bool  started = false;

    private void OnEnable()
    {
        hfsm   = new(this);
        Begin  = new BeginState (this);
        Launch = new LaunchState(this);
        Latch  = new LatchState (this);

        hfsm.AddTransitions(new() { 
            new(Begin,  Launch, () => PlayerInputs.Jump),
            new(Launch, Latch,  () => canLatch),
            new(Latch,  Launch, () => latchFinished),
        });

        hfsm.SetStartState(Begin);
    }

    private void Update()
    {
        hfsm.CheckTransitions();
        hfsm.Update();

        if (Input.GetKeyDown(KeyCode.Q)) {
            Application.targetFrameRate = Application.targetFrameRate == 15 ? 1000 : 15;
        }

        launchBuffer -= Time.deltaTime;
    }

    private void FixedUpdate()
    {
        if (started && hfsm.Duration > 0.1f && hfsm.CurrentState != Latch) {
            if (Physics.SphereCast(rb.transform.position, groundCheckRadius, Vector3.down, out RaycastHit _, groundCheckDistance, groundLayer)) {
                deathCounter += Time.deltaTime;
                rb.linearVelocity = Vector3.MoveTowards(rb.linearVelocity, Vector3.zero, Time.deltaTime * drag);
            }
            else {
                deathCounter = 0;
            }
        }

        if (deathCounter >= deathTime) PlayerRespawn.Respawn();

        hfsm.FixedUpdate();
    }

    public void Rebound()
    {
        cam.LockCamera = false;

        cam.FOVPulse(jumpPulseFOV);

        reticle.gameObject.SetActive(false);
        line.gameObject.SetActive(false);

        TimeManager.Instance.SetScale(latchLaunchGraceTime);

        rb.linearVelocity = new Vector3(rb.linearVelocity.x, exitLaunch, rb.linearVelocity.z);
    }

    private void ApplyGravity() => rb.linearVelocity -= gravity * Time.fixedDeltaTime * Vector3.up;

    private bool Reticle(out RaycastHit hit)
    {
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
                if (Physics.Linecast(cam.CamComponent.transform.position, h.collider.transform.position, groundLayer)) {
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
        }
    }

    private void ReticlePulse()
    {
        StartCoroutine(ReticlePulseCoroutine());
    }

    private IEnumerator ReticlePulseCoroutine()
    {
        Image image = reticle.GetComponent<Image>();

        Vector3 scaleVel = Vector3.zero;
        float angle    = reticlePulseAngle;
        float angleVel = 0;
        float time     = 0;

        image.color = reticlePulseColor;
        reticle.transform.localScale = Vector3.one * reticlePulseSize;

        while (angle > Mathf.Epsilon)
        {
            reticle.gameObject.SetActive(true);

            Vector3 pos = cam.CamComponent.WorldToViewportPoint(LatchObject.transform.position);
            reticle.transform.localPosition = new(
                (pos.x * canvas.referenceResolution.x) - (canvas.referenceResolution.x * 0.5f),
                (pos.y * canvas.referenceResolution.y) - (canvas.referenceResolution.y * 0.5f)
            );

            reticle.transform.localScale        = Vector3.SmoothDamp(reticle.transform.localScale, Vector3.zero, ref scaleVel, reticleSmoothing, Mathf.Infinity, Time.unscaledDeltaTime);
            reticle.transform.localEulerAngles += new Vector3(0, 0, angle) * Time.unscaledDeltaTime;
            image.color = Color.Lerp(image.color, Color.white, time);

            angle = Mathf.SmoothDamp(angle, 0, ref angleVel, reticleSmoothing);
            time += Time.unscaledDeltaTime;
            yield return null;
        }

        reticle.gameObject.SetActive(false);
        reticle.transform.localScale = Vector3.one;
    }
}
