using UnityEngine;

public class Area : MonoBehaviour
{
    [SerializeField] private Transform spawnPosition;
    [SerializeField] private AreaEnd   endPosition;

    public Transform SpawnPosition => spawnPosition;

    private void Awake()
    {
        endPosition.SetParent(this);
    }

    public void Trigger()
    {
        AreaTransitionManager.Instance.TransitionTo(this);
    }

    public void EndTrigger()
    {
        AreaTransitionManager.Instance.TransitionFrom(this);
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawSphere(spawnPosition.position, 1);

        Gizmos.color = Color.red;
        Gizmos.DrawRay(spawnPosition.position, spawnPosition.forward * 2);
    }
}
