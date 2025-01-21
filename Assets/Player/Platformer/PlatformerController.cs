using HFSMFramework;
using UnityEngine;

public class PlatformerController : PlayerManager.PlayerController
{
    private class WalkingState : State<PlatformerController>
    {
        public WalkingState(PlatformerController context) : base(context) { }

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
            momentum = context.rb.linearVelocity;

            if (context.slideBoost && context.HorizontalVelocity.magnitude <= context.slideForce)
            {
                if (context.PlayerInputs.IsInputting) context.cam.FOVPulse(context.slidePulse);

                if (context.collisions.SlopeCollision)
                {
                    Vector3 dir = (context.MoveDir * context.slideForce) + (context.gravity * Time.fixedDeltaTime * Vector3.down);
                    momentum = Quaternion.FromToRotation(Vector3.up, context.collisions.GroundNormal) * dir;
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
                float normalizedAngle = Vector3.Angle(Vector3.up, context.collisions.GroundNormal) / 90.0f;
                Vector3 adjusted = slopeGravity * (1.0f + normalizedAngle);
                desiredVelocity = momentum + adjusted;
            }
            else
            {
                desiredVelocity = new Vector3(context.MoveDir.x, momentum.y, context.MoveDir.z);
            }

            // Move towards gathered velocity (y is seperated so I can tinker with it more, it can be simplified ofc)
            Vector3 slideInterpolated = new(
                Mathf.MoveTowards(momentum.x, desiredVelocity.x, Time.fixedDeltaTime * context.slideAcceleration),
                Mathf.MoveTowards(momentum.y, desiredVelocity.y, Time.fixedDeltaTime * context.slideSlopeMomentumGain),
                Mathf.MoveTowards(momentum.z, desiredVelocity.z, Time.fixedDeltaTime * context.slideAcceleration));

            momentum = slideInterpolated;

            if (context.MoveDir != Vector3.zero)
            {
                Vector3 desiredDirection = Vector3.ProjectOnPlane(context.MoveDir.normalized, context.collisions.GroundNormal).normalized;
                Vector3 slopeDirection   = Vector3.ProjectOnPlane(Vector3.down, context.collisions.GroundNormal).normalized;

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
                    momentum.y = context.rb.linearVelocity.y;
                }
            }

            context.rb.linearVelocity = momentum;
            context.DesiredHorizontalVelocity = new Vector2(momentum.x, momentum.z);
        }

        public override void Exit()
        {
            context.rb.linearVelocity = new Vector3(momentum.x, Mathf.Min(momentum.y, 0), momentum.z);
            context.DesiredHorizontalVelocity = new Vector2(momentum.x, momentum.z);
        }
    }

    private class SlideJumping : State<PlatformerController>
    {
        public SlideJumping(PlatformerController context) : base(context) { }

        public override void Enter()
        {
            context.cam.FOVPulse(context.slideJumpPulse);

            context.collisions.ResetCollisions();

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

    [Header("References")]
    [SerializeField] private PlayerCamera cam;
    [SerializeField] private PlatformerCollisions collisions;
    [SerializeField] private PlatformerAnimator animator;

    [Header("Physics")]
    [SerializeField] private Rigidbody rb;
    [SerializeField] private CapsuleCollider capsuleCollider;

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

    private readonly float slideDotMin = -0.25f;
    private readonly float jumpGraceTime = 0.1f;

    public Rigidbody          Rigidbody => rb;
    public PlayerCamera       Camera    => cam;
    public PlatformerAnimator Animator  => animator;

    private WalkingState Walking   { get; set; }
    private JumpingState Jumping   { get; set; }
    private FallingState Falling   { get; set; }
    private SlidingState Sliding   { get; set; }
    private SlideJumping SlideJump { get; set; }

    private StateMachine<PlatformerController> hfsm { get; set; }

    private Vector3 HorizontalVelocity {
        get {
            return new Vector3(rb.linearVelocity.x, 0, rb.linearVelocity.z);
        }
    }

    private Vector3 MoveDir {
        get {
            return (cam.ForwardNoY * PlayerInputs.Input.normalized.y + cam.RightNoY * PlayerInputs.Input.normalized.x).normalized;
        }
    }

    private Vector2 DesiredHorizontalVelocity;
    private float jumpBuffer;
    private bool slideBoost = true;

    private void OnEnable()
    {
        hfsm = new(this);
        Walking = new(this);
        Jumping = new(this);
        Falling = new(this);
        Sliding = new(this);
        SlideJump = new(this);

        hfsm.AddTransitions(new()
        {
            new(Walking, Jumping,   () => PlayerInputs.Jump || jumpBuffer > 0),
            new(Walking, Falling,   () => !collisions.GroundCollision),
            new(Walking, Sliding,   () => PlayerInputs.Slide),

            new(Falling, Jumping,   () => PlayerInputs.Jump && hfsm.PreviousState == Walking && hfsm.Duration <= coyoteTime),
            new(Falling, SlideJump, () => PlayerInputs.Jump && hfsm.PreviousState == Sliding && hfsm.Duration <= coyoteTime),
            new(Falling, Walking,   () => collisions.GroundCollision && !PlayerInputs.Slide),
            new(Falling, Sliding,   () => collisions.GroundCollision && PlayerInputs.Slide),

            new(Jumping, Falling,   () => rb.linearVelocity.y < 0 && hfsm.Duration > jumpGraceTime),

            // Sliding transitions   
            new(Sliding, SlideJump, () => (PlayerInputs.Jump || jumpBuffer > 0) && collisions.GroundCollision),
            new(Sliding, Falling,   () => !collisions.GroundCollision),
            new(Sliding, Walking,   () => !PlayerInputs.Slide && collisions.GroundCollision),

            // Slide jump transitions
            new(SlideJump, Falling, () => ((!PlayerInputs.Slide) || (rb.linearVelocity.y < 0)) && hfsm.Duration > jumpGraceTime),
            new(SlideJump, Walking, () => !PlayerInputs.Slide && collisions.GroundCollision && hfsm.Duration >= jumpGraceTime),
            new(SlideJump, Sliding, () => PlayerInputs.Slide  && collisions.GroundCollision && hfsm.Duration >= jumpGraceTime),
        });

        hfsm.SetStartState(Falling);

        capsuleCollider.enabled = true;
    }

    private void OnDisable()
    {
        rb.linearVelocity = Vector3.zero;
        capsuleCollider.enabled = false;
    }

    private void Update()
    {
        hfsm.CheckTransitions();
        hfsm.Update();

        collisions.ChangeSize(PlayerInputs.Slide ? crouchSize : standardSize, crouchTime);

        jumpBuffer -= Time.deltaTime;

        cam.ViewTilt();
    }

    private void FixedUpdate()
    {
        collisions.CheckGroundCollisions();
        hfsm.FixedUpdate();
    }

    private void Gravity() => rb.linearVelocity -= Time.deltaTime * gravity * Vector3.up;

    private void Move(bool keepMomentum)
    {
        float speed = keepMomentum ? Mathf.Max(moveSpeed, HorizontalVelocity.magnitude) : moveSpeed;
        float accel = acceleration;

        DesiredHorizontalVelocity = Vector2.MoveTowards(DesiredHorizontalVelocity, new Vector2(MoveDir.x, MoveDir.z) * speed, Time.deltaTime * accel);

        Vector3 set = new(DesiredHorizontalVelocity.x, rb.linearVelocity.y, DesiredHorizontalVelocity.y);

        if (hfsm.CurrentState == Walking && collisions.SlopeCollision) {
            set.y = 0;
            set = Quaternion.FromToRotation(Vector3.up, collisions.GroundNormal) * set;
        }

        rb.linearVelocity = set;
    }
}
