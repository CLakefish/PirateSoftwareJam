using UnityEngine;

public class MindPortalTrigger : MonoBehaviour
{
    public System.Action OnTrigger;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player")) OnTrigger?.Invoke();
    }
}
