using UnityEngine;
using UnityEngine.SceneManagement;

public class EnemyController : MonoBehaviour
{
    [SerializeField] private Area connected;
    [SerializeField] private GameObject fire;

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
        connected.Trigger(this);

        return true;
    }

    public void OnExit()
    {
        GameObject particle = Instantiate(fire, transform);
        particle.transform.localPosition = Vector3.up * 1.1f;
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.white;
        Gizmos.DrawLine(transform.position, connected.transform.position);
    }
}
