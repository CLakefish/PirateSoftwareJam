using HFSMFramework;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;

public class HomunculusController : PlayerManager.PlayerController
{
    private class BeginState : State<HomunculusController> {
        public BeginState(HomunculusController context) : base(context) { }

        public override void Update() {
            context.reticle.CheckReticle();
        }

        public override void FixedUpdate() {
            context.ApplyGravity();
        }

        public override void Exit() {
            TimeManager.Instance.SetScale(1);

            AudioManager.Instance.PlaySFX(context.lunge);

            context.started = true;
            context.rb.linearVelocity = (Vector3.up + context.cam.CamComponent.transform.forward).normalized * context.beginLaunchForce;
            context.cam.FOVPulse(context.jumpPulseFOV);
        }
    }

    private class LaunchState : State<HomunculusController> {
        private bool hasLaunched = false;

        public LaunchState(HomunculusController context) : base(context) { }

        public override void Enter() {
            hasLaunched = false;
        }

        public override void Update() {
            context.reticle.CheckReticle();

            if (context.PlayerInputs.Jump) {

                if (!hasLaunched && context.hfsm.Duration > 0.01f)
                {
                    hasLaunched = true;

                    context.cam.FOVPulse(context.jumpPulseFOV);

                    AudioManager.Instance.PlaySFX(context.lunge);

                    Vector3 dir = (Vector3.up + context.cam.CamComponent.transform.forward).normalized * context.launchForce;
                    dir.y       = Mathf.Max(dir.y, context.rb.linearVelocity.y + dir.y);
                    context.rb.linearVelocity = dir;
                }

                Renderer closest = context.reticle.Closest.obj;

                if (closest != null)
                {
                    context.canLatch = true;
                    return;
                }
            }
        }

        public override void FixedUpdate() {
            context.ApplyGravity();
        }
    }

    private class LatchState : State<HomunculusController> {
        private Vector3 camVel;
        private Vector3 startVel;
        private Vector3 movePos;
        private Vector3 offset;

        private bool slowTime;
        public LatchState(HomunculusController context) : base(context) { }

        private void GetLatchable() {
            var latchable = context.reticle.Closest.obj.GetComponent<Latchable>();
            slowTime = latchable.slowTime;
            offset = latchable.offset;
            latchable.Latch();

            context.LatchPos = latchable.transform.position + offset;
        }

        public override void Enter() {
            context.canLatch = context.latchFinished = false;

            startVel = context.rb.linearVelocity;
            movePos  = context.rb.position;
            context.rb.linearVelocity = Vector3.zero;

            GetLatchable();

            TimeManager.Instance.SetScale(1);
            AudioManager.Instance.PlaySFX(context.latch);

            context.cam.LockCamera = slowTime;

            context.cam.Recoil(context.recoil);
            context.cam.FOVPulse(context.pulseFOV);

            context.PlayerTransitions.PulseFire(context.latchFirePulse);
            context.reticle.PulseReticle(context.reticle.Closest.obj.gameObject);
            context.PlayerLatching.InitLine(context.cam);
        }

        public override void Update() {
            context.PlayerLatching.InterpolateLine();
            context.rb.MovePosition(movePos);
        }

        public override void FixedUpdate() {
            Vector3 pos = context.reticle.Closest.obj.transform.position + offset;
            movePos     = Vector3.Lerp(context.rb.position, pos, context.PlayerLatching.latchLerp.Evaluate(context.hfsm.Duration));

            context.latchFinished = Vector3.Distance(context.rb.position, pos) < 0.01f;

            if (slowTime && context.hfsm.Duration > 1f)
            {
                Vector3 fwd = context.cam.CamComponent.transform.forward;
                Vector3 dir = (pos - context.cam.CamComponent.transform.position).normalized;
                context.cam.CamComponent.transform.forward = new Vector3(
                    Mathf.SmoothDampAngle(fwd.x, dir.x, ref camVel.x, context.PlayerLatching.latchCamInterpolate),
                    Mathf.SmoothDampAngle(fwd.y, dir.y, ref camVel.y, context.PlayerLatching.latchCamInterpolate),
                    Mathf.SmoothDampAngle(fwd.z, dir.z, ref camVel.z, context.PlayerLatching.latchCamInterpolate));
            }
        }

        public override void Exit() {
            context.PlayerLatching.SetActive(false);

            context.latchFinished  = true;
            context.cam.LockCamera = false;

            context.reticle.ResetPulse();

            Vector3 dir = context.cam.ForwardNoY * Mathf.Max(context.launchForce, new Vector2(startVel.x, startVel.z).magnitude);

            context.Exit(dir, slowTime);
        }
    }

    [Header("References")]
    [SerializeField] private PlayerCamera   cam;
    [SerializeField] private PlayerReticle  reticle;

    [Header("Physics")]
    [SerializeField] private Rigidbody rb;
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private float groundCheckRadius;
    [SerializeField] private float groundCheckDistance;

    [Header("Movement Parameters")]
    [SerializeField] private float gravity;
    [SerializeField] private float launchForce;
    [SerializeField] private float beginLaunchForce;
    [SerializeField] private float deathBounceForce;
    [SerializeField] public float  exitLaunchForce;
    [SerializeField] private int   deathBounceTally = 3;

    [Header("VFX")]
    [SerializeField] private float jumpPulseFOV;
    [SerializeField] private float bounceTimeSlow;
    [SerializeField] private float latchFirePulse;

    [Header("Latch VFX")]
    [SerializeField] private float pulseFOV;
    [SerializeField] private Vector3 recoil;

    [Header("Audio")]
    [SerializeField] private AudioClip latch;
    [SerializeField] private AudioClip lunge;
    [SerializeField] private AudioClip hitGround;
    [SerializeField] private AudioClip die;

    public Rigidbody      Rigidbody   => rb;
    public PlayerCamera   Camera      => cam;
    public PlayerReticle  Reticle     => reticle;

    private BeginState  Begin  { get; set; }
    private LaunchState Launch { get; set; }
    private LatchState  Latch  { get; set; }
    private StateMachine<HomunculusController> hfsm { get; set; }

    public Vector3 LatchPos { get; private set; }

    public bool Latching {
        get {
            return hfsm.CurrentState == Latch;
        }
    }

    private float deathCounter = 0;
    private bool latchFinished;
    private bool canLatch;
    private bool started = false;

    private void OnEnable()
    {
        hfsm   = new(this);
        Begin  = new(this);
        Launch = new(this);
        Latch  = new(this);

        hfsm.AddTransitions(new() {
            new(Begin,  Launch, () => PlayerInputs.Jump),
            new(Launch, Latch,  () => canLatch),
            new(Latch,  Launch, () => latchFinished),
        });

        hfsm.AddOnChange(new() {
            () => deathCounter = 0,
        });

        hfsm.SetStartState(Begin);
    }

    private void Update()
    {
        hfsm.CheckTransitions();
        hfsm.Update();
    }

    private void FixedUpdate()
    {
        if (started && hfsm.Duration > 0.1f && hfsm.CurrentState != Latch) {
            if (deathCounter >= deathBounceTally)
            {
                AudioManager.Instance.PlaySFX(die);
                rb.linearVelocity = Vector3.zero;
                PlayerRespawn.Respawn();
                return;
            }

            if (Physics.SphereCast(rb.transform.position, groundCheckRadius, Vector3.down, out RaycastHit _, groundCheckDistance, groundLayer))
            {
                deathCounter += 1;
                rb.linearVelocity = new Vector3(rb.linearVelocity.x, deathBounceForce, rb.linearVelocity.z);
                TimeManager.Instance.SetScale(bounceTimeSlow);
                AudioManager.Instance.PlaySFX(hitGround);
            }
        }

        hfsm.FixedUpdate();
    }

    private void OnGUI()
    {
        hfsm.OnGUI();
    }

    private void ApplyGravity() => rb.linearVelocity -= gravity * Time.fixedDeltaTime * Vector3.up;
    public void Exit(Vector3 dir, bool timeSet = true)
    {
        cam.LockCamera = false;

        if (timeSet)
        {
            TimeManager.Instance.Interpolate = true;
            TimeManager.Instance.SetScale(PlayerLatching.latchLaunchTimeSlow);
        }

        AudioManager.Instance.PlaySFX(lunge);
        rb.linearVelocity = new Vector3(dir.x, PlayerLatching.exitLaunch, dir.z);
    }
}