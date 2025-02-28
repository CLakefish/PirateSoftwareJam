using HFSMFramework;
using UnityEngine;

public class PlatformerController : PlayerManager.PlayerController
{
    private class WalkingState : State<PlatformerController>
    {
        private float time = 0;

        public WalkingState(PlatformerController context) : base(context) { }

        public override void Enter()
        {
            AudioManager.Instance.PlaySFX(context.land);
            time = Time.time;
        }

        public override void Update()
        {
            if (Time.time > time + context.stepTime && context.PlayerInputs.IsInputting)
            {
                AudioManager.Instance.PlaySFX(context.step);
                time = Time.time;
            }
        }

        public override void FixedUpdate()
        {
            context.slideBoost = context.hfsm.Duration > 0.1f;

            context.rb.linearVelocity = context.HorizontalVelocity;
            context.Move(context.hfsm.Duration > 0.1f);
        }
    }

    private class JumpingState : State<PlatformerController>
    {
        public JumpingState(PlatformerController context) : base(context) { }

        public override void Enter()
        {
            AudioManager.Instance.PlaySFX(context.jump);

            context.jumpBuffer = 0;
            context.rb.linearVelocity = new Vector3(context.rb.linearVelocity.x, Mathf.Max(context.jumpHeight, context.rb.linearVelocity.y + context.jumpHeight), context.rb.linearVelocity.z);
        }

        public override void Update()
        {
            context.Move(true);

            if (context.PlayerInputs.Jump)
            {
                context.jumpBuffer = context.jumpBufferTime;
            }
        }

        public override void FixedUpdate()
        {
            context.Gravity();
        }

        public override void Exit()
        {
            context.DesiredHorizontalVelocity = context.HorizontalVelocity;
        }
    }

    private class FallingState : State<PlatformerController>
    {
        public FallingState(PlatformerController context) : base(context) { }

        public override void Update()
        {
            context.Move(true);

            if (context.PlayerInputs.Jump)
            {
                context.jumpBuffer = context.jumpBufferTime;
            }

            if (!context.slideBoost) context.slideBoost = context.hfsm.Duration >= context.slideReplenishTime;
        }

        public override void FixedUpdate()
        {
            context.Gravity();
        }

        public override void Exit()
        {
            context.ResetTilt();
        }
    }

    private class SlidingState : State<PlatformerController>
    {
        public SlidingState(PlatformerController context) : base(context) { }

        private Vector3 momentum;

        public override void Enter()
        {
            AudioManager.Instance.PlaySFX(context.slide);

            momentum = context.rb.linearVelocity;

            if (context.slideBoost && context.HorizontalVelocity.magnitude <= context.slideBoostSpeedCap)
            {
                if (context.PlayerInputs.IsInputting) context.cam.FOVPulse(context.slidePulse);

                Vector3 mov = context.MoveDir == Vector3.zero ? context.cam.ForwardNoY : context.MoveDir;
                Vector3 dir = Vector3.ProjectOnPlane(mov * context.slideForce, context.collisions.GroundNormal);
                momentum += dir;
            }

            context.rb.linearVelocity = momentum;
            context.DesiredHorizontalVelocity = new Vector2(momentum.x, momentum.z);
            context.slideBoost = false;
        }

        public override void Update()
        {
            if (context.PlayerInputs.Jump)
            {
                context.jumpBuffer = context.jumpBufferTime;
            }
        }

        public override void FixedUpdate()
        {
            context.SetSlideTilt();
            context.Gravity();

            Vector3 desiredVelocity;

            // Gaining momentum
            if (context.collisions.SlopeCollision)
            {
                // Get the gravity, angle, and current momentumDirection
                Vector3 slopeGravity  = Vector3.down * context.gravity;
                float normalizedAngle = Vector3.Angle(Vector3.up, context.collisions.GroundNormal) / 90.0f;
                Vector3 adjusted      = slopeGravity * (1.0f + normalizedAngle);
                desiredVelocity       = momentum + adjusted;
            }
            else if (context.collisions.GroundCollision && !context.collisions.SlopeCollision)
            {
                desiredVelocity = context.MoveDir;
            }
            else
            {
                desiredVelocity = momentum;
            }

            // Move towards gathered velocity (y is seperated so I can tinker with it more, it can be simplified ofc)
            float accel = context.collisions.GroundCollision && !context.collisions.SlopeCollision ? context.slideAcceleration : context.slideAirAcceleration;

            Vector3 slideInterpolated = Vector3.MoveTowards(momentum, desiredVelocity, Time.deltaTime * accel);

            momentum = context.collisions.SlopeCollision ? slideInterpolated.normalized * Mathf.Max(slideInterpolated.magnitude, context.rb.linearVelocity.magnitude + (slideInterpolated.magnitude * Time.deltaTime)) : slideInterpolated;

            if (context.MoveDir != Vector3.zero)
            {
                Vector3 desiredDirection = Vector3.ProjectOnPlane(context.MoveDir, context.collisions.GroundNormal).normalized;
                Vector3 slopeDirection   = Vector3.ProjectOnPlane(Vector3.down,    context.collisions.GroundNormal).normalized;

                if (Vector3.Dot(desiredDirection, slopeDirection) >= context.slideDotMin)
                {
                    Vector3 currentMomentum = context.collisions.SlopeCollision
                        ? Vector3.ProjectOnPlane(momentum, context.collisions.GroundNormal).normalized
                        : momentum;

                    Quaternion rotation     = Quaternion.FromToRotation(currentMomentum, desiredDirection);
                    Quaternion interpolated = Quaternion.Slerp(Quaternion.identity, rotation, Time.deltaTime * context.slideRotationSpeed);

                    momentum = interpolated * momentum;

                    if (Vector3.Dot(momentum, slopeDirection) <= 0 && context.collisions.SlopeCollision)
                    {
                        momentum = Vector3.ProjectOnPlane(momentum, context.collisions.GroundNormal);
                    }
                }
                else
                {
                    momentum = new Vector3(momentum.x, context.rb.linearVelocity.y, momentum.z);
                }
            }

            context.rb.linearVelocity = momentum;
        }

        public override void Exit()
        {
            context.ResetTilt();

            context.rb.linearVelocity = new Vector3(momentum.x, Mathf.Max(momentum.y, 0), momentum.z);
            context.DesiredHorizontalVelocity = context.HorizontalVelocity;
        }
    }

    private class SlideJumping : State<PlatformerController>
    {
        public SlideJumping(PlatformerController context) : base(context) { }

        public override void Enter()
        {
            AudioManager.Instance.PlaySFX(context.jump);

            context.collisions.ResetCollisions();

            context.cam.FOVPulse(context.slideJumpPulse);

            context.jumpBuffer = 0;

            context.rb.linearVelocity = new Vector3(context.rb.linearVelocity.x, Mathf.Max(context.slideJumpForce, context.rb.linearVelocity.y + context.slideJumpForce), context.rb.linearVelocity.z);
        }

        public override void Update()
        {
            if (context.PlayerInputs.Jump)
            {
                context.jumpBuffer = context.jumpBufferTime;
            }
        }

        public override void FixedUpdate()
        {
            context.SetSlideTilt();

            context.Move(true);
            context.Gravity();
        }

        public override void Exit()
        {
            context.ResetTilt();

            context.DesiredHorizontalVelocity = context.HorizontalVelocity;
        }
    }

    private class LatchingState : State<PlatformerController>
    {
        private Vector3 startVel;
        private Vector3 offset;

        public LatchingState(PlatformerController context) : base(context) { }

        public override void Enter()
        {
            context.slideBoost = true;
            context.latchFinished = false;

            startVel = context.rb.linearVelocity;

            var latchable = context.reticle.Closest.obj.GetComponent<Latchable>();
            latchable.Latch();

            context.reticle.PulseReticle(context.reticle.Closest.obj.gameObject);

            context.cam.Recoil(context.recoil);
            context.cam.FOVPulse(context.pulseFOV);

            context.PlayerLatching.InitLine(context.cam);
        }

        public override void Update()
        {
            float increasedVal = context.PlayerLatching.latchLerp.Evaluate(context.hfsm.Duration * Mathf.Max(context.latchCurveSpeedIncrease, context.HorizontalVelocity.magnitude * 0.5f) * Mathf.Max(context.hfsm.Duration, 1));

            Vector3 dir = (context.reticle.Closest.obj.transform.position - context.rb.position).normalized;

            context.rb.linearVelocity = context.latchVelocitySpeed * increasedVal * dir;
        }

        public override void FixedUpdate()
        {
            context.PlayerTransitions.PulseFire(context.latchFirePulse);

            Vector3 pos = context.reticle.Closest.obj.transform.position + offset;
            context.latchFinished = Vector3.Distance(context.rb.position, pos) < context.launchMinDist;
        }

        public override void Exit()
        {
            context.cam.FOVPulse(context.pulseFOV);

            context.reticle.ResetPulse();
            context.PlayerLatching.SetActive(false);

            Vector3 dir = context.cam.CamComponent.transform.forward * Mathf.Max(context.launchEndForce, startVel.magnitude);
            float yVel  = context.PlayerLatching.exitLaunch * Mathf.Sign(context.cam.CamComponent.transform.forward.y + context.latchDownwardLaunchThreshold);

            context.rb.linearVelocity         = dir + new Vector3(0, yVel, 0);
            context.DesiredHorizontalVelocity = context.HorizontalVelocity;
        }
    }

    private class LungingState : State<PlatformerController>
    {
        public LungingState(PlatformerController context) : base(context)
        {
        }

        public override void Enter()
        {
            AudioManager.Instance.PlaySFX(context.jump);

            context.slideBoost = true;

            context.jumpBuffer = 0;
            context.rb.linearVelocity = new Vector3(context.rb.linearVelocity.x, Mathf.Max(context.lungeForce, context.rb.linearVelocity.y + context.lungeForce), context.rb.linearVelocity.z);
        }

        public override void Update()
        {
            context.Move(true);

            if (context.PlayerInputs.Jump)
            {
                context.jumpBuffer = context.jumpBufferTime;
            }
        }

        public override void FixedUpdate()
        {
            context.Gravity();
        }
    }

    private class WallRunningState : State<PlatformerController>
    {
        public WallRunningState(PlatformerController context) : base(context)
        {
        }

        public override void Enter()
        {
            Vector3 dir = GetProjected() * Mathf.Max(context.HorizontalVelocity.magnitude, context.wallRunSpeed);
            dir.y = context.rb.linearVelocity.y > 0 ? context.rb.linearVelocity.y : context.rb.linearVelocity.y * context.wallRunEnterReduct;
            context.rb.linearVelocity = dir;

            context.SetWallTilt();
        }

        public override void Update()
        {
            base.Update();
        }

        public override void FixedUpdate()
        {
            if (context.rb.linearVelocity.y > 0)
            {
                context.Gravity();
            }
            else
            {
                context.rb.linearVelocity -= Time.deltaTime * context.wallRunGravity * Vector3.up;
            }

            if (context.HorizontalVelocity.magnitude <= context.wallRunSpeed)
            {
                Vector3 projected = GetProjected() * Mathf.Max(context.wallRunSpeed, context.HorizontalVelocity.magnitude);

                context.DesiredHorizontalVelocity = new Vector3(projected.x, 0, projected.z);
                context.rb.linearVelocity = new Vector3(context.DesiredHorizontalVelocity.x, context.rb.linearVelocity.y, context.DesiredHorizontalVelocity.z);
            }
        }

        public override void Exit()
        {
            context.ResetTilt();
            context.DesiredHorizontalVelocity = context.HorizontalVelocity;
        }

        Vector3 GetProjected()
        {
            Vector3 dir;

            if (context.HorizontalVelocity.magnitude <= Mathf.Epsilon) dir = context.Camera.ForwardNoY;
            else dir = context.HorizontalVelocity;

            Vector3 projected = Vector3.ProjectOnPlane(dir, context.collisions.WallNormal).normalized;
            return projected;
        }
    }

    private class WallJumpingState : State<PlatformerController>
    {
        public WallJumpingState(PlatformerController context) : base(context)
        {
        }

        public override void Enter()
        {
            context.slideBoost = true;

            context.Camera.FOVPulse(context.wallJumpFOVPulse);

            Vector3 dir = context.collisions.WallNormal * context.wallJumpForce + (Vector3.up * context.wallJumpHeight);
            context.rb.linearVelocity = new Vector3(context.rb.linearVelocity.x + dir.x, dir.y, context.rb.linearVelocity.z + dir.z);
        }

        public override void Update()
        {
            base.Update();
        }

        public override void FixedUpdate()
        {
            context.Gravity();
        }

        public override void Exit()
        {
            context.DesiredHorizontalVelocity = context.HorizontalVelocity;
        }
    }

    [Header("References")]
    [SerializeField] private PlayerCamera   cam;
    [SerializeField] private PlayerReticle  reticle;

    [Header("Physics")]
    [SerializeField] private PlatformerCollisions collisions;
    [SerializeField] private CapsuleCollider      capsuleCollider;
    [SerializeField] private Rigidbody            rb;

    [Header("Movement")]
    [SerializeField] private float moveSpeed;
    [SerializeField] private float acceleration;
    [SerializeField] private float airAcceleration;
    [SerializeField] private float gravity;

    [Header("Jumping")]
    [SerializeField] private float jumpHeight;
    [SerializeField] private float coyoteTime;
    [SerializeField] private float jumpBufferTime;

    [Header("Sliding Parameters")]
    [SerializeField] private float slideForce;
    [SerializeField] private float slideRotationSpeed;
    [SerializeField] private float slideAcceleration;
    [SerializeField] private float slideAirAcceleration;
    [SerializeField] private float slideReplenishTime;
    [SerializeField] private float slideBoostSpeedCap = 50;
    [SerializeField] private float slidePulse;
    [SerializeField] private float slideTilt;

    [Header("Slide Jump Parameters")]
    [SerializeField] private float slideJumpForce;
    [SerializeField] private float slideJumpPulse;

    [Header("Size Change Parameters")]
    [SerializeField] private float crouchSize;
    [SerializeField] private float standardSize;
    [SerializeField] private float crouchTime;

    [Header("Latch Parameters")]
    [SerializeField] private float latchVelocitySpeed = 45;
    [SerializeField] private float latchCurveSpeedIncrease = 2;
    [SerializeField] private float latchDownwardLaunchThreshold = 0.8f;
    [SerializeField] private float launchEndForce;
    [SerializeField] private float launchMinDist = 0.6f;
    [SerializeField] private float latchFirePulse;

    [Header("Latch VFX")]
    [SerializeField] private float pulseFOV;
    [SerializeField] private Vector3 recoil;

    [Header("Lunging")]
    [SerializeField] private float lungeForce;

    [Header("Wallrun Parameters")]
    [SerializeField] private float wallRunThreshold;
    [SerializeField] private float wallRunSpeed;
    [SerializeField] private float wallRunEnterReduct;
    [SerializeField] private float wallRunGravity;
    [SerializeField] private float wallRunStickForce;
    [SerializeField] private float wallRunTilt;

    [Header("Walljump Parameters")]
    [SerializeField] private float wallJumpTime;
    [SerializeField] private float wallJumpForce;
    [SerializeField] private float wallJumpHeight;
    [SerializeField] private float wallJumpFOVPulse;

    [Header("Launch")]
    [SerializeField] private float launchFOVPulse;

    [Header("SFX")]
    [SerializeField] private AudioClip land;
    [SerializeField] private AudioClip jump;
    [SerializeField] private AudioClip slide;
    [SerializeField] private AudioClip step;
    [SerializeField] private AudioClip die;
    [SerializeField] private float     stepTime;

    private readonly float slideDotMin   = -0.25f;
    private readonly float jumpGraceTime = 0.1f;

    public PlayerCamera  Camera    => cam;
    public PlayerReticle Reticle   => reticle;
    public Rigidbody     Rigidbody => rb;

    private WalkingState    Walking   { get; set; }
    private JumpingState    Jumping   { get; set; }
    private FallingState    Falling   { get; set; }
    private SlidingState    Sliding   { get; set; }
    private SlideJumping    SlideJump { get; set; }
    private LatchingState   Latching  { get; set; }
    private LungingState    Lunging   { get; set; }
    private WallRunningState WallRun  { get; set; }
    private WallJumpingState WallJump { get; set; }
    private StateMachine<PlatformerController> hfsm { get; set; }

    private Vector3 HorizontalVelocity {
        get {
            return new Vector3(rb.linearVelocity.x, 0, rb.linearVelocity.z);
        }
    }

    private Vector3 MoveDir {
        get {
            Vector3 dir = (cam.ForwardNoY * PlayerInputs.Input.normalized.y + cam.RightNoY * PlayerInputs.Input.normalized.x).normalized;
            return dir;
        }
    }

    private bool AllowWallRun
    {
        get
        {
            float angle        = Vector3.SignedAngle(collisions.WallNormal, MoveDir, Vector3.up);
            bool excludeStates = hfsm.CurrentState != WallJump && hfsm.CurrentState != Jumping && hfsm.CurrentState != Walking && hfsm.CurrentState != Latching;

            return Mathf.Abs(angle) > wallRunThreshold && excludeStates;
        }
    }

    private Vector3 DesiredHorizontalVelocity;
    private float jumpBuffer;
    private bool  slideBoost;
    private bool  latchFinished;

    private void OnEnable()
    {
        hfsm = new(this);
        Walking   = new(this);
        Jumping   = new(this);
        Falling   = new(this);
        Sliding   = new(this);
        SlideJump = new(this);
        Latching  = new(this);
        Lunging   = new(this);
        WallRun   = new(this);
        WallJump  = new(this);

        hfsm.AddTransitions(new()
        {
            // Walking transitions
            new(Walking, Jumping,   () => PlayerInputs.Jump || jumpBuffer > 0),
            new(Walking, Falling,   () => !collisions.GroundCollision),
            new(Walking, Sliding,   () => PlayerInputs.Slide),

            // Falling transitions
            new(Falling, Jumping,   () => PlayerInputs.Jump && hfsm.PreviousState == Walking && hfsm.Duration <= coyoteTime),
            new(Falling, SlideJump, () => PlayerInputs.Jump && hfsm.PreviousState == Sliding && hfsm.Duration <= coyoteTime),
            new(Falling, Walking,   () => collisions.GroundCollision && !PlayerInputs.Slide && hfsm.Duration >= 0.1f),
            new(Falling, Sliding,   () => collisions.GroundCollision && PlayerInputs.Slide  && hfsm.Duration >= 0.1f),

            // Jumping transitions
            new(Jumping, Falling,   () => hfsm.Duration > jumpGraceTime),

            // Sliding transitions   
            new(Sliding, SlideJump, () => (PlayerInputs.Jump || jumpBuffer > 0) && collisions.GroundCollision),
            new(Sliding, Falling,   () => !collisions.GroundCollision),
            new(Sliding, Walking,   () => !PlayerInputs.Slide && collisions.GroundCollision),

            // Slide jump transitions
            new(SlideJump, Falling, () => !PlayerInputs.Slide || hfsm.Duration > jumpGraceTime),
            new(SlideJump, Walking, () => !PlayerInputs.Slide && collisions.GroundCollision && hfsm.Duration >= jumpGraceTime),
            new(SlideJump, Sliding, () => PlayerInputs.Slide  && collisions.GroundCollision && hfsm.Duration >= jumpGraceTime),

            // Latching transitions
            new(null,     Latching, () => (PlayerInputs.Jump || jumpBuffer > 0) && reticle.Closest.obj != null),
            new(Latching, Falling,  () => latchFinished || reticle.Closest.obj == null),

            // Lunging transitions
            new(Falling, Lunging, () => (PlayerInputs.Jump || jumpBuffer > 0) && hfsm.PreviousState == Latching),
            new(Lunging, Falling, () => hfsm.Duration > jumpGraceTime),

            // Wall Running transitions
            new(null,    WallRun,   () => collisions.WallCollision && AllowWallRun),
            new(WallRun, WallJump,  () => collisions.WallCollision && PlayerInputs.Jump),
            new(WallRun, Falling,   () => !collisions.WallCollision || !AllowWallRun),

            // Wall Jumping transitions
            new(WallJump, Falling,  () => hfsm.Duration >= wallJumpTime),
        });

        hfsm.SetStartState(Walking);

        cam.Reload();
        capsuleCollider.enabled = true;
        slideBoost = true;

        Launch(cam.ForwardNoY * moveSpeed);
    }

    private void OnDisable()
    {
        DesiredHorizontalVelocity = rb.linearVelocity = Vector3.zero;
        capsuleCollider.enabled   = false;
    }

    private void Update()
    {
        hfsm.CheckTransitions();
        hfsm.Update();

        jumpBuffer -= Time.deltaTime;

        cam.ViewTilt();
        if (hfsm.CurrentState != Latching) reticle.CheckReticle();
    }

    private void FixedUpdate()
    {
        collisions.ChangeSize(PlayerInputs.Slide ? crouchSize : standardSize, crouchTime);

        collisions.CheckGroundCollisions();
        collisions.CheckWallCollisions();

        hfsm.FixedUpdate();
    }

    private void OnGUI()
    {
        hfsm.OnGUI();
        GUILayout.BeginArea(new Rect(10, 150, 800, 200));

        string current = $"Current Velocity: { rb.linearVelocity }\nCurrent Magnitude: { rb.linearVelocity.magnitude }";
        GUILayout.Label($"<size=15>{current}</size>");
        GUILayout.EndArea();
    }

    private void Move(bool keepMomentum)
    {
        float speed = keepMomentum ? Mathf.Max(moveSpeed, new Vector3(rb.linearVelocity.x, 0, rb.linearVelocity.z).magnitude) : moveSpeed;
        float accel = collisions.GroundCollision ? acceleration : airAcceleration;

        if (HorizontalVelocity.magnitude > moveSpeed) accel = airAcceleration;

        DesiredHorizontalVelocity = Vector3.MoveTowards(DesiredHorizontalVelocity, MoveDir * speed, Time.deltaTime * accel);

        Vector3 set = new(DesiredHorizontalVelocity.x, rb.linearVelocity.y, DesiredHorizontalVelocity.z);

        // Add in better momentum

        if (hfsm.CurrentState == Walking && collisions.SlopeCollision)
        {
            set.y = 0;
            set = Quaternion.FromToRotation(Vector3.up, collisions.GroundNormal) * set;
        }

        rb.linearVelocity = set;
    }

    public void Launch(Vector3 force)
    {
        hfsm.ChangeState(Falling);
        collisions.ResetCollisions();

        cam.FOVPulse(launchFOVPulse);

        rb.linearVelocity = new Vector3(rb.linearVelocity.x + force.x, force.y, rb.linearVelocity.z + force.z);
        DesiredHorizontalVelocity += new Vector3(force.x, 0, force.z);
    }

    private void Gravity()
    {
        rb.linearVelocity -= Time.deltaTime * gravity * rb.transform.up;
    }

    public void ResetVelocity()
    {
        AudioManager.Instance.PlaySFX(die);

        hfsm.ChangeState(Falling);
        collisions.ResetCollisions();

        DesiredHorizontalVelocity = MoveDir.normalized * moveSpeed;
        rb.linearVelocity         = DesiredHorizontalVelocity;

        slideBoost = true;
    }

    public void SetActive(bool on)
    {
        collisions.enabled = on;
        reticle.enabled = on;

        Camera.LockCamera = !on;
        Camera.CamComponent.GetComponent<AudioListener>().enabled = on;
    }

    private void SetSlideTilt() => cam.AddTilt(Mathf.Sign(-PlayerInputs.Input.x) * slideTilt);
    private void SetWallTilt()  => cam.AddTilt(Mathf.Sign(Vector3.Dot(Camera.CamComponent.transform.right, (collisions.WallPos - Camera.CamComponent.transform.position).normalized)) * wallRunTilt);
    private void ResetTilt()    => cam.AddTilt(0);
}