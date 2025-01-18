using UnityEngine;

public class PlatformerCollisions : MonoBehaviour
{
    [Header("Ground Collisions")]
    [SerializeField] private Rigidbody rb;
    [SerializeField] private LayerMask groundLayers;
    [SerializeField] private float groundCastDist;
    [SerializeField] private float groundCastRadius;

    private bool groundCollision = false;
    private bool slopeCollision = false;

    private Vector3 groundNormal;
    private Vector3 groundPoint;

    public bool GroundCollision => groundCollision;
    public bool SlopeCollision  => slopeCollision;

    public Vector3 GroundNormal => groundNormal;
    public Vector3 GroundPoint  => groundPoint;

    public void CheckGroundCollisions()
    {
        if (Physics.SphereCast(rb.transform.position, groundCastDist, Vector3.down, out RaycastHit interpolated, groundCastDist, groundLayers))
        {
            Vector3 dir = (interpolated.point - rb.transform.position).normalized;
            Vector3 desiredNormal = interpolated.normal;

            if (Physics.Raycast(rb.transform.position, dir, out RaycastHit nonInterpolated, dir.magnitude + 0.01f, groundLayers))
            {
                if (Vector3.Angle(Vector3.up, nonInterpolated.normal) >= 90)
                {
                    groundNormal = Vector3.up;
                }
                else
                {
                    groundNormal = nonInterpolated.normal;
                }
            }

            groundPoint = interpolated.point;
            groundCollision = true;
            slopeCollision = Vector3.Angle(Vector3.up, desiredNormal) > 0;
            return;
        }

        groundNormal = Vector3.up;
        slopeCollision = false;
        groundCollision = false;
    }

    public void ResetCollisions()
    {
        groundCollision = false;
        slopeCollision = false;
    }
}
