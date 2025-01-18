using UnityEngine;

public class AreaEnd : MonoBehaviour
{
    private Area parent;

    public void SetParent(Area parent) => this.parent = parent;

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        parent.EndTrigger();
    }
}
