using UnityEngine;

public class Spring : MonoBehaviour
{
    [SerializeField] private Vector3 dir;
    [SerializeField] private float verticalForce;

    private void OnTriggerEnter(Collider other)
    {
        if (other.transform.TryGetComponent(out PlatformerController movement))
        {
            movement.Launch(dir.normalized * verticalForce);
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawRay(transform.position, dir.normalized * verticalForce);
    }
}
