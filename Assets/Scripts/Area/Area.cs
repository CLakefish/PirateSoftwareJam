using UnityEngine;

public class Area : MonoBehaviour
{
    [SerializeField] private Transform spawnPosition;
    [SerializeField] private AreaEnd   endPosition;

    public Transform SpawnPosition => spawnPosition;
    private bool hasTriggered = false;

    private void Awake()
    {
        endPosition.SetParent(this);
    }

    public void Trigger()
    {
        hasTriggered = false;
        PlayerManager.Instance.Transitions.ToPlayer(this);
    }

    public void EndTrigger()
    {
        if (hasTriggered) return;
        hasTriggered = true;
        PlayerManager.Instance.Transitions.ToHomunculus();
    }

    public void SetPosition()
    {
        PlayerManager.Instance.Transitions.SetPlayer(SpawnPosition);
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawSphere(spawnPosition.position, 1);

        Gizmos.color = Color.red;
        Gizmos.DrawRay(spawnPosition.position, spawnPosition.forward * 2);
    }
}
