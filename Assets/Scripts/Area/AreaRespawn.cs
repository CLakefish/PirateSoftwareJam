using UnityEngine;

public class AreaRespawn : MonoBehaviour
{
    [SerializeField] private Area parent;

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        parent.SetPosition();
    }
}
