using UnityEngine;

public class DialogueCollisionTrigger : DialogueTrigger
{
    private bool hasTriggered = false;

    private void OnTriggerEnter(Collider other)
    {
        if (hasTriggered) return;
        hasTriggered = true;
        TriggerDialogue();
    }

    private void OnTriggerStay(Collider other)
    {
        if (hasTriggered) return;
        hasTriggered = true;
        TriggerDialogue();
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = new Color(1, 0.92f, 0.016f, 0.5f);
        Gizmos.DrawCube(transform.position, GetComponent<MeshRenderer>().bounds.size);
    }
}
