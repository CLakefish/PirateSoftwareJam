using HFSMFramework;
using UnityEngine;
using UnityEngine.UI;

public class HomunculusController : MonoBehaviour
{
    private class BeginState : State<HomunculusController>
    {
        public BeginState(HomunculusController context) : base(context)
        {
        }

        public override void FixedUpdate()
        {
            context.ApplyGravity();
        }

        public override void Exit()
        {
            context.rb.linearVelocity = (Vector3.up + context.cam.Cam.transform.forward).normalized * context.launchForce;
        }
    }

    private class LaunchState : State<HomunculusController>
    {
        public LaunchState(HomunculusController context) : base(context)
        {
        }

        public override void Enter()
        {
            
        }

        public override void Update()
        {
            if (context.Reticle(out RaycastHit hit))
            {
                if (context.inputs.Jump)
                {
                    context.latchObject = hit.collider.gameObject;
                    context.canLatch = true;
                    return;
                }
            }

            if (context.inputs.Jump || context.launchBuffer > 0)
            {
                context.launchBuffer = 0;

                if (context.hfsm.Duration <= context.latchLaunchGraceTime && context.hfsm.PreviousState == context.Latch)
                {
                    context.rb.linearVelocity = (Vector3.up + context.cam.Cam.transform.forward).normalized * context.launchForce;
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
            context.canLatch = false;
            context.rb.linearVelocity = Vector3.zero;
        }
    }

    private class LatchState : State<HomunculusController>
    {
        private Vector3 latchVel;
        private Vector3 movePos;
        private Vector3 camVel;

        public LatchState(HomunculusController context) : base(context)
        {
        }

        public override void Enter()
        {
            context.latchFinished     = false;
            context.rb.linearVelocity = Vector3.zero;
            movePos = context.rb.position;

            context.cam.LockCamera = true;
        }

        public override void Update()
        {
            if (context.inputs.Jump) {
                context.launchBuffer = context.launchBufferTime;
            }

            context.rb.MovePosition(movePos);
        }

        public override void FixedUpdate()
        {
            context.Reticle(out RaycastHit _);

            Vector3 pos = context.latchObject.transform.position + Vector3.up;
            movePos = Vector3.Lerp(context.rb.position, pos, context.latchLerp.Evaluate(context.hfsm.Duration));

            Vector3 fwd = context.cam.Cam.transform.forward;
            context.cam.Cam.transform.forward = Vector3.SmoothDamp(fwd, (pos - context.cam.Cam.transform.position).normalized, ref camVel, context.latchCamInterpolate);

            float dist = Vector3.Distance(context.rb.position, pos);
            context.latchFinished = dist <= 0.1f;
        }

        public override void Exit()
        {
            context.latchObject.GetComponent<EnemyController>().Trigger();

            context.reticle.gameObject.SetActive(false);

            context.rb.linearVelocity = new Vector3(context.rb.linearVelocity.x, context.exitLaunch, context.rb.linearVelocity.z);
            context.cam.LockCamera    = false;
        }
    }

    [Header("Inputs")]
    [SerializeField] private PlayerInputManager inputs;
    [SerializeField] private PlayerCamera       cam;

    [Header("Physics")]
    [SerializeField] private Rigidbody rb;
    [SerializeField] private LayerMask latchableLayer;
    [SerializeField] private float latchRadius;
    [SerializeField] private float latchDistance;

    [Header("Movement Parameters")]
    [SerializeField] private float gravity;
    [SerializeField] private float launchForce;
    [SerializeField] private float exitLaunch;
    [SerializeField] private AnimationCurve latchLerp;
    [SerializeField] private float latchCamInterpolate;
    [SerializeField] private float latchLaunchGraceTime;
    [SerializeField] private float launchBufferTime;
    [SerializeField] private float latchBufferTime;

    [Header("Canvas")]
    [SerializeField] private CanvasScaler canvas;
    [SerializeField] private RectTransform reticle;

    private BeginState  Begin  { get; set; }
    private LaunchState Launch { get; set; }
    private LatchState  Latch  { get; set; }

    private StateMachine<HomunculusController> hfsm { get; set; }

    private GameObject latchObject;
    private bool latchFinished;
    private bool canLatch;

    private float latchBuffer;
    private float launchBuffer;

    private void OnEnable()
    {
        hfsm   = new(this);
        Begin  = new BeginState (this);
        Launch = new LaunchState(this);
        Latch  = new LatchState (this);

        hfsm.AddTransitions(new() { 
            new(Begin, Launch, () => inputs.Jump),
            new(Launch, Latch, () => canLatch),
            new(Latch, Launch, () => latchFinished),
        });

        hfsm.SetStartState(Begin);
    }

    private void Update()
    {
        hfsm.CheckTransitions();
        hfsm.Update();

        if (Input.GetKeyDown(KeyCode.Q))
        {
            Application.targetFrameRate = Application.targetFrameRate == 15 ? 1000 : 15;
        }

        launchBuffer -= Time.deltaTime;
        latchBuffer  -= Time.deltaTime;
    }

    private void FixedUpdate()
    {
        hfsm.FixedUpdate();
    }

    private void OnGUI()
    {
        hfsm.OnGUI();
    }

    private void ApplyGravity() => rb.linearVelocity -= gravity * Time.fixedDeltaTime * Vector3.up;

    private bool Reticle(out RaycastHit hit)
    {
        if (Physics.SphereCast(cam.Cam.transform.position, latchRadius, cam.Cam.transform.forward, out hit, latchDistance, latchableLayer))
        {
            Vector3 pos = cam.Cam.WorldToViewportPoint(hit.collider.transform.position);
            reticle.transform.localPosition = new Vector3(pos.x * 1920 - (1920 / 2.0f), pos.y * 1080 - (1080 / 2.0f));

            reticle.gameObject.SetActive(true);

            return true;
        }

        reticle.gameObject.SetActive(false);
        return false;
    }
}
