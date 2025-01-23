using UnityEngine;
using UnityEngine.Events;

public class AreaEnd : MonoBehaviour
{
    [SerializeField] private GameObject spawned;
    [SerializeField] private UnityEvent onTrigger;
    private Area parent;

    public void SetParent(Area parent) => this.parent = parent;

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        GameObject spawn = Instantiate(spawned, transform);
        spawn.transform.localPosition = Vector3.zero;
        onTrigger?.Invoke();

        parent.EndTrigger();
    }
}
