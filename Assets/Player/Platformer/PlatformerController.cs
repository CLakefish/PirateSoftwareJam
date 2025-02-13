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

            context.rb.linearVelocity = new Vector3(context.rb.linearVelocity.x, 0, context.rb.linearVelocity.z);
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
            context.rb.linearVelocity = new Vector3(context.rb.linearVelocity.x, context.jumpHeight, context.rb.linearVelocity.z);
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
        }

        public override void FixedUpdate()
        {
            context.Gravity();
        }
    }

    private class SlidingState : State<PlatformerController>
    {
        private Vector3 momentum;
        public SlidingState(PlatformerController context) : base(context) { }

        public override void Enter()
        {
            AudioManager.Instance.PlaySFX(context.slide);

            momentum = context.rb.linearVelocity;

            if (context.slideBoost && context.HorizontalVelocity.magnitude <= context.slideForce)
            {
                if (context.PlayerInputs.IsInputting) context.cam.FOVPulse(context.slidePulse);

                if (context.collisions.SlopeCollision)
                {
                    Vector3 dir = (context.MoveDir * context.slideForce) + (context.gravity * Time.fixedDeltaTime * Vector3.down);
                    momentum = Quaternion.FromToRotation(context.rb.transform.up, context.collisions.GroundNormal) * dir;
                }
                else
                {
                    momentum = context.MoveDir * context.slideForce;
                }
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
            context.Gravity();

            Vector3 desiredVelocity;

            // Gaining momentum
            if (context.collisions.SlopeCollision)
            {
                // Get the gravity, angle, and current momentumDirection
                Vector3 slopeGravity = Vector3.ProjectOnPlane(Vector3.down * context.gravity, context.collisions.GroundNormal);
                float normalizedAngle = Vector3.Angle(context.rb.transform.up, context.collisions.GroundNormal) / 90.0f;
                Vector3 adjusted = slopeGravity * (1.0f + normalizedAngle);
                desiredVelocity = momentum + adjusted;
            }
            else
            {
                desiredVelocity = context.MoveDir;
            }

            // Move towards gathered velocity (y is seperated so I can tinker with it more, it can be simplified ofc)
            Vector3 slideInterpolated = new(
                Mathf.MoveTowards(momentum.x, desiredVelocity.x, Time.fixedDeltaTime * context.slideAcceleration),
                Mathf.MoveTowards(momentum.y, desiredVelocity.y, Time.fixedDeltaTime * context.slideSlopeMomentumGain),
                Mathf.MoveTowards(momentum.z, desiredVelocity.z, Time.fixedDeltaTime * context.slideAcceleration));

            momentum = slideInterpolated;

            if (context.MoveDir != Vector3.zero)
            {
                Vector3 desiredDirection = Vector3.ProjectOnPlane(context.MoveDir, context.collisions.GroundNormal).normalized;
                Vector3 slopeDirection = Vector3.ProjectOnPlane(Vector3.down, context.collisions.GroundNormal).normalized;

                if (Vector3.Dot(desiredDirection, slopeDirection) >= context.slideDotMin)
                {
                    Vector3 currentMomentum = context.collisions.SlopeCollision
                        ? Vector3.ProjectOnPlane(momentum, context.collisions.GroundNormal).normalized
                        : momentum;

                    Quaternion rotation = Quaternion.FromToRotation(currentMomentum, desiredDirection);
                    Quaternion interpolated = Quaternion.Slerp(Quaternion.identity, rotation, Time.fixedDeltaTime * context.slideRotationSpeed);

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
            context.rb.linearVelocity = momentum;
            context.DesiredHorizontalVelocity = new Vector3(momentum.x, 0, momentum.z);
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
            context.slideBoost = false;

            context.rb.linearVelocity = new Vector3(context.rb.linearVelocity.x, context.slideJumpForce, context.rb.linearVelocity.z);
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
            context.Move(true);
            context.Gravity();
        }
    }

    private class LatchingState : State<PlatformerController>
    {
        private Vector3 movePos, startVel;
        private Vector3 offset;

        public LatchingState(PlatformerController context) : base(context) { }

        public override void Enter()
        {
            context.latchFinished = false;

            startVel = context.rb.linearVelocity;
            movePos  = context.rb.position;
            context.rb.linearVelocity = Vector3.zero;

            var latchable = context.reticle.Closest.obj.GetComponent<Latchable>();
            latchable.Latch();

            context.reticle.Set(context.reticle.Closest.obj.gameObject);

            context.cam.Recoil(context.PlayerLatching.recoil);
            context.cam.FOVPulse(context.pulseFOV);

            context.PlayerLatching.InitLine(context.cam);
        }

        public override void Update()
        {
            context.PlayerLatching.InterpolateLine();
            context.rb.MovePosition(movePos);
        }

        public override void FixedUpdate()
        {
            Vector3 pos = context.reticle.Closest.obj.transform.position + offset;
            movePos = Vector3.Lerp(context.rb.position, pos, context.PlayerLatching.latchLerp.Evaluate(context.hfsm.Duration));

            context.latchFinished = Vector3.Distance(context.rb.position, pos) < context.launchMinDist;
        }

        public override void Exit()
        {
            context.cam.FOVPulse(context.pulseFOV);

            context.reticle.ResetPulse();
            context.PlayerLatching.SetActive(false);

            Vector3 dir = context.cam.CamComponent.transform.forward * Mathf.Max(context.launchEndForce, new Vector2(startVel.x, startVel.z).magnitude);
            context.rb.linearVelocity = dir + new Vector3(0, context.PlayerLatching.exitLaunch * Mathf.Sign(context.cam.CamComponent.transform.forward.y + 0.8f), 0);
            context.DesiredHorizontalVelocity = new Vector3(context.rb.linearVelocity.x, 0, context.rb.linearVelocity.z);
        }
    }

    private class LungingState : State<PlatformerController>
    {
        public LungingState(PlatformerController context) : base(context) { }
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
    [SerializeField] private float gravity;

    [Header("Jumping")]
    [SerializeField] private float jumpHeight;
    [SerializeField] private float coyoteTime;
    [SerializeField] private float jumpBufferTime;

    [Header("Sliding Parameters")]
    [SerializeField] private float slideForce;
    [SerializeField] private float slideRotationSpeed;
    [SerializeField] private float slideAcceleration;
    [SerializeField] private float slideSlopeMomentumGain;
    [SerializeField] private float slidePulse;

    [Header("Slide Jump Parameters")]
    [SerializeField] private float slideJumpForce;
    [SerializeField] private float slideJumpPulse;

    [Header("Size Change Parameters")]
    [SerializeField] private float crouchSize;
    [SerializeField] private float standardSize;
    [SerializeField] private float crouchTime;

    [Header("Latch VFX")]
    [SerializeField] private float launchEndForce;
    [SerializeField] private float launchMinDist = 0.6f;
    [SerializeField] private float pulseFOV;

    [Header("SFX")]
    [SerializeField] private AudioClip land;
    [SerializeField] private AudioClip jump;
    [SerializeField] private AudioClip slide;
    [SerializeField] private AudioClip step;
    [SerializeField] private AudioClip die;
    [SerializeField] private float     stepTime;

    private readonly float slideDotMin = -0.25f;
    private readonly float jumpGraceTime = 0.1f;

    public PlayerCamera  Camera    => cam;
    public PlayerReticle Reticle   => reticle;

    private WalkingState  Walking   { get; set; }
    private JumpingState  Jumping   { get; set; }
    private FallingState  Falling   { get; set; }
    private SlidingState  Sliding   { get; set; }
    private SlideJumping  SlideJump { get; set; }
    private LatchingState Latching  { get; set; }
    private LungingState  Lunging   { get; set; }

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

    private Vector3 DesiredHorizontalVelocity;
    private float jumpBuffer;
    private bool  slideBoost = true;
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

            // Latching transititions
            new(null,     Latching, () => PlayerInputs.Jump && reticle.Closest.obj != null),
            new(Latching, Falling,  () => latchFinished || reticle.Closest.obj == null),

            // Lunging transitions
            new(Lunging,  Falling,  () => true),
        });

        hfsm.SetStartState(Falling);

        cam.Reload();
        capsuleCollider.enabled = true;
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
        collisions.CheckGroundCollisions();
        collisions.ChangeSize(PlayerInputs.Slide ? crouchSize : standardSize, crouchTime);

        hfsm.FixedUpdate();
    }

    private void OnGUI()
    {
        hfsm.OnGUI();
    }

    private void Gravity()
    {
        rb.linearVelocity -= Time.deltaTime * gravity * rb.transform.up;
    }

    private void Move(bool keepMomentum)
    {
        float speed = keepMomentum ? Mathf.Max(moveSpeed, new Vector3(rb.linearVelocity.x, 0, rb.linearVelocity.z).magnitude) : moveSpeed;
        float accel = acceleration;

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

        rb.linearVelocity = new Vector3(rb.linearVelocity.x + force.x, force.y, rb.linearVelocity.z + force.z);
        DesiredHorizontalVelocity += new Vector3(force.x, 0, force.z);
    }

    public void ResetVelocity()
    {
        AudioManager.Instance.PlaySFX(die);

        hfsm.ChangeState(Falling);
        collisions.ResetCollisions();

        DesiredHorizontalVelocity = MoveDir.normalized * moveSpeed;
        rb.linearVelocity = DesiredHorizontalVelocity;
    }
}