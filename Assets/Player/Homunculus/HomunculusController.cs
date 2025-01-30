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
            context.reticle.Reticle();
        }

        public override void FixedUpdate()
        {
            context.ApplyGravity();
        }

        public override void Exit()
        {
            TimeManager.Instance.SetScale(1);

            context.started = true;
            context.rb.linearVelocity = (Vector3.up + context.cam.CamComponent.transform.forward).normalized * context.launchForce;
            context.cam.FOVPulse(context.jumpPulseFOV);
        }
    }

    private class LaunchState : State<HomunculusController>
    {
        private bool hasLaunched = false;

        public LaunchState(HomunculusController context) : base(context) { }

        public override void Enter()
        {
            hasLaunched = false;
        }

        public override void Update()
        {
            context.reticle.Reticle();

            Debug.Log(hasLaunched);

            if ((context.PlayerInputs.Jump || context.launchBuffer > 0) && !hasLaunched)
            {
                hasLaunched = true;
                context.launchBuffer = 0;

                context.cam.FOVPulse(context.jumpPulseFOV);
                Vector3 dir = (Vector3.up + context.cam.CamComponent.transform.forward).normalized * context.launchForce;
                context.rb.linearVelocity = dir;
            }

            if (context.PlayerInputs.Jump)
            {
                Renderer closest = context.reticle.GetClosestToCenter().obj;

                if (closest != null)
                {
                    context.reticle.Set(closest.gameObject);
                    context.canLatch = true;
                    return;
                }
            }
        }

        public override void FixedUpdate()
        {
            context.ApplyGravity();
        }
    }

    private class LatchState : State<HomunculusController>
    {
        private Vector3 latchVel;

        private Vector3 camVel;
        private Vector3 posVel;

        private Vector3 movePos;
        private Vector3 offset;

        private bool slowTime;

        public LatchState(HomunculusController context) : base(context) { }

        public override void Enter()
        {
            context.canLatch = context.latchFinished = false;

            var latchable = context.LatchObject.GetComponent<Latchable>();
            latchable.Latch(context);
            offset = latchable.offset;
            context.LatchPos = context.LatchObject.transform.position + offset;

            latchVel = context.rb.linearVelocity;
            movePos  = context.rb.position;

            context.rb.linearVelocity = Vector3.zero;

            context.cam.FOVPulse(context.pulseFOV);
            slowTime = latchable.slowTime;

            context.cam.Recoil(context.recoil);

            TimeManager.Instance.SetScale(1);

            Vector3 startPosition = context.cam.CamComponent.transform.position - Vector3.up + context.cam.CamComponent.transform.right;

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
            context.line.SetPosition(1, Vector3.SmoothDamp(context.line.GetPosition(1), context.LatchObject.transform.position + offset, ref posVel, context.lineInterpolation));
            context.fireParticles.transform.position = context.line.GetPosition(1);
        }

        public override void FixedUpdate()
        {
            Vector3 pos = context.LatchObject.transform.position + offset;
            movePos     = Vector3.Lerp(context.rb.position, pos, context.latchLerp.Evaluate(context.hfsm.Duration));

            context.latchFinished = Vector3.Distance(context.rb.position, pos) < 0.01f;

            if (slowTime)
            {
                Vector3 fwd = context.cam.CamComponent.transform.forward;
                Vector3 dir = (pos - context.cam.CamComponent.transform.position).normalized;
                context.cam.CamComponent.transform.forward = new Vector3(
                    Mathf.SmoothDampAngle(fwd.x, dir.x, ref camVel.x, context.latchCamInterpolate),
                    Mathf.SmoothDampAngle(fwd.y, dir.y, ref camVel.y, context.latchCamInterpolate),
                    Mathf.SmoothDampAngle(fwd.z, dir.z, ref camVel.z, context.latchCamInterpolate));
            }
        }

        public override void Exit()
        {
            context.Rebound(slowTime);
            context.reticle.ResetReticle();

            if (!slowTime)
            {
                context.rb.linearVelocity += context.cam.ForwardNoY * latchVel.magnitude;
            }
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
    [SerializeField] private float deathBounce;
    [SerializeField] private float bounceTimeSlow;

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

    public Vector3 LatchPos { get; private set; }

    private BeginState  Begin  { get; set; }
    private LaunchState Launch { get; set; }
    private LatchState  Latch  { get; set; }
    private StateMachine<HomunculusController> hfsm { get; set; }

    public bool Latching {
        get {
            return hfsm.CurrentState == Latch;
        } 
    }

    private float launchBuffer = 0;
    private float deathCounter = 0;
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

        launchBuffer -= Time.deltaTime;
    }

    private void FixedUpdate()
    {
        if (started && hfsm.Duration > 0.1f && hfsm.CurrentState != Latch) {

            switch (deathCounter)
            {
                case 2:
                    rb.linearVelocity = Vector3.zero;
                    PlayerRespawn.Respawn();
                    return;

                default:
                    if (Physics.SphereCast(rb.transform.position, groundCheckRadius, Vector3.down, out RaycastHit _, groundCheckDistance, groundLayer))
                    {
                        deathCounter += 1;
                        rb.linearVelocity = new Vector3(rb.linearVelocity.x, deathBounce, rb.linearVelocity.z);
                        TimeManager.Instance.SetScale(bounceTimeSlow);
                    }
                    break;
            }
        }

        hfsm.FixedUpdate();
    }

    private void OnGUI()
    {
        // hfsm.OnGUI();
    }

    private void ApplyGravity() => rb.linearVelocity -= gravity * Time.fixedDeltaTime * Vector3.up;

    public void Rebound(bool timeSet = true)
    {
        cam.LockCamera = false;
        latchFinished  = true;

        cam.FOVPulse(jumpPulseFOV);

        line.gameObject.SetActive(false);

        if (timeSet)
        {
            TimeManager.Instance.Interpolate = true;
            TimeManager.Instance.SetScale(latchLaunchTimeSlow);
        }

        rb.linearVelocity = new Vector3(rb.linearVelocity.x, exitLaunch, rb.linearVelocity.z);
    }
}
