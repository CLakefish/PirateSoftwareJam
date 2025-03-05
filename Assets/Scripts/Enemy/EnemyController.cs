using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class EnemyController : Latchable
{
    [SerializeField] private Area connected;
    [SerializeField] private GameObject fire;
    [SerializeField] private UnityEvent onTrigger;
    [SerializeField] private GameObject renderer;

    public bool HasTriggered { get; private set; }
    public bool HasCompleted;

    private GameObject particle;

    private void Awake()
    {
        HasTriggered = false;
        connected.gameObject.SetActive(false);
    }

    public void SetRendering(bool enabled)
    {
        renderer.SetActive(enabled);
    }

    public void BrainTrigger()
    {
        HasCompleted = true;
        if (particle != null) Destroy(particle.gameObject);
        particle = Instantiate(fire, renderer.transform);
        particle.transform.localPosition = Vector3.up * 0.5f;
    }

    public void ClearTrigger()
    {
        if (particle != null) Destroy(particle.gameObject);
        HasCompleted = false;
    }

    public void OnExit()
    {
        connected.gameObject.SetActive(false);
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.white;
        Gizmos.DrawLine(transform.position, connected.transform.position);
    }

    public override void Latch()
    {
        if (HasCompleted) {
            slowTime = false;
            PlayerManager.Instance.Transitions.Grab();
            return;
        }

        connected.gameObject.SetActive(true);

        onTrigger?.Invoke();
        HasTriggered = true;
        connected.Trigger(this);
    }
}
