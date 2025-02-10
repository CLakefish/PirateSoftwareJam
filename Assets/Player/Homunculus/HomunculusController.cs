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
            context.rb.linearVelocity = (Vector3.up + context.cam.CamComponent.transform.forward).normalized * context.launchForce;
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

            if (context.PlayerInputs.Jump && !hasLaunched) {
                hasLaunched = true;

                context.cam.FOVPulse(context.jumpPulseFOV);

                AudioManager.Instance.PlaySFX(context.lunge);

                Vector3 dir               = (Vector3.up + context.cam.CamComponent.transform.forward).normalized * context.launchForce;
                context.rb.linearVelocity = dir;
            }

            if (context.PlayerInputs.Jump) {
                Renderer closest = context.reticle.Closest.obj;

                if (closest != null) {
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

            context.rb.linearVelocity = Vector3.zero;
            movePos                   = context.rb.position;

            context.reticle.Set(context.reticle.Closest.obj.gameObject);
            GetLatchable();

            context.cam.LockCamera = true;
            context.cam.Recoil(context.line.recoil);
            context.cam.FOVPulse(context.pulseFOV);

            context.line.InitLine(context.cam);

            TimeManager.Instance.SetScale(1);
            AudioManager.Instance.PlaySFX(context.latch);
        }

        public override void Update() {
            context.line.InterpolateLine();
            context.rb.MovePosition(movePos);
        }

        public override void FixedUpdate() {
            Vector3 pos = context.reticle.Closest.obj.transform.position + offset;
            movePos     = Vector3.Lerp(context.rb.position, pos, context.line.latchLerp.Evaluate(context.hfsm.Duration));

            context.latchFinished = Vector3.Distance(context.rb.position, pos) < 0.01f;

            if (slowTime) {
                Vector3 fwd = context.cam.CamComponent.transform.forward;
                Vector3 dir = (pos - context.cam.CamComponent.transform.position).normalized;
                context.cam.CamComponent.transform.forward = new Vector3(
                    Mathf.SmoothDampAngle(fwd.x, dir.x, ref camVel.x, context.line.latchCamInterpolate),
                    Mathf.SmoothDampAngle(fwd.y, dir.y, ref camVel.y, context.line.latchCamInterpolate),
                    Mathf.SmoothDampAngle(fwd.z, dir.z, ref camVel.z, context.line.latchCamInterpolate));
            }
        }

        public override void Exit() {
            context.line.SetActive(false);

            context.latchFinished  = true;
            context.cam.LockCamera = false;
            context.deathCounter   = 0;

            context.reticle.ResetPulse();
            context.Exit(slowTime ? context.cam.ForwardNoY * context.launchForce : Vector3.zero, slowTime);
        }
    }

    [Header("References")]
    [SerializeField] private PlayerCamera   cam;
    [SerializeField] private PlayerReticle  reticle;
    [SerializeField] private PlayerLatching line;

    [Header("Physics")]
    [SerializeField] private Rigidbody rb;
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private float groundCheckRadius;
    [SerializeField] private float groundCheckDistance;

    [Header("Movement Parameters")]
    [SerializeField] private float gravity;
    [SerializeField] private float launchForce;
    [SerializeField] private float jumpPulseFOV;
    [SerializeField] private float deathBounce;
    [SerializeField] private float bounceTimeSlow;

    [Header("Latch VFX")]
    [SerializeField] private float pulseFOV;

    [Header("Audio")]
    [SerializeField] private AudioClip latch;
    [SerializeField] private AudioClip lunge;
    [SerializeField] private AudioClip hitGround;
    [SerializeField] private AudioClip die;

    public Rigidbody      Rigidbody   => rb;
    public PlayerCamera   Camera      => cam;
    public PlayerLatching Line        => line;

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
            switch (deathCounter) {
                case 2:
                    AudioManager.Instance.PlaySFX(die);
                    rb.linearVelocity = Vector3.zero;
                    PlayerRespawn.Respawn();
                    return;

                default:
                    if (Physics.SphereCast(rb.transform.position, groundCheckRadius, Vector3.down, out RaycastHit _, groundCheckDistance, groundLayer)) {
                        deathCounter += 1;
                        rb.linearVelocity = new Vector3(rb.linearVelocity.x, deathBounce, rb.linearVelocity.z);
                        TimeManager.Instance.SetScale(bounceTimeSlow);
                        AudioManager.Instance.PlaySFX(hitGround);
                    }
                    break;
            }
        }

        hfsm.FixedUpdate();
    }

/*    private void OnGUI()
    {
        hfsm.OnGUI();
    }
*/
    private void ApplyGravity() => rb.linearVelocity -= gravity * Time.fixedDeltaTime * Vector3.up;
    public void Exit(Vector3 dir, bool timeSet = true)
    {
        cam.LockCamera = false;

        if (timeSet)
        {
            TimeManager.Instance.Interpolate = true;
            TimeManager.Instance.SetScale(line.latchLaunchTimeSlow);
        }

        AudioManager.Instance.PlaySFX(lunge);
        rb.linearVelocity = new Vector3(dir.x, line.exitLaunch, dir.z);
    }
}