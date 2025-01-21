using UnityEngine;
using UnityEngine.SceneManagement;

public class EnemyController : MonoBehaviour
{
    [SerializeField] private Area connected;

    public bool HasTriggered { get; private set; }

    private void Awake()
    {
        HasTriggered = false;
    }

    public bool Trigger() {
        if (HasTriggered) {
            return false;
        }

        HasTriggered = true;
        connected.Trigger();

        return true;
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.white;
        Gizmos.DrawLine(transform.position, connected.transform.position);
    }
}
