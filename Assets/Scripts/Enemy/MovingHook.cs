using UnityEngine;

public class MovingHook : Latchable
{
    [SerializeField] private Transform startPosition;
    [SerializeField] private Transform endPosition;

    [Header("Params")]
    [SerializeField] private Rigidbody attached;
    [SerializeField] private float moveSpeed;
    private Transform desiredPos;

    private void Awake()
    {
        desiredPos = startPosition;
        attached.position = desiredPos.position;
    }

    private void FixedUpdate()
    {
        attached.MovePosition(Vector3.MoveTowards(attached.position, desiredPos.position, Time.unscaledDeltaTime * moveSpeed));
    }

    public override void Latch(HomunculusController controller)
    {
        desiredPos = desiredPos == startPosition ? endPosition : startPosition;
        PlayerManager.Instance.Transitions.IdleSnap();
    }
}
