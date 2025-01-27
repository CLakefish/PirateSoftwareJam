using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

public class EnemyController : Latchable
{
    [SerializeField] private Area connected;
    [SerializeField] private GameObject fire;
    [SerializeField] private UnityEvent onTrigger;

    public bool HasTriggered { get; private set; }

    private void Awake()
    {
        HasTriggered = false;
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

    public override void Latch(HomunculusController controller)
    {
        if (HasTriggered) {
            slowTime = false;
            PlayerManager.Instance.Transitions.IdleSnap();
            return;
        }

        onTrigger?.Invoke();
        HasTriggered = true;
        connected.Trigger(this);
    }
}
