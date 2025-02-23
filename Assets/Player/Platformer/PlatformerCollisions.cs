using System.Collections;
using UnityEngine;

public class PlatformerCollisions : MonoBehaviour
{
    [Header("Refs")]
    [SerializeField] private Rigidbody rb;
    [SerializeField] private CapsuleCollider CapsuleCollider;

    [Header("Ground")]
    [SerializeField] private LayerMask groundLayers;
    [SerializeField] private float groundCastDist;
    [SerializeField] private float groundCastRadius;

    [Header("Wall")]
    [SerializeField] private int wallCastIncrements;
    [SerializeField] private float wallCastDistance;

    private bool groundCollision = false;
    private bool slopeCollision  = false;
    private bool wallCollision   = false;
    private Coroutine sizeChange;

    private Vector3 groundNormal;
    private Vector3 groundPoint;
    private Vector3 wallNormal;
    private Vector3 wallPos;

    public bool GroundCollision => groundCollision;
    public bool SlopeCollision  => slopeCollision;
    public bool WallCollision   => wallCollision;

    public Vector3 GroundNormal => groundNormal;
    public Vector3 GroundPoint  => groundPoint;
    public Vector3 WallNormal   => wallNormal;
    public Vector3 WallPos      => wallPos;

    public float Size
    {
        get
        {
            return CapsuleCollider.height;
        }
        set
        {
            float start = CapsuleCollider.height;
            float hD = value - start;

            CapsuleCollider.height += hD;
            if (GroundCollision) rb.MovePosition(rb.position + Vector3.up * (hD / 2.0f));
        }
    }

    public void ChangeSize(float endSize, float speed)
    {
        if (sizeChange != null) StopCoroutine(sizeChange);
        sizeChange = StartCoroutine(ChangeSizeCoroutine(endSize, speed));
    }

    private IEnumerator ChangeSizeCoroutine(float endSize, float speed)
    {
        while (Mathf.Abs(Size - endSize) > 0.01f)
        {
            Size = Mathf.MoveTowards(Size, endSize, Time.unscaledDeltaTime * speed);
            yield return new WaitForFixedUpdate();
        }

        Size = endSize;
    }

    public void CheckGroundCollisions()
    {
        float castDist = groundCastDist * (Size / 2.0f);

        if (Physics.SphereCast(rb.transform.position, castDist, Vector3.down, out RaycastHit interpolated, groundCastRadius, groundLayers))
        {
            Vector3 dir = (interpolated.point - rb.transform.position).normalized;

            if (Physics.Raycast(rb.transform.position, dir, out RaycastHit nonInterpolated, dir.magnitude + 0.01f, groundLayers))
            {
                if (Vector3.Angle(Vector3.up, nonInterpolated.normal) >= 80)
                {
                    groundNormal = rb.transform.up;
                }
                else
                {
                    groundNormal = nonInterpolated.normal;
                }
            }

            float angle = Vector3.Angle(Vector3.up, groundNormal);
            groundPoint = interpolated.point;
            groundCollision = true;
            slopeCollision = angle > 0 && angle <= 75;
            return;
        }

        groundNormal = rb.transform.up;
        ResetCollisions();
    }

    public void CheckWallCollisions()
    {
        float P2 = Mathf.PI * 2 / wallCastIncrements;

        Vector3 combined = Vector3.zero;

        for (int i = 0; i < wallCastIncrements; ++i)
        {
            Vector3 dir = new Vector3(Mathf.Cos(P2 * i), 0, Mathf.Sin(P2 * i)).normalized;

            if (Physics.Raycast(rb.position, dir, out RaycastHit hit, wallCastDistance, groundLayers))
            {
                combined += hit.normal;
                wallPos = hit.point;
            }
        }

        if (combined == Vector3.zero)
        {
            wallCollision = false;
            return;
        }

        wallCollision = true;
        wallNormal = combined.normalized;
    }

    public void ResetCollisions()
    {
        groundCollision = false;
        slopeCollision  = false;
        wallCollision   = false;
    }
}
