using HFSMFramework;
using UnityEngine;

public class PlatformerController : MonoBehaviour
{
    private class WalkingState : State<PlatformerController>
    {
        public WalkingState(PlatformerController context) : base(context)
        {
        }

        public override void FixedUpdate()
        {
            context.rb.linearVelocity = new Vector3(context.rb.linearVelocity.x, 0, context.rb.linearVelocity.z);
            context.Move(context.hfsm.Duration > 0.1f);
        }
    }

    private class JumpingState : State<PlatformerController>
    {
        public JumpingState(PlatformerController context) : base(context)
        {
        }

        public override void Enter()
        {
            context.jumpBuffer = 0;
            context.rb.linearVelocity = new Vector3(context.rb.linearVelocity.x, context.jumpHeight, context.rb.linearVelocity.z);
        }

        public override void Update()
        {
            context.Move(true);

            if (context.input.Jump)
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
        public FallingState(PlatformerController context) : base(context)
        {
        }

        public override void Update()
        {
            context.Move(true);

            if (context.input.Jump)
            {
                context.jumpBuffer = context.jumpBufferTime;
            }
        }

        public override void FixedUpdate()
        {
            context.Gravity();
        }
    }

    [Header("References")]
    [SerializeField] private PlayerCamera         cam;
    [SerializeField] private PlayerInputManager   input;
    [SerializeField] private PlatformerCollisions collisions;

    [Header("Physics")]
    [SerializeField] private Rigidbody rb;

    [Header("Movement")]
    [SerializeField] private float moveSpeed;
    [SerializeField] private float acceleration;
    [SerializeField] private float gravity;

    [Header("Jumping")]
    [SerializeField] private float jumpHeight;
    [SerializeField] private float coyoteTime;
    [SerializeField] private float jumpBufferTime;

    private WalkingState Walking;
    private JumpingState Jumping;
    private FallingState Falling;

    private StateMachine<PlatformerController> hfsm;

    private Vector3 HorizontalVelocity {
        get {
            return new Vector3(rb.linearVelocity.x, 0, rb.linearVelocity.z);
        }
    }

    private Vector3 MoveDir {
        get {
            return (cam.ForwardNoY * input.Input.normalized.y + cam.RightNoY * input.Input.normalized.x).normalized;
        }
    }

    private Vector2 DesiredHorizontalVelocity;
    private float jumpBuffer;

    private void OnEnable()
    {
        hfsm = new(this);
        Walking = new(this);
        Jumping = new(this);
        Falling = new(this);

        hfsm.AddTransitions(new()
        {
            new(Walking, Jumping, () => input.Jump || jumpBuffer > 0),
            new(Walking, Falling, () => !collisions.GroundCollision),

            new(Falling, Jumping, () => input.Jump && hfsm.Duration <= coyoteTime && hfsm.PreviousState == Walking),
            new(Falling, Walking, () => collisions.GroundCollision),

            new(Jumping, Falling, () => rb.linearVelocity.y < 0 && hfsm.Duration > 0.1f),
        });

        hfsm.SetStartState(Falling);
    }

    private void Update()
    {
        hfsm.CheckTransitions();
        hfsm.Update();

        jumpBuffer -= Time.deltaTime;
    }

    private void FixedUpdate()
    {
        collisions.CheckGroundCollisions();
        hfsm.FixedUpdate();
    }

    private void OnGUI()
    {
        hfsm.OnGUI();
    }

    private void Gravity() => rb.linearVelocity -= Time.deltaTime * gravity * Vector3.up;

    private void Move(bool keepMomentum)
    {
        float speed = keepMomentum ? Mathf.Max(moveSpeed, HorizontalVelocity.magnitude) : moveSpeed;
        float accel = acceleration;

        DesiredHorizontalVelocity = Vector2.MoveTowards(DesiredHorizontalVelocity, new Vector2(MoveDir.x, MoveDir.z) * speed, Time.deltaTime * accel);

        Vector3 set = new(DesiredHorizontalVelocity.x, rb.linearVelocity.y, DesiredHorizontalVelocity.y);

        if (hfsm.CurrentState == Walking && collisions.SlopeCollision) {
            set = Quaternion.FromToRotation(Vector3.up, collisions.GroundNormal) * set;
        }

        rb.linearVelocity = set;
    }
}
