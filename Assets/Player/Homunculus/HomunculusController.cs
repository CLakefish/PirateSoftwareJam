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

        public override void FixedUpdate()
        {
            context.ApplyGravity();
            context.reticle.Reticle();
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
        bool hasLaunched = false;

        public LaunchState(HomunculusController context) : base(context) { }

        public override void Enter()
        {
            hasLaunched = false;
        }

        public override void Update()
        {
            GameObject closest = context.reticle.GetClosestToCenter();

            if (closest != null) {
                if (context.PlayerInputs.Jump) {
                    context.reticle.Set(closest);
                    context.canLatch = true;
                    return;
                }
            }

            if ((context.PlayerInputs.Jump || context.launchBuffer > 0) && !hasLaunched) {
                hasLaunched          = true;
                context.launchBuffer = 0;

                if (context.hfsm.Duration <= context.latchLaunchGraceTime && context.hfsm.PreviousState == context.Latch) {
                    context.cam.FOVPulse(context.jumpPulseFOV);
                    context.rb.linearVelocity = (Vector3.up + context.cam.CamComponent.transform.forward).normalized * context.launchForce;
                    return;
                }
            }
        }

        public override void FixedUpdate()
        {
            context.reticle.Reticle();
            context.ApplyGravity();
        }

        public override void Exit()
        {
            context.cam.Recoil(context.recoil);
            TimeManager.Instance.SetScale(1);
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
            context.canLatch = false;
            context.rb.linearVelocity = Vector3.zero;

            context.LatchObject.GetComponent<Latchable>().Latch(context);

            context.latchFinished     = false;
            context.rb.linearVelocity = Vector3.zero;
            movePos = context.rb.position;

            context.cam.FOVPulse(context.pulseFOV);
            context.cam.LockCamera = true;

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
            Vector3 pos = context.LatchObject.transform.position;
            movePos = Vector3.Lerp(context.rb.position, pos, context.latchLerp.Evaluate(context.hfsm.Duration));

            Vector3 fwd = context.cam.CamComponent.transform.forward;
            Vector3 dir = (pos - context.cam.CamComponent.transform.position).normalized;
            context.cam.CamComponent.transform.forward = new Vector3(
                Mathf.SmoothDampAngle(fwd.x, dir.x, ref camVel.x, context.latchCamInterpolate),
                Mathf.SmoothDampAngle(fwd.y, dir.y, ref camVel.y, context.latchCamInterpolate),
                Mathf.SmoothDampAngle(fwd.z, dir.z, ref camVel.z, context.latchCamInterpolate));

            float dist = Vector3.Distance(context.rb.position, pos);
            context.latchFinished = dist < 0.01f;
        }

        public override void Exit()
        {
            context.Rebound();
        }
    }

    [Header("Inputs")]
    [SerializeField] private PlayerCamera cam;

    [Header("Physics")]
    [SerializeField] private Rigidbody rb;
    [SerializeField] private HomunculusReticle reticle;
    [SerializeField] private LayerMask groundLayer;
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
    [SerializeField] private float latchLaunchTimeSlow;
    [SerializeField] private Vector3 recoil;

    [Header("Latch VFX")]
    [SerializeField] private LineRenderer line;
    [SerializeField] private ParticleSystem fireParticles;
    [SerializeField] private float lineInterpolation;
    [SerializeField] private float pulseFOV;

    public Rigidbody Rigidbody => rb;
    public PlayerCamera Camera => cam;
    public LayerMask GroundLayer => groundLayer;
    public GameObject LatchObject => reticle.LatchObject;

    private float deathCounter = 0;


    private BeginState  Begin  { get; set; }
    private LaunchState Launch { get; set; }
    private LatchState  Latch  { get; set; }
    private StateMachine<HomunculusController> hfsm { get; set; }

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

    private void OnGUI()
    {
        hfsm.OnGUI();
    }

    private void ApplyGravity() => rb.linearVelocity -= gravity * Time.fixedDeltaTime * Vector3.up;

    public void Rebound()
    {
        cam.LockCamera = false;
        latchFinished  = true;

        cam.FOVPulse(jumpPulseFOV);

        line.gameObject.SetActive(false);

        TimeManager.Instance.Interpolate = true;
        TimeManager.Instance.SetScale(latchLaunchTimeSlow);

        rb.linearVelocity = new Vector3(rb.linearVelocity.x, exitLaunch, rb.linearVelocity.z);
    }
}
