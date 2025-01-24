using System.Collections;
using UnityEngine;

public class PlatformerCollisions : MonoBehaviour
{
    [Header("Ground Collisions")]
    [SerializeField] private Rigidbody rb;
    [SerializeField] private CapsuleCollider CapsuleCollider;
    [SerializeField] private LayerMask groundLayers;
    [SerializeField] private float groundCastDist;
    [SerializeField] private float groundCastRadius;

    private bool groundCollision = false;
    private bool slopeCollision = false;
    private Coroutine sizeChange;

    private Vector3 groundNormal;
    private Vector3 groundPoint;

    public bool GroundCollision => groundCollision;
    public bool SlopeCollision  => slopeCollision;

    public Vector3 GroundNormal => groundNormal;
    public Vector3 GroundPoint  => groundPoint;

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
                    groundNormal = Vector3.up;
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

        groundNormal = Vector3.up;
        ResetCollisions();
    }

    public void ResetCollisions()
    {
        groundCollision = false;
        slopeCollision = false;
    }
}
